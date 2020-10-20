using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPopup : MonoBehaviour
{
    public Text headLineText;
    public Image bodyImage;
    public Text bodyText;

    public AudioClip defaultOpenPopupSFX;
    public AudioClip mouseOverOptionButton;

    public List<Button> optionButtons;
    public List<Text> optionButtonTexts;

    public List<GameObject> optionEffectsDescriptions;
    public List<Image> optionEffectsDescriptionBackgroundImages;
    public List<Text> optionEffectsDescriptionTexts;

    List<GalaxyPopupOptionData> optionsData = new List<GalaxyPopupOptionData>();

    public float popupScaleIncreaseRate;

    public int popupIndex;

    bool answerRequired;
    bool mouseOverPopup;
    bool beingMoved;

    Vector2 mouseToMenuDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && transform.GetSiblingIndex() == transform.parent.childCount - 1 && !GalaxyManager.popupClosedOnFrame && !GalaxyConfirmationPopup.galaxyConfirmationPopup.gameObject.activeInHierarchy)
        {
            ClosePopup();
        }

        if (transform.localScale.x < 1 || transform.localScale.y < 1)
        {
            transform.localScale = new Vector3(transform.localScale.x + (popupScaleIncreaseRate * Time.deltaTime), transform.localScale.y + (popupScaleIncreaseRate * Time.deltaTime), transform.localScale.z);

            if (transform.localScale.x > 1)
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            if (transform.localScale.y > 1)
                transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        }

        //Deals with the popup being dragged.
        if (beingMoved)
        {
            transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if (transform.localPosition.x < -291)
            {
                transform.localPosition = new Vector2(-291, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x < GalaxyManager.galaxyCamera.pixelWidth * (-.13545f))
                    mouseToMenuDistance.x = GalaxyManager.galaxyCamera.pixelWidth * (-.13545f);
            }
            //Right barrier.
            if (transform.localPosition.x > 291)
            {
                transform.localPosition = new Vector2(291, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x > GalaxyManager.galaxyCamera.pixelWidth * (.13545f))
                    mouseToMenuDistance.x = GalaxyManager.galaxyCamera.pixelWidth * (.13545f);
            }
            //Top barrier.
            if (transform.localPosition.y > 67.5f)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, 67.5f);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y > GalaxyManager.galaxyCamera.pixelHeight * (.2771f))
                    mouseToMenuDistance.y = GalaxyManager.galaxyCamera.pixelHeight * (.2771f);
            }
            //Bottom barrier.
            if (transform.localPosition.y < -99)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, -99);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y < GalaxyManager.galaxyCamera.pixelHeight * (-.2771f))
                    mouseToMenuDistance.y = GalaxyManager.galaxyCamera.pixelHeight * (-.2771f);
            }
        }

        //If the popup is clicked, it is brought to the top of the popup hierarchy.
        if (mouseOverPopup && Input.GetMouseButtonDown(0))
            transform.SetAsLastSibling();

        foreach(GameObject optionEffectDescription in optionEffectsDescriptions)
        {
            optionEffectDescription.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    public void CreatePopup(GalaxyPopupData popupData, int indexOfPopup)
    {
        headLineText.text = popupData.headLine;
        bodyImage.sprite = GalaxyPopupManager.GetPopupSpriteFromName(popupData.spriteName);
        bodyText.text = popupData.bodyText;
        answerRequired = popupData.answerRequired;
        List<int> optionButtonsUsed = new List<int>();
        int optionsProcessed = 0;
        for(int x = optionButtons.Count - popupData.options.Count; x < optionButtons.Count; x++)
        {
            optionButtonsUsed.Add(x);
            optionsData.Add(popupData.options[optionsProcessed]);
            optionButtonTexts[x].text = popupData.options[optionsProcessed].mainText;
            optionEffectsDescriptionTexts[x].text = popupData.options[optionsProcessed].effectDescriptionText;
            optionEffectsDescriptionBackgroundImages[x].rectTransform.sizeDelta = new Vector2(optionEffectsDescriptionTexts[x].preferredWidth + 5, optionEffectsDescriptionBackgroundImages[x].rectTransform.sizeDelta.y);
            optionsProcessed++;
        }
        for(int x = 0; x < optionButtons.Count; x++)
        {
            if (!optionButtonsUsed.Contains(x))
            {
                optionButtons[x].gameObject.SetActive(false);
            }
        }

        popupIndex = indexOfPopup;
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(defaultOpenPopupSFX);
    }

    public void ChooseOption(int optionNumber)
    {
        int optionsProcessed = 0;
        for (int x = optionButtons.Count - optionsData.Count; x < optionButtons.Count; x++)
        {
            for(int y = 0; y < optionsData[optionsProcessed].effects.Count; y++)
            {
                GalaxyPopupOptionEffect effect = optionsData[optionsProcessed].effects[y];
                switch (effect.effectType)
                {
                    case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.None:
                        break;

                    default:
                        Debug.Log("Popup Option Effect Type Does Nothing (Not Implemented In Switch Statement In GalaxyPopup Class ChooseOption Method).");
                        break;
                }
            }
            optionsProcessed++;
        }
        ClosePopup();
    }

    public bool IsAnswerRequired()
    {
        return answerRequired;
    }

    public void PointerDownPopup()
    {
        //Tells the update function that the player is dragging the menu.
        beingMoved = true;

        //Tells the update function the set difference between the mouse position and the menu's position.
        mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;
        mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;
    }

    public void PointerUpPopup()
    {
        //Tells the update function that the player is no longer dragging the menu.
        beingMoved = false;

        //Resets the vector that says the difference between the mouse position and the menu's position.
        mouseToMenuDistance = Vector2.zero;
    }

    public void ToggleMouseOverPopup()
    {
        mouseOverPopup = !mouseOverPopup;
    }

    public bool IsMouseOverPopup()
    {
        return mouseOverPopup;
    }

    public void ClosePopup()
    {
        GalaxyPopupManager.ClosePopup(popupIndex);
    }

    public void PointerEnterOptionButton(int buttonNum)
    {
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverOptionButton);
        mouseOverPopup = true;

        optionEffectsDescriptions[buttonNum].SetActive(true);
    }

    public void PointerExitOptionButton(int buttonNum)
    {
        mouseOverPopup = false;

        optionEffectsDescriptions[buttonNum].SetActive(false);
    }
}

public class GalaxyPopupData
{
    public string headLine;
    public string spriteName;
    public string bodyText;

    public bool answerRequired;

    public List<GalaxyPopupOptionData> options = new List<GalaxyPopupOptionData>();
}

public class GalaxyPopupOptionData
{
    public enum GalaxyPopupOptionEffectType
    {
        None
    }
    public List<GalaxyPopupOptionEffect> effects = new List<GalaxyPopupOptionEffect>();

    public string mainText;
    public string effectDescriptionText;
}

public struct GalaxyPopupOptionEffect
{
    public enum GalaxyPopupOptionEffectType
    {
        None
    }
    public GalaxyPopupOptionEffectType effectType;

    public int effectAmount;
}
