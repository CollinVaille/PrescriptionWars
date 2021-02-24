using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class GalaxyPopup : GalaxyPopupBehaviour
{
    public Text headLineText;
    public Image bodyImage;
    public Text bodyText;

    public AudioClip specialOpenPopupSFX;
    public AudioClip mouseOverOptionButton;
    public AudioClip clickOptionButtonSFX;

    public List<Button> optionButtons;
    public List<Text> optionButtonTexts;

    List<GalaxyPopupOptionData> optionsData = new List<GalaxyPopupOptionData>();

    //Indicates the popup's index in the list of popups in the popup manager.
    public int popupIndex;

    //Indicates whether the player is required to provide an answer for the popup to close.
    bool answerRequired;
    //Indicates whether the special sound effect for whenever the popup opens has played or not.
    bool specialOpenPopupSFXPlayed;

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
            if (specialOpenPopupSFX != null)
                GalaxyManager.galaxyManager.sfxSource.PlayOneShot(specialOpenPopupSFX);
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
        bodyImage.sprite = GalaxyPopupManager.GetPopupSpriteFromName(popupData.spriteName);
        bodyText.text = popupData.bodyText;
        specialOpenPopupSFX = GalaxyPopupManager.GetPopupSFXFromName(popupData.specialOpenSFXName);
        answerRequired = popupData.answerRequired;
        List<int> optionButtonsUsed = new List<int>();
        int optionsProcessed = 0;
        //Sets the data of all of the option buttons that will be used.
        for(int x = optionButtons.Count - popupData.options.Count; x < optionButtons.Count; x++)
        {
            optionButtonsUsed.Add(x);
            optionsData.Add(popupData.options[optionsProcessed]);
            optionButtonTexts[x].text = popupData.options[optionsProcessed].mainText;
            optionButtons[x].gameObject.GetComponent<GalaxyTooltip>().SetTooltipText(popupData.options[optionsProcessed].effectDescriptionText);
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
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(clickOptionButtonSFX);
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
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverOptionButton);
        SetMouseOverPopup(true);
    }

    public void PointerExitOptionButton(int buttonNum)
    {
        SetMouseOverPopup(false);
    }

    public string GetHeadLine()
    {
        return headLineText.text;
    }
}

[System.Serializable]

public class GalaxyPopupData
{
    public string headLine;
    public string spriteName;
    public string bodyText;
    public string specialOpenSFXName = null;

    public bool answerRequired;

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
