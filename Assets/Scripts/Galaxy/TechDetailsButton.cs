using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TechDetailsButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("Image Components")]

    [SerializeField]
    [Tooltip("The image component that displays the background image of the tech that is having its details displayed.")]
    private Image backgroundImage = null;

    [Header("Text Components")]

    [SerializeField]
    [Tooltip("The text component that displays the name of the tech that is having its details displayed.")]
    private Text techNameText = null;
    [SerializeField]
    [Tooltip("The text component that displays the description of the tech that is having its details displayed.")]
    private Text techDescriptionText = null;
    [SerializeField]
    [Tooltip("The text component that displays the level of the tech that is having its details displayed.")]
    private Text techLevelText = null;
    [SerializeField]
    [Tooltip("The text component that displays the cost of the tech that is having its details displayed.")]
    private Text techCostText = null;

    [Header("SFX Options")]

    [SerializeField]
    [Tooltip("The sound effect that will be played whenever the player mouses over the button.")]
    private AudioClip mouseOverSFX = null;

    //Non-inspector variables.

    //The variable that indicates the name of the sprite that will be loaded in from the Resources/Galaxy/Tech Details Buttons folder and assigned to the sprite of the background image.
    private string backgroundSpriteName;

    private bool backgroundSpriteLoadedIn;

    // Start is called before the first frame update
    void Start()
    {
        if (ShouldBackgroundSpriteBeLoadedIn())
        {
            LoadInBackgroundSprite();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!backgroundSpriteLoadedIn && ShouldBackgroundSpriteBeLoadedIn())
        {
            LoadInBackgroundSprite();
        }
    }

    //Returns a boolean that indicates whether or not the sprite of the background image should be loaded in from the project resources or not.
    private bool ShouldBackgroundSpriteBeLoadedIn()
    {
        if (-175 + (60 * transform.GetSiblingIndex()) <= transform.parent.localPosition.y)
        {
            return true;
        }

        return false;
    }

    //Loads in the sprite that should be assigned to the sprite of the background image from the project resources.
    private void LoadInBackgroundSprite()
    {
        backgroundImage.sprite = Resources.Load<Sprite>("Galaxy/Tech Details Buttons/" + backgroundSpriteName);

        backgroundSpriteLoadedIn = true;
    }

    //The tech name text component has its displayed text set to the specified string value.
    public void SetTechNameText(string techName)
    {
        techNameText.text = techName;
    }

    //The tech description text component has its displayed text set to the specified string value.
    public void SetTechDescriptionText(string techDescription)
    {
        techDescriptionText.text = techDescription;
    }

    //The tech level text component has its displayed text set to the specified string value.
    public void SetTechLevelText(string techLevel)
    {
        techLevelText.text = techLevel;
    }

    //The tech cost text component has its displayed text set to the specified string value.
    public void SetTechCostText(string techCost)
    {
        techCostText.text = techCost;
    }

    //The variable that indicates the name of the sprite that will be loaded in from the Resources/Galaxy/Tech Details Buttons folder and assigned to the sprite of the background image is set to the specified value.
    public void SetBackgroundSpriteName(string newBackgroundSpriteName)
    {
        backgroundSpriteName = newBackgroundSpriteName;
    }

    //This method is called whenever the player mouses over the button (through an event trigger).
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Plays the appropriate sound effect.
        if(mouseOverSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverSFX);
    }
}
