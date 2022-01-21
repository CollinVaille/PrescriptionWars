using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxySpritePickerConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Sprite Picker Confirmation Popup Components")]

    [SerializeField] private Text bodyText = null;

    [SerializeField] private Transform scrollListContent = null;

    [Header("Sprite Picker Confirmation Popup Options")]

    [SerializeField] private GameObject spritePickerOptionButtonPrefab = null;

    //Non-inspector variables.

    private Sprite[] sprites = null;
    private string[] spriteNames = null;
    private Color defaultSpriteColor = Color.white;

    private int spriteSelected = 0;
    /// <summary>
    /// Returns the sprite that the user currently has selected.
    /// </summary>
    public Sprite SpriteSelected { get => sprites[spriteSelected]; }
    /// <summary>
    /// Returns the name of the sprite the user currently has selected.
    /// </summary>
    public string SpriteSelectedName { get => spriteNames[spriteSelected]; }
    /// <summary>
    /// Indicates the color that the images displaying the sprites on the sprite picker option buttons will have.
    /// </summary>
    public Color DefaultSpriteColor
    {
        get
        {
            return defaultSpriteColor;
        }
        set
        {
            defaultSpriteColor = value;

            //Updates the default sprite color for each sprite picker option button.
            if(scrollListContent.childCount > 0)
            {
                for(int spriteIndex = 0; spriteIndex < scrollListContent.childCount; spriteIndex++)
                {
                    scrollListContent.GetChild(spriteIndex).GetComponent<SpritePickerOptionButton>().SpriteColor = value;
                }
            }
        }
    }

    public static GameObject galaxySpritePickerConfirmationPopupPrefab = null;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    //This method should be called in order to properly create the confirmation popup and set the text components.
    public void CreateConfirmationPopup(string topText, string bodyText, Sprite[] sprites, Color defaultSpriteColor, string[] spriteNames = null)
    {
        CreateConfirmationPopup(topText);
        this.bodyText.text = bodyText;
        this.sprites = sprites;
        this.defaultSpriteColor = defaultSpriteColor;
        this.spriteNames = spriteNames;

        if (sprites != null)
            PopulateScrollList();
    }

    //This method should be called in order to populate the scroll list with sprite picker option buttons.
    private void PopulateScrollList()
    {
        for(int spriteIndex = 0; spriteIndex < sprites.Length; spriteIndex++)
        {
            SpritePickerOptionButton.CreateSpritePickerOptionButton(spritePickerOptionButtonPrefab, scrollListContent, this, sprites[spriteIndex], defaultSpriteColor, (spriteNames != null && spriteNames.Length > spriteIndex) ? spriteNames[spriteIndex] : null);
        }
    }

    //This method is called whenever a sprite picker option button is clicked and sets the sprite selected to the sprite at the specified index.
    public void SetSpriteSelected(int index)
    {
        scrollListContent.GetChild(spriteSelected).GetComponent<SpritePickerOptionButton>().Selected = false;
        spriteSelected = index;
        scrollListContent.GetChild(spriteSelected).GetComponent<SpritePickerOptionButton>().Selected = true;
    }

    //This method is called in order to set the sprite selected index to the index that the specified sprite is located at.
    public void SetSpriteSelected(Sprite sprite)
    {
        int spriteIndex = -1;
        for (int x = 0; x < sprites.Length; x++)
        {
            if (sprite == sprites[x])
            {
                spriteIndex = x;
                break;
            }
        }
        if (spriteIndex < 0)
            return;

        scrollListContent.GetChild(spriteSelected).GetComponent<SpritePickerOptionButton>().Selected = false;
        spriteSelected = spriteIndex;
        scrollListContent.GetChild(spriteSelected).GetComponent<SpritePickerOptionButton>().Selected = true;
    }
}
