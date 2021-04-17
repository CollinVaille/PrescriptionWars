using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCreationMenu : GalaxyMenuBehaviour
{
    [Header("Flag Creation Menu")]

    public List<Sprite> flagSymbols;

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

    [ReadOnly, SerializeField] private int symbolSelected = 0;
    public int SymbolSelected
    {
        get
        {
            return symbolSelected;
        }
        set
        {
            symbolSelected = value;
            flagSymbolImage.sprite = flagSymbols[SymbolSelected];
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

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //Don't Remove
        flagSymbolImage.sprite = flagSymbols[symbolSelected];
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

    public void ClickSymbolSelectedBackArrow()
    {
        SymbolSelected = SymbolSelected == 0 ? flagSymbols.Count - 1 : SymbolSelected - 1;
        PlayClickButtonSFX();
    }

    public void ClickSymbolSelectedNextArrow()
    {
        SymbolSelected = SymbolSelected == flagSymbols.Count - 1 ? 0 : SymbolSelected + 1;
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
