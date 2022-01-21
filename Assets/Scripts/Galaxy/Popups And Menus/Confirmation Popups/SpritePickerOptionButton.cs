using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpritePickerOptionButton : MonoBehaviour
{
    [Header("Sprite Picker Option Button Components")]

    [SerializeField] private Image image = null;
    [SerializeField] private Image backgroundImage = null;

    [SerializeField] private Text spriteNameText = null;

    [Header("Sprite Picker Option Button SFX Options")]

    [SerializeField] private AudioClip clickSFX = null;

    //Non-inspector variables.

    public Sprite Sprite
    {
        get
        {
            return image.sprite;
        }
        set
        {
            image.sprite = value;
            image.gameObject.SetActive(image.sprite != null);
            //Resizes the image to match the dimensions of the given sprite.
            if(value != null)
            {
                //Determines the biggest size a dimension (x and y) of the image can be.
                float biggestImageDimensionSize = image.rectTransform.rect.width > image.rectTransform.rect.height ? image.rectTransform.rect.width : image.rectTransform.rect.height;

                //If pixel width and pixel height of the given sprite is the same.
                if (value.rect.width == value.rect.height)
                {
                    image.rectTransform.sizeDelta = new Vector2(biggestImageDimensionSize, biggestImageDimensionSize);
                }

                //If pixel width is greater than the pixel height for the given sprite.
                else if (value.rect.width > value.rect.height)
                {
                    image.rectTransform.sizeDelta = new Vector2(biggestImageDimensionSize, (value.rect.height / value.rect.width) * biggestImageDimensionSize);
                }

                //If the pixel width is less than the pixel height for the given sprite.
                else if (value.rect.width < value.rect.height)
                {
                    image.rectTransform.sizeDelta = new Vector2((value.rect.width / value.rect.height) * biggestImageDimensionSize, biggestImageDimensionSize);
                }
            }
        }
    }
    public Color SpriteColor { get => image.color; set => image.color = value; }
    public string SpriteName { get => spriteNameText.text; set => spriteNameText.text = value == null ? string.Empty : value; }

    public bool Selected
    {
        set
        {
            backgroundImage.color = value ? new Color(68 / 255.0f, 99 / 255.0f, 93 / 255.0f, 1) : new Color(82 / 255.0f, 137 / 255.0f, 126 / 255.0f, 1);
        }
    }

    private GalaxySpritePickerConfirmationPopup assignedConfirmationPopup = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //This method is called whenever the sprite picker option button is clicked.
    public void OnPointerClick()
    {
        assignedConfirmationPopup.SetSpriteSelected(transform.GetSiblingIndex());

        //Plays the appropriate sound effect for the button being clicked.
        AudioManager.PlaySFX(clickSFX);
    }

    //This method should be called in order to create a new sprite picker option button.
    public static SpritePickerOptionButton CreateSpritePickerOptionButton(GameObject prefab, Transform parent, GalaxySpritePickerConfirmationPopup assignedConfirmationPopup, Sprite sprite, Color defaultSpriteColor, string spriteName = null)
    {
        GameObject spritePickerOptionButton = Instantiate(prefab);
        SpritePickerOptionButton spritePickerOptionButtonScript = spritePickerOptionButton.GetComponent<SpritePickerOptionButton>();

        spritePickerOptionButtonScript.transform.SetParent(parent);
        spritePickerOptionButtonScript.transform.localScale = Vector3.one;

        spritePickerOptionButtonScript.assignedConfirmationPopup = assignedConfirmationPopup;

        spritePickerOptionButtonScript.Sprite = sprite;
        spritePickerOptionButtonScript.SpriteColor = defaultSpriteColor;
        spritePickerOptionButtonScript.SpriteName = spriteName;

        return spritePickerOptionButtonScript;
    }
}
