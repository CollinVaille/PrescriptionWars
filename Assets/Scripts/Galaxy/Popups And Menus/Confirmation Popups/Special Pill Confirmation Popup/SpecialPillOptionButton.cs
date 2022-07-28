using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpecialPillOptionButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    [Header("Components")]

    [SerializeField] private RawImage pillViewRawImage = null;

    [SerializeField] private Image experienceLevelImage = null;

    [SerializeField] private Text nameText = null;
    [SerializeField] private Text taskText = null;
    [SerializeField] private Text skillText = null;
    [SerializeField] private Text experienceLevelText = null;

    /// <summary>
    /// Public property that is access only and returns the special pill that the option button is meant to represent (returns null if there is none).
    /// </summary>
    public GalaxySpecialPill specialPill { get => Empire.empires[GalaxyManager.PlayerID].GetSpecialPill(specialPillID); }
    /// <summary>
    /// Public property that should be used both to access and mutate the id of the special pill that the special pill option button is supposed to be representing.
    /// </summary>
    public int specialPillID
    {
        get => specialPillIDVar;
        set
        {
            originalNormalButtonColor = gameObject.GetComponent<Button>().colors.normalColor;
            originalHighlightedButtonColor = gameObject.GetComponent<Button>().colors.highlightedColor;
            specialPillIDVar = value;
            GalaxySpecialPill specialPill = Empire.empires[GalaxyManager.PlayerID].GetSpecialPill(value);
            if(specialPill != null)
            {
                if (pillView == null)
                    pillView = PillViewsManager.GetNewPillView(specialPill.convertedToGalaxyPill);
                else
                    pillView.DisplayedPill = specialPill.convertedToGalaxyPill;
                pillViewRawImage.texture = pillView.RenderTexture;
                pillViewRawImage.gameObject.SetActive(true);
                nameText.text = specialPill.name;
                nameText.gameObject.SetActive(true);
                taskText.text = specialPill.isBusy ? "Busy " + specialPill.task : "Available";
                taskText.gameObject.SetActive(true);
                if(!selected)
                    gameObject.GetComponent<Button>().interactable = !specialPill.isBusy;
                skillSelectedIndex = skillSelectedIndex;
            }
            else
            {
                pillView.Delete();
                pillViewRawImage.gameObject.SetActive(false);
                pillViewRawImage.texture = null;
                nameText.gameObject.SetActive(false);
                taskText.gameObject.SetActive(false);
                gameObject.GetComponent<Button>().interactable = true;
                skillText.gameObject.SetActive(false);
                experienceLevelImage.gameObject.SetActive(false);
                experienceLevelText.gameObject.SetActive(false);
            }
        }
    }
    private int specialPillIDVar = -1;

    /// <summary>
    /// Public property that should be used both to access and muatate the skill that the special pill has their experience displayed for.
    /// </summary>
    public int skillSelectedIndex
    {
        get => skillSelectedIndexVar;
        set
        {
            skillSelectedIndexVar = value;
            if(value < 0 || value >= Enum.GetValues(typeof(GalaxySpecialPill.Skill)).Length || specialPill == null)
            {
                skillText.gameObject.SetActive(false);
                experienceLevelImage.gameObject.SetActive(false);
                experienceLevelText.gameObject.SetActive(false);
            }
            else
            {
                skillText.text = GeneralHelperMethods.GetEnumText(((GalaxySpecialPill.Skill)value).ToString()) + ":";
                skillText.gameObject.SetActive(true);
                experienceLevelImage.gameObject.SetActive(true);
                experienceLevelText.text = specialPill.GetExperienceLevel((GalaxySpecialPill.Skill)value).ToString();
                experienceLevelText.gameObject.SetActive(true);
            }
        }
    }
    private int skillSelectedIndexVar = -1;

    /// <summary>
    /// Public property that should be used both to access and mutate whether the special pill option button appears to be selected (but does not register a selection change with the confirmation popup, that should be done in the onclick method).
    /// </summary>
    public bool selected
    {
        get => selectedVar;
        set
        {
            ColorBlock colorBlock = gameObject.GetComponent<Button>().colors;
            if (selected && !value)
            {
                colorBlock.normalColor = originalNormalButtonColor;
                colorBlock.highlightedColor = originalHighlightedButtonColor;
            }
            else if (!selected && value)
            {
                colorBlock.normalColor = colorBlock.pressedColor;
                colorBlock.highlightedColor = colorBlock.pressedColor;
                gameObject.GetComponent<Button>().interactable = true;
            }
            gameObject.GetComponent<Button>().colors = colorBlock;
            selectedVar = value;
        }
    }
    private bool selectedVar = false;

    private Color originalNormalButtonColor, originalHighlightedButtonColor;

    private PillView pillView = null;

    public SpecialPillConfirmationPopup specialPillConfirmationPopup = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method should be called through an event trigger whenever the special pill option button is clicked and sets its assigned special pill as the one that is selected.
    /// </summary>
    public void OnClickSpecialPillOptionButton()
    {
        specialPillConfirmationPopup.OnClickSpecialPillOptionButton(specialPillID);
    }

    /// <summary>
    /// This method should be called whenever the button is selected in order to immediately deselect it (the button just looks better this way in my opinion).
    /// </summary>
    /// <param name="eventData"></param>
    public void OnSelect(BaseEventData eventData)
    {
        gameObject.GetComponent<Button>().OnDeselect(eventData);
    }

    /// <summary>
    /// This method should be called whenever the player hovers over the special pill option button and calls a method on the confirmation popup in order to play the appropriate sound effect.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        specialPillConfirmationPopup.OnPointerEnterSpecialPillOptionButton(this);
    }

    /// <summary>
    /// This method is called whenever the special pill option button is destroyed and ensure that the pill view has been deleted.
    /// </summary>
    private void OnDestroy()
    {
        if (pillView != null)
            pillView.Delete();
    }
}
