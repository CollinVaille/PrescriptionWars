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
            flagSymbolImage.sprite = Resources.Load<Sprite>("General/Flag Symbols/" + FlagDataLoader.flagSymbolNames[SymbolSelected]);
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
        flagSymbolImage.sprite = Resources.Load<Sprite>("General/Flag Symbols/" + FlagDataLoader.flagSymbolNames[SymbolSelected]);
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

public class NewFlag
{
    /// <summary>
    /// Constructor that should be used to create a new flag for an empire for the first time.
    /// </summary>
    /// <param name="symbolName"></param>
    /// <param name="backgroundColor"></param>
    /// <param name="symbolColor"></param>
    public NewFlag(string symbolName, Color backgroundColor, Color symbolColor)
    {
        _symbolName = symbolName;
        _backgroundColor = backgroundColor;
        _symbolColor = symbolColor;

        UpdateSprite();
    }

    /// <summary>
    /// Constructor that should be used to recreate a flag for an empire based off of save data of a flag.
    /// </summary>
    /// <param name="flagData"></param>
    public NewFlag(FlagData flagData)
    {
        _symbolName = flagData.symbolName;
        _backgroundColor = new Color(flagData.backgroundColor[0], flagData.backgroundColor[1], flagData.backgroundColor[2], flagData.backgroundColor[3]);
        _symbolColor = new Color(flagData.symbolColor[0], flagData.symbolColor[1], flagData.symbolColor[2], flagData.symbolColor[3]);

        UpdateSprite();
    }

    /// <summary>
    /// Private holder variable for the string that contains the name of the symbol on the flag.
    /// </summary>
    private string _symbolName = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the symbol on the flag via a symbol name string.
    /// </summary>
    public string symbolName { get => _symbolName; set { _symbolName = value; spriteUpdated = false; } }

    /// <summary>
    /// Private holder variable for the flag's background color, which is the color behind the symbol color.
    /// </summary>
    private Color _backgroundColor = Color.white;
    /// <summary>
    /// Public property that should be used both to access and mutate the background color of the flag.
    /// </summary>
    public Color backgroundColor { get => _backgroundColor; set { _backgroundColor = value; spriteUpdated = false; } }

    /// <summary>
    /// Private holder variable for the flag's symbol color, which is the foreground color of the flag.
    /// </summary>
    private Color _symbolColor = Color.white;
    /// <summary>
    /// Public property that should be used both to access and mutate the symbol color of the flag.
    /// </summary>
    public Color symbolColor { get => _symbolColor; set { _symbolColor = value; spriteUpdated = false; } }

    /// <summary>
    /// Private holder variable for the combined flag sprite that contains both the symbol and the background of the flag colored correctly.
    /// </summary>
    private Sprite _sprite = null;
    /// <summary>
    /// Private holder variable that indicates whether or not the flag's sprite has been updated yet since it was most recently changed.
    /// </summary>
    private bool spriteUpdated = false;
    /// <summary>
    /// Public property that should be accessed in order to obtain the flag's sprite that should be applied to an image to represent the flag.
    /// </summary>
    public Sprite sprite { get { if (!spriteUpdated) UpdateSprite(); return _sprite; } }

    /// <summary>
    /// Private method that should be called whenever the sprite property is being accessed with the sprite variable having not been updated yet.
    /// </summary>
    private void UpdateSprite()
    {
        //Declares and initializes a new texture 2D.
        Texture2D newTex = new Texture2D(500, 300);

        //Loops through each pixel in the new texture and lerps the symbol color and background color together at the specified x and y based on the symbol sprite's alpha value.
        for(int x = 0; x < newTex.width; x++)
        {
            for(int y = 0; y < newTex.height; y++)
            {
                Color pixelColor = Resources.Load<Sprite>("General/Flag Symbols/" + _symbolName).texture.GetPixel(x, y);
                newTex.SetPixel(x, y, Color.Lerp(_backgroundColor, new Color(_symbolColor.r * pixelColor.r, _symbolColor.g * pixelColor.g, symbolColor.b * pixelColor.b, _symbolColor.a), pixelColor.a));
            }
        }

        //Applies the changes to the new texture.
        newTex.Apply();
        //Creates a new sprite from the new texture 2D.
        _sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
        _sprite.name = "Flag Sprite";
        //Logs that the flag's sprite variable has been updated.
        spriteUpdated = true;
    }
}

[System.Serializable]
public class FlagData
{
    public string symbolName = null;
    public float[] backgroundColor = null;
    public float[] symbolColor = null;

    public FlagData(NewFlag flag)
    {
        symbolName = flag.symbolName;

        backgroundColor = new float[4];
        backgroundColor[0] = flag.backgroundColor.r;
        backgroundColor[1] = flag.backgroundColor.g;
        backgroundColor[2] = flag.backgroundColor.b;
        backgroundColor[3] = flag.backgroundColor.a;

        symbolColor = new float[4];
        symbolColor[0] = flag.symbolColor.r;
        symbolColor[1] = flag.symbolColor.g;
        symbolColor[2] = flag.symbolColor.b;
        symbolColor[3] = flag.symbolColor.a;
    }
}