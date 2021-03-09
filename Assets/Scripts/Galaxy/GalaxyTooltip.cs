using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

public class GalaxyTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    //The parent transform of the tooltip.
    [SerializeField, Tooltip("The parent transform of the tooltip.")]
    private Transform tooltipParent = null;

    //Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.
    [SerializeField, Tooltip("Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.")]
    private Vector2 initialLocalPosition = new Vector2(5, -15);

    //Indicates the amount of space between the text of the tooltip and the edge of the tooltip.
    [Tooltip("Indicates the amount of space between the text of the tooltip and the edge of the tooltip.")]
    [SerializeField]
    private Vector2 edgeBuffer = new Vector2(5, 3);

    //Indicates whether the tooltip will have its location set to the mouse's location every update.
    [Tooltip("Indicates whether the tooltip will have its location set to the mouse's location every update.")]
    public bool updateToMousePosition;

    [Header("Text Content")]

    //The text that the tooltip displays to the user.
    [SerializeField, TextArea, Tooltip("The text that the tooltip displays to the user.")]
    private string tooltipText = "";

    //The font that the text of the tooltip will be.
    [SerializeField, Tooltip("The font that the text of the tooltip will be.")]
    private Font font = null;

    //Indicates the size of the font of the tooltip text.
    [SerializeField, Tooltip("Indicates the size of the font of the tooltip text.")]
    private int fontSize;

    [Header("Text Shadow")]

    //Indicates whether the shadow component of the tooltip's text is enabled.
    [Tooltip("Indicates whether the shadow component of the tooltip's text is enabled.")]
    public bool textShadowEnabled = false;

    //Indicates the effect distance of the shadow component of the tooltip's text.
    [SerializeField, Tooltip("Indicates the effect distance of the shadow component of the tooltip's text."), ConditionalField("textShadowEnabled", true, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    private Vector2 textShadowEffectDistance = new Vector2(1, -1);

    public enum GalaxyTooltipColorOption
    {
        Default,
        Transparent,
        PlayerEmpireColor,
        CustomColor
    }

    [Header("Coloring Options")]

    //Indicates the color of the text of the tooltip.
    [LabelOverride("Text Color"), Tooltip("Indicates the color of the text of the tooltip.")]
    public GalaxyTooltipColorOption textColorOption = GalaxyTooltipColorOption.Default;

    [SerializeField, ConditionalField("textColorOption", GalaxyTooltipColorOption.CustomColor, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    private Color textCustomColor = Color.white;

    //Indicates the color of the shadow component of the tooltip's text.
    [LabelOverride("Text Shadow Color"), Tooltip("Indicates the color of the shadow component of the tooltip's text.")]
    public GalaxyTooltipColorOption textShadowColorOption = GalaxyTooltipColorOption.Default;

    [SerializeField, ConditionalField("textShadowColorOption", GalaxyTooltipColorOption.CustomColor, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    private Color textShadowCustomColor = Color.white;

    //Indicates the background color of the tooltip.
    [LabelOverride("Background Color"), Tooltip("Indicates the background color of the tooltip.")]
    public GalaxyTooltipColorOption backgroundColorOption = GalaxyTooltipColorOption.Default;

    [SerializeField, ConditionalField("backgroundColorOption", GalaxyTooltipColorOption.CustomColor, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    private Color backgroundCustomColor = Color.white;

    //---------------------------------------------------------------------------------------
    //Non-inspector variables.
    //---------------------------------------------------------------------------------------

    //The actual game object of the tooltip.
    private GameObject tooltip = null;

    //The prefab that the tooltip game object will be instantiated from (assigned a value in the start method of the galaxy generator).
    public static GameObject tooltipPrefab;

    //The canvas that the tooltip is under (set in the start method of the tooltip).
    private Canvas parentCanvas = null;

    //Indicates whether the tooltip is open or not.
    private bool tooltipOpen = false;

    // Start is called before the first frame update
    private void Start()
    {
        parentCanvas = GetCurrentParentCanvas();
    }

    // Update is called once per frame
    private void Update()
    {
        //Updates the position of the tooltip to the position of the player's mouse if the tooltip is open.
        if (tooltipOpen && updateToMousePosition)
        {
            tooltip.transform.position = Input.mousePosition;
            tooltip.transform.localPosition = new Vector2(tooltip.transform.localPosition.x + initialLocalPosition.x, tooltip.transform.localPosition.y + (initialLocalPosition.y - (edgeBuffer.y / 2)));
            PreventTooltipGoingOffscreen();
        }
    }

    //Prevents the tooltip from going off of the screen.
    private void PreventTooltipGoingOffscreen()
    {
        if (tooltipOpen)
        {
            //Left barrier.
            if (tooltip.transform.GetChild(0).position.x < 0)
            {
                tooltip.transform.position = new Vector2(-5, tooltip.transform.position.y);
            }
            //Right barrier.
            else if (tooltip.transform.GetChild(0).position.x + (tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x * parentCanvas.scaleFactor) > GalaxyManager.galaxyCamera.scaledPixelWidth)
            {
                tooltip.transform.position = new Vector2(GalaxyManager.galaxyCamera.scaledPixelWidth - (tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x * parentCanvas.scaleFactor), tooltip.transform.position.y);
            }

            //Top barrier.
            if(tooltip.transform.GetChild(0).position.y + ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor) > GalaxyManager.galaxyCamera.scaledPixelHeight)
            {
                tooltip.transform.position = new Vector2(tooltip.transform.position.x, GalaxyManager.galaxyCamera.scaledPixelHeight - ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor));
            }
            //Bottom barrier.
            else if (tooltip.transform.GetChild(0).position.y < 0 + ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor))
            {
                tooltip.transform.position = new Vector2(tooltip.transform.position.x, 0 + ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor));
            }
        }
    }

    //Returns the canvas that the tooltip is under in the hierarchy.
    private Canvas GetCurrentParentCanvas()
    {
        Canvas currentParentCanvas = null;

        Transform nextTransformToCheck = transform.parent;
        while (currentParentCanvas == null)
        {
            if (nextTransformToCheck.GetComponent<Canvas>() != null)
            {
                currentParentCanvas = nextTransformToCheck.GetComponent<Canvas>();
                break;
            }

            if (nextTransformToCheck.parent != null)
                nextTransformToCheck = nextTransformToCheck.parent;
            else
                break;
        }

        return currentParentCanvas;
    }

    //Opens the tooltip whenever the mouse/pointer enters the trigger zone.
    public void OnPointerEnter(PointerEventData eventData)
    {
        OpenTooltip();
    }

    //Closes the tooltip whenever the mouse/pointer exits the trigger zone.
    public void OnPointerExit(PointerEventData eventData)
    {
        CloseTooltip();
    }

    //Instantiates the tooltip game object from the tooltip prefab, sets the parent of the tooltip, resets essential transfrom components, and updates the text of the tooltip.
    private void CreateTooltip()
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
        tooltip.transform.GetChild(0).GetComponent<Image>().color = GetColorFromColorOption(backgroundColorOption, "Background");
        //Updates the text of the tooltip.
        UpdateTooltipText();
    }

    //Opens the tooltip.
    private void OpenTooltip()
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
    private void UpdateTooltipText()
    {
        Text textElementOfTooltip = tooltip.transform.GetChild(1).GetComponent<Text>();
        //Sets the font of the text of the tooltip.
        if(font != null)
            textElementOfTooltip.font = font;
        //Sets the font size of the text of the tooltip.
        if(fontSize > 0)
            textElementOfTooltip.fontSize = fontSize;
        //Sets the color of the text of the tooltip.
        textElementOfTooltip.color = GetColorFromColorOption(textColorOption, "Text");
        //Enables the shadow component of the tooltip's text if it is supposed to be enabled and disables it if it is supposed to be disabled.
        if (textShadowEnabled)
            tooltip.transform.GetChild(1).GetComponent<Shadow>().enabled = true;
        else
            tooltip.transform.GetChild(1).GetComponent<Shadow>().enabled = false;
        //Sets the effect distance of the shadow component of the tooltip's text.
        tooltip.transform.GetChild(1).GetComponent<Shadow>().effectDistance = textShadowEffectDistance;
        //Sets the color of the shadow component of the tooltip's text.
        tooltip.transform.GetChild(1).GetComponent<Shadow>().effectColor = GetColorFromColorOption(textShadowColorOption, "Text Shadow");
        //Sets the text of the tooltip.
        textElementOfTooltip.text = tooltipText;
        //Adjusts the background image of the tooltip to the size of the text of the tooltip.
        tooltip.transform.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(tooltip.transform.GetChild(1).GetComponent<Text>().preferredWidth + edgeBuffer.x, tooltip.transform.GetChild(1).GetComponent<Text>().preferredHeight + edgeBuffer.y);
        //Adjusts the local x position of the text component of the tooltip in order to keep the text centered within the edge buffer.
        tooltip.transform.GetChild(1).localPosition = new Vector2(tooltipPrefab.transform.GetChild(1).localPosition.x + (edgeBuffer.x / 2), tooltip.transform.GetChild(1).localPosition.y);
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
        backgroundColorOption = colorOption;
        if(tooltipOpen || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            tooltip.transform.GetChild(0).GetComponent<Image>().color = GetColorFromColorOption(backgroundColorOption, "Background");
    }

    //Should be called in order to set the color of the text of the tooltip through code.
    public void SetTextColor(GalaxyTooltipColorOption colorOption)
    {
        textColorOption = colorOption;
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
        textShadowColorOption = colorOption;
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
                return Empire.empires[GalaxyManager.PlayerID].empireColor;
            case GalaxyTooltipColorOption.CustomColor:      //Edits needs to be made here.
                switch (componentOfTooltip.ToLower())
                {
                    case "background":
                        return backgroundCustomColor;
                    case "text":
                        return textCustomColor;
                    case "text shadow":
                        return textShadowCustomColor;

                    default:
                        Debug.Log("Something has gone wrong. " + componentOfTooltip + " is not a valid tooltip component. See the GetColorFromIdentifier() method in the GalaxyTooltip class for details.");
                        return Color.white;
                }

            default:
                Debug.Log("Something has gone wrong. Color option: " + colorOption + " is not a valid color identifier. See the GetColorFromIdentifier() method in the GalaxyTooltip class for details.");
                return Color.white;
        }
    }

    public GalaxyTooltipColorOption GetTextColorOption()
    {
        return textColorOption;
    }

    //Closes the tooltip if the trigger zone is deactivated/disabled.
    private void OnDisable()
    {
        CloseTooltip();
    }
}
