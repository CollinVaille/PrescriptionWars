using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCreationMenu : GalaxyMenuBehaviour
{
    [Header("Flag Creation Menu")]

    [SerializeField]
    private Image flagBackgroundImage = null;
    [SerializeField]
    private Image flagSymbolImage = null;

    [SerializeField]
    private FlexibleColorPicker symbolColorPicker = null;
    [SerializeField]
    private FlexibleColorPicker backgroundColorPicker = null;

    [SerializeField]
    private AudioClip hoverOverDropdownOptionSFX = null;
    [SerializeField]
    private AudioClip clickDropdownOptionSFX = null;
    [SerializeField]
    private AudioClip hoverButtonSFX = null;
    [SerializeField]
    private AudioClip clickButtonSFX = null;

    [Header("Additional Information")]

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private int symbolSelected = 0;
    public int SymbolSelected
    {
        get
        {
            return symbolSelected;
        }
        set
        {
            symbolSelected = value;
            flagSymbolImage.sprite = Resources.Load<Sprite>("Flag Symbols/" + FlagDataLoader.flagSymbolNames[SymbolSelected]);
        }
    }

    private Color flagBackgroundColor = Color.white;
    public Color FlagBackgroundColor
    {
        get
        {
            return flagBackgroundColor;
        }
        set
        {
            flagBackgroundColor = value;
            flagBackgroundImage.color = FlagBackgroundColor;
        }
    }
    private Color flagSymbolColor = Color.white;
    public Color FlagSymbolColor
    {
        get
        {
            return flagSymbolColor;
        }
        set
        {
            flagSymbolColor = value;
            flagSymbolImage.color = FlagSymbolColor;
        }
    }

    //Non-inspector variables.

    public Flag EmpireFlag
    {
        get
        {
            return new Flag(SymbolSelected, FlagBackgroundColor, FlagSymbolColor);
        }
    }

    //Indicates whether a popup is hovering over and blocking the back arrow.
    private bool IsAPopupOverTheBackArrow
    {
        get
        {
            return (symbolColorPicker.transform.localPosition.x < -325 && symbolColorPicker.transform.localPosition.y > 90) || (backgroundColorPicker.transform.localPosition.x < -325 && backgroundColorPicker.transform.localPosition.y > 90);
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //Don't Remove
        flagSymbolImage.sprite = Resources.Load<Sprite>("Flag Symbols/" + FlagDataLoader.flagSymbolNames[SymbolSelected]);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (symbolColorPicker.gameObject.activeInHierarchy)
            FlagSymbolColor = symbolColorPicker.color;
        if (backgroundColorPicker.gameObject.activeInHierarchy)
            FlagBackgroundColor = backgroundColorPicker.color;
    }

    //Closes any color picker popups that might be open before it switches to the previous menu.
    public override void SwitchToPreviousMenu()
    {
        if (symbolColorPicker.gameObject.activeInHierarchy)
            symbolColorPicker.CloseWithoutSFX();
        if (backgroundColorPicker.gameObject.activeInHierarchy)
            backgroundColorPicker.CloseWithoutSFX();

        base.SwitchToPreviousMenu();
    }

    public override bool ShouldBackArrowBeDisabled()
    {
        return IsAPopupOverTheBackArrow;
    }

    public void ClickSymbolSelectedBackArrow()
    {
        SymbolSelected = SymbolSelected == 0 ? FlagDataLoader.flagSymbolNames.Length - 1 : SymbolSelected - 1;
        PlayClickButtonSFX();
    }

    public void ClickSymbolSelectedNextArrow()
    {
        SymbolSelected = SymbolSelected == FlagDataLoader.flagSymbolNames.Length - 1 ? 0 : SymbolSelected + 1;
        PlayClickButtonSFX();
    }

    public void ClickPickSymbolColorButton()
    {
        symbolColorPicker.Open();
        PlayClickButtonSFX();
    }

    public void ClickPickBackgroundColorButton()
    {
        backgroundColorPicker.Open();
        PlayClickButtonSFX();
    }

    public void PlayHoverOverDropdownOptionSFX()
    {
        AudioManager.PlaySFX(hoverOverDropdownOptionSFX);
    }

    public void PlayClickDropdownOptionSFX()
    {
        AudioManager.PlaySFX(clickDropdownOptionSFX);
    }

    public void PlayHoverButtonSFX()
    {
        AudioManager.PlaySFX(hoverButtonSFX);
    }

    public void PlayClickButtonSFX()
    {
        AudioManager.PlaySFX(clickButtonSFX);
    }
}

public class Flag
{
    public Flag()
    {

    }

    public Flag(int symbolSelected, Color backgroundColor, Color symbolColor)
    {
        this.symbolSelected = symbolSelected;
        this.backgroundColor = backgroundColor;
        this.symbolColor = symbolColor;
    }

    public int symbolSelected = 0;

    public Color backgroundColor = Color.white;
    public Color symbolColor = Color.white;
}
