using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class GalaxyPopup : GalaxyPopupBehaviour
{
    [Header("Text Components")]

    [SerializeField]
    private Text headLineText = null;
    [SerializeField]
    private Text bodyText = null;
    [SerializeField]
    private List<Text> optionButtonTexts = new List<Text>();

    [Header("Image Components")]

    [SerializeField]
    private Image bodyImage = null;
    [SerializeField]
    private Image bodyImageMask = null;

    [Header("Button Components")]

    [SerializeField]
    private List<Button> optionButtons = null;

    [Header("SFX Options")]

    [SerializeField]
    private AudioClip specialOpenPopupSFX = null;
    [SerializeField]
    private AudioClip mouseOverOptionButton = null;
    [SerializeField]
    private AudioClip clickOptionButtonSFX = null;

    [Header("Additional Information")]

    [SerializeField]
    [ReadOnly] private GalaxyPopupSpriteFit spriteFit = 0;

    [SerializeField]
    [ReadOnly] private float spritePositionPercentage = 0;

    //Non-inspector variables.

    private List<GalaxyPopupOptionData> optionsData = new List<GalaxyPopupOptionData>();

    //Indicates the popup's index in the list of popups in the popup manager.
    private int popupIndex = 0;

    //Indicates whether the player is required to provide an answer for the popup to close.
    private bool answerRequired = false;
    //Indicates whether the special sound effect for whenever the popup opens has played or not.
    private bool specialOpenPopupSFXPlayed = false;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (!specialOpenPopupSFXPlayed)
        {
            AudioManager.PlaySFX(specialOpenPopupSFX);
            specialOpenPopupSFXPlayed = true;
        }
    }

    //Indicates whether the popup should close due to the player pressing escape with special parameters compared to the super class.
    public override bool ShouldClose()
    {
        return base.ShouldClose() && !answerRequired;
    }

    public void CreatePopup(GalaxyPopupData popupData, int indexOfPopup)
    {
        gameObject.name = popupData.headLine + " Popup";
        headLineText.text = popupData.headLine;
        SetBodyImageSprite(Resources.Load<Sprite>("Galaxy/" + popupData.spriteResourcesFilePath), popupData.spriteFit, popupData.spritePositionPercentage);
        bodyText.text = popupData.bodyText;
        specialOpenPopupSFX = GalaxyPopupManager.GetPopupSFXFromName(popupData.specialOpenSFXName);
        answerRequired = popupData.answerRequired;
        spriteFit = popupData.spriteFit;
        spritePositionPercentage = popupData.spritePositionPercentage;
        List<int> optionButtonsUsed = new List<int>();
        int optionsProcessed = 0;
        //Sets the data of all of the option buttons that will be used.
        for(int x = optionButtons.Count - popupData.options.Count; x < optionButtons.Count; x++)
        {
            optionButtonsUsed.Add(x);
            optionsData.Add(popupData.options[optionsProcessed]);
            optionButtonTexts[x].text = popupData.options[optionsProcessed].mainText;
            optionButtons[x].gameObject.GetComponent<GalaxyTooltip>().Text = popupData.options[optionsProcessed].effectDescriptionText;
            optionsProcessed++;
        }
        //Deactivates all option buttons that will not be used.
        for(int x = 0; x < optionButtons.Count; x++)
        {
            if (!optionButtonsUsed.Contains(x))
            {
                optionButtons[x].gameObject.SetActive(false);
            }
        }

        //Assigns the popup its appropriate index in the list of popups.
        popupIndex = indexOfPopup;
    }

    public void ChooseOption(int optionNumber)
    {
        if(IsOpeningAnimationDone())
        {
            int optionsProcessed = 0;
            for (int x = optionButtons.Count - optionsData.Count; x < optionButtons.Count; x++)
            {
                for (int y = 0; y < optionsData[optionsProcessed].effects.Count; y++)
                {
                    GalaxyPopupOptionEffect effect = optionsData[optionsProcessed].effects[y];
                    GalaxyPopupManager.ApplyPopupOptionEffect(effect);
                }
                optionsProcessed++;
            }
            AudioManager.PlaySFX(clickOptionButtonSFX);
            Close();
        }
    }

    //Gets the value of the boolean that indicates whether an answer is required for the popup to close.
    public bool IsAnswerRequired()
    {
        return answerRequired;
    }

    public override void Close()
    {
        //Executes the super classes's logic for whenever a popup needs to be closed.
        base.Close();

        //Executes the logic for whenever a popup of this type needs to be closed.
        GalaxyPopupManager.ClosePopup(popupIndex);
    }

    public void PointerEnterOptionButton(int buttonNum)
    {
        AudioManager.PlaySFX(mouseOverOptionButton);
    }

    public string GetHeadLine()
    {
        return headLineText.text;
    }

    public int GetPopupIndex()
    {
        return popupIndex;
    }

    public void SetPopupIndex(int index)
    {
        popupIndex = index;
    }

    private void SetBodyImageSprite(Sprite sprite, GalaxyPopupSpriteFit spriteFit, float spritePositionPercentage)
    {
        //Sets the sprite of the body image to the specified sprite.
        bodyImage.sprite = sprite;
        //Sets the size of the body image to the size of the specified sprite.
        bodyImage.rectTransform.sizeDelta = sprite.rect.size;
        //Makes sure that the sprite of the body image fits correctly.
        switch (spriteFit)
        {
            case GalaxyPopupSpriteFit.VerticalAndHorizontal:
                FitBodyImageSpriteVertically();
                FitBodyImageSpriteHorizontally();
                break;
            case GalaxyPopupSpriteFit.Vertical:
                FitBodyImageSpriteVertically();
                break;
            case GalaxyPopupSpriteFit.Horizontal:
                FitBodyImageSpriteHorizontally();
                break;

            default:
                Debug.LogWarning("Galaxy Popup Sprite Fit: " + spriteFit.ToString() + " does not have any logic implemented for it.");
                break;
        }
        //Updates the position of the sprite of the body image.
        UpdateBodyImageSpritePosition(spriteFit, spritePositionPercentage);
    }

    private void FitBodyImageSpriteVertically()
    {
        bodyImage.rectTransform.sizeDelta = new Vector2(bodyImage.rectTransform.sizeDelta.x / (bodyImage.rectTransform.sizeDelta.y / bodyImageMask.rectTransform.sizeDelta.y), bodyImage.rectTransform.sizeDelta.y / (bodyImage.rectTransform.sizeDelta.y / bodyImageMask.rectTransform.sizeDelta.y));
    }

    private void FitBodyImageSpriteHorizontally()
    {
        bodyImage.rectTransform.sizeDelta = new Vector2(bodyImage.rectTransform.sizeDelta.x / (bodyImage.rectTransform.sizeDelta.x / bodyImageMask.rectTransform.sizeDelta.x), bodyImage.rectTransform.sizeDelta.y / (bodyImage.rectTransform.sizeDelta.x / bodyImageMask.rectTransform.sizeDelta.x));
    }

    private void UpdateBodyImageSpritePosition(GalaxyPopupSpriteFit spriteFit, float spritePositionPercentage)
    {
        switch (spriteFit)
        {
            case GalaxyPopupSpriteFit.VerticalAndHorizontal:
                break;
            case GalaxyPopupSpriteFit.Vertical:
                float maximumChangeInXPosition = (bodyImage.rectTransform.sizeDelta.x / 2) - (bodyImageMask.rectTransform.sizeDelta.x / 2);
                bodyImage.transform.localPosition = new Vector2(bodyImage.transform.localPosition.x + (maximumChangeInXPosition * spritePositionPercentage), bodyImage.transform.localPosition.y);
                break;
            case GalaxyPopupSpriteFit.Horizontal:
                float maximumChangeInYPosition = (bodyImage.rectTransform.sizeDelta.y / 2) - (bodyImageMask.rectTransform.sizeDelta.y / 2);
                bodyImage.transform.localPosition = new Vector2(bodyImage.transform.localPosition.x, bodyImage.transform.localPosition.y + (maximumChangeInYPosition * (spritePositionPercentage * -1)));
                break;

            default:
                Debug.LogWarning("Update Body Image Sprite Position logic for Galaxy Popup Sprite Fit: " + spriteFit.ToString() + " has not been implemented.");
                break;
        }
    }
}

