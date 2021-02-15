using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyTooltip : MonoBehaviour
{
    public enum TooltipClosingType
    {
        Deactivate,     //The tooltip game object is not destroyed, but rather still in the background and deactivated (More Processor Friendly).
        Destroy     //The tooltip game object is not in the background and deactivated, but rather completely destroyed (More Memory Friendly).
    }

    [Header("Logic Options")]

    //Indicates how the tooltip will close (see the enum definition above to get more details on the closing types).
    [Tooltip("Indicates how the tooltip will close (see the enum definition above to get more details on the closing types).")]
    public TooltipClosingType closingType;

    //The prefab that the tooltip game object will be instantiated from (assigned a value in the start method of the galaxy generator).
    public static GameObject tooltipPrefab;
    //The actual game object of the tooltip.
    GameObject tooltip;

    //The parent transform of the tooltip.
    [Tooltip("The parent transform of the tooltip.")]
    [SerializeField]
    private Transform tooltipParent;

    //Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.
    [Tooltip("Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.")]
    [SerializeField]
    private Vector2 initialLocalPosition = Vector2.zero;

    //Indicates whether the tooltip will have its location set to the mouse's location every update.
    [Tooltip("Indicates whether the tooltip will have its location set to the mouse's location every update.")]
    public bool updateToMousePosition;
    //Indicates whether the tooltip is open or not.
    bool tooltipOpen;

    [Header("Text Content")]

    //The text that the tooltip displays to the user.
    [Tooltip("The text that the tooltip displays to the user.")]
    [TextArea]
    [SerializeField]
    private string tooltipText;

    //The font that the text of the tooltip will be.
    [Tooltip("The font that the text of the tooltip will be.")]
    [SerializeField]
    private Font font;

    //Indicates the size of the font of the tooltip text.
    [Tooltip("Indicates the size of the font of the tooltip text.")]
    [SerializeField]
    private int fontSize;

    [Header("Text Shadow")]

    //Indicates whether the shadow component of the tooltip's text is enabled.
    [Tooltip("Indicates whether the shadow component of the tooltip's text is enabled.")]
    [SerializeField]
    private bool textShadowEnabled;

    //Indicates the effect distance of the shadow component of the tooltip's text.
    [Tooltip("Indicates the effect distance of the shadow component of the tooltip's text.")]
    [SerializeField]
    private Vector2 textShadowEffectDistance = new Vector2(1, -1);

    public enum GalaxyTooltipColorOption
    {
        Default,
        Transparent,
        PlayerEmpireColor,
        CustomColor0,
        CustomColor1,
        CustomColor2
    }

    [Header("Coloring Options")]

    //Indicates the color of the text of the tooltip.
    [Tooltip("Indicates the color of the text of the tooltip.")]
    [SerializeField]
    private GalaxyTooltipColorOption textColor;

    //Indicates the color of the shadow component of the tooltip's text.
    [Tooltip("Indicates the color of the shadow component of the tooltip's text.")]
    [SerializeField]
    private GalaxyTooltipColorOption textShadowColor;

    //Indicates the background color of the tooltip.
    [Tooltip("Indicates the background color of the tooltip.")]
    [SerializeField]
    private GalaxyTooltipColorOption backgroundColor;

    //List of custom colors that the tooltip can use.
    [Tooltip("A list of custom colors that the tooltip can use.")]
    [SerializeField]
    private List<Color> customColors = new List<Color>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Updates the position of the tooltip to the position of the player's mouse if the tooltip is open.
        if (tooltipOpen && updateToMousePosition)
        {
            tooltip.transform.position = Input.mousePosition;
            tooltip.transform.localPosition = new Vector2(tooltip.transform.localPosition.x + initialLocalPosition.x, tooltip.transform.localPosition.y + initialLocalPosition.y);
        }
    }

    //Opens the tooltip whenever the mouse/pointer enters the trigger zone (must be called through the Unity event trigger system).
    public void PointerEnter()
    {
        OpenTooltip();
    }

    //Closes the tooltip whenever the mouse/pointer exits the trigger zone (must be called through the Unity event trigger system, except for when being called by the OnDisable() method of this class).
    public void PointerExit()
    {
        CloseTooltip();
    }

    //Instantiates the tooltip game object from the tooltip prefab, sets the parent of the tooltip, resets essential transfrom components, and updates the text of the tooltip.
    void CreateTooltip()
    {
        //Instantiates the tooltip game object from the tooltip prefab.
        tooltip = Instantiate(tooltipPrefab);
        //Sets the parent of the tooltip.
        if(tooltipParent != null)
            tooltip.transform.SetParent(tooltipParent);
        //Sets the name of the new tooltip game object.
        tooltip.gameObject.name = gameObject.name + " Tooltip";
        //Resets the scale and position of the tooltip.
        tooltip.transform.localScale = Vector3.one;
        tooltip.transform.localPosition = initialLocalPosition;
        //Sets the color of the background image of the tooltip.
        tooltip.transform.GetChild(0).GetComponent<Image>().color = GetColorFromColorOption(backgroundColor, "Background");
        //Updates the text of the tooltip.
        UpdateTooltipText();
    }

    //Opens the tooltip.
    void OpenTooltip()
    {
        //Activates the tooltip's game object if it is not destroyed every time the tooltip closes.
        if (closingType == TooltipClosingType.Deactivate)
        {
            if(tooltip == null)
                CreateTooltip();
            tooltip.SetActive(true);
        }
        //Creates a whole new tooltip again if it is destroyed every time the tooltip closes.
        else if (closingType == TooltipClosingType.Destroy)
            CreateTooltip();

        //Indicates that the tooltip is open.
        tooltipOpen = true;
    }

    //Closes the tooltip.
    public void CloseTooltip()
    {
        //Deactivates the tooltip's game object if it is not destroyed every time the tooltip closes.
        if (closingType == TooltipClosingType.Deactivate)
        {
            if(tooltipOpen)
                tooltip.SetActive(false);
        }
        //Destroys the tooltip game object if it is supposed to be destroyed every time the tooltip closes.
        else if (closingType == TooltipClosingType.Destroy)
            Destroy(tooltip);

        //Indicates that the tooltip is closed.
        tooltipOpen = false;
    }

    //Updates the text element of the tooltip and makes sure that the width of the background image adjusts to match the width of the text.
    void UpdateTooltipText()
    {
        Text textElementOfTooltip = tooltip.transform.GetChild(1).GetComponent<Text>();
        //Sets the font of the text of the tooltip.
        if(font != null)
            textElementOfTooltip.font = font;
        //Sets the font size of the text of the tooltip.
        if(fontSize > 0)
            textElementOfTooltip.fontSize = fontSize;
        //Sets the color of the text of the tooltip.
        textElementOfTooltip.color = GetColorFromColorOption(textColor, "Text");
        //Enables the shadow component of the tooltip's text if it is supposed to be enabled and disables it if it is supposed to be disabled.
        if (textShadowEnabled)
            tooltip.transform.GetChild(1).GetComponent<Shadow>().enabled = true;
        else
            tooltip.transform.GetChild(1).GetComponent<Shadow>().enabled = false;
        //Sets the effect distance of the shadow component of the tooltip's text.
        tooltip.transform.GetChild(1).GetComponent<Shadow>().effectDistance = textShadowEffectDistance;
        //Sets the color of the shadow component of the tooltip's text.
        tooltip.transform.GetChild(1).GetComponent<Shadow>().effectColor = GetColorFromColorOption(textShadowColor, "Text Shadow");
        //Sets the text of the tooltip.
        textElementOfTooltip.text = tooltipText;
        //Adjusts the background image of the tooltip to the size of the text of the tooltip.
        tooltip.transform.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(tooltip.transform.GetChild(1).GetComponent<Text>().preferredWidth + 5, tooltip.transform.GetChild(1).GetComponent<Text>().preferredHeight + 3);
    }

    //Should be called in order to set the tooltip text through code.
    public void SetTooltipText(string newTooltipText)
    {
        tooltipText = newTooltipText;
        if(tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Should be called in order to set the tooltip's parent through code.
    public void SetTooltipParent(Transform newTooltipParent)
    {
        tooltipParent = newTooltipParent;
        if(tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            tooltip.transform.SetParent(tooltipParent);
    }

    //Should be called in order to set the tooltip's text font through code.
    public void SetFont(Font newTooltipTextFont)
    {
        font = newTooltipTextFont;
        if (tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Should be called in order to set the font size of the tooltip text through code.
    public void SetTooltipTextFontSize(int newFontSize)
    {
        fontSize = newFontSize;
        if (tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Should be called in order to set the background color of the tooltip through code.
    public void SetBackgroundColor(GalaxyTooltipColorOption colorOption)
    {
        backgroundColor = colorOption;
        if(tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            tooltip.transform.GetChild(0).GetComponent<Image>().color = GetColorFromColorOption(backgroundColor, "Background");
    }

    //Should be called in order to set the color of the text of the tooltip through code.
    public void SetTextColor(GalaxyTooltipColorOption colorOption)
    {
        textColor = colorOption;
        if (tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Should be called in order to set if the shadow component of the tooltip's text is enabled or not through code.
    public void SetTextShadowEnabled(bool isTextShadowEnabled)
    {
        textShadowEnabled = isTextShadowEnabled;
        if (tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Should be called in order to set the effect distance of the shadow component of the tooltip's text through code.
    public void SetTextShadowEffectDistance(Vector2 newTextShadowEffectDistance)
    {
        textShadowEffectDistance = newTextShadowEffectDistance;
        if (tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    public void SetTextShadowColor(GalaxyTooltipColorOption colorOption)
    {
        textShadowColor = colorOption;
        if (tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Returns the appropriate color depending on the color identifier and what component of the tooltip the color corresponds to.
    Color GetColorFromColorOption(GalaxyTooltipColorOption colorOption, string componentOfTooltip)
    {
        switch (colorOption)
        {
            case GalaxyTooltipColorOption.Default:
                switch (componentOfTooltip.ToLower())
                {
                    case "background":
                        return tooltipPrefab.transform.GetChild(0).GetComponent<Image>().color;
                    case "text":
                        return tooltipPrefab.transform.GetChild(1).GetComponent<Text>().color;
                    case "text shadow":
                        return tooltipPrefab.transform.GetChild(1).GetComponent<Shadow>().effectColor;

                    default:
                        Debug.Log("Something has gone wrong. " + componentOfTooltip + " is not a valid tooltip component. See the GetColorFromIdentifier() method in the GalaxyTooltip class for details.");
                        return Color.white;
                }
            case GalaxyTooltipColorOption.Transparent:
                return Color.clear;
            case GalaxyTooltipColorOption.PlayerEmpireColor:
                return Empire.empires[GalaxyManager.playerID].empireColor;

            default:
                if (colorOption.ToString().ToLower().Contains("customcolor"))
                {
                    int customColorIndex = GeneralHelperMethods.GetNumberFromText(colorOption.ToString(), 11, colorOption.ToString().Length - 1);
                    if (customColorIndex >= 0 && customColorIndex < customColors.Count)
                        return customColors[customColorIndex];
                    Debug.Log("Custom Color Index: " + customColorIndex + " is an invalid custom color index. See the GetColorFromIdentifier() method in the GalaxyTooltip class for details.");
                    return Color.white;
                }
                Debug.Log("Something has gone wrong. Color option: " + colorOption + " is not a valid color identifier. See the GetColorFromIdentifier() method in the GalaxyTooltip class for details.");
                return Color.white;
        }
    }

    //Closes the tooltip if the trigger zone is deactivated/disabled.
    private void OnDisable()
    {
        CloseTooltip();
    }
}