[System.Serializable]

public class GalaxyPopupData
{
    public string headLine;
    public string spriteResourcesFilePath;
    public string bodyText;
    public string specialOpenSFXName = null;

    public bool answerRequired = false;

    public GalaxyPopupSpriteFit spriteFit = 0;

    //Ranges from -1 (sprite lowest or leftmost y position) to 1 (sprite highest or rightmost y position).
    public float spritePositionPercentage = 0;

    public List<GalaxyPopupOptionData> options = new List<GalaxyPopupOptionData>();
}

[System.Serializable]

public class GalaxyPopupOptionData
{
    public List<GalaxyPopupOptionEffect> effects = new List<GalaxyPopupOptionEffect>();

    public string mainText;
    public string effectDescriptionText;
}

[System.Serializable]

public class GalaxyPopupOptionEffect
{
    public enum GalaxyPopupOptionEffectType
    {
        AddCreditsToEmpire,
        AddCreditsPerTurnToEmpire,
        AddPresciptionsToEmpire,
        AddPrescriptionsPerTurnToEmpire,
        AddScienceToEmpire,
        AddSciencePerTurnToEmpire,
        ConquerPlanet,
        OpenResearchView
    }
    public GalaxyPopupOptionEffectType effectType;

    public List<int> effectDependencies = new List<int>();
    public int effectAmount;
}

public enum GalaxyPopupSpriteFit
{
    VerticalAndHorizontal,
    Vertical,
    Horizontal
}