using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.AnimatedValues;

#endif

[System.Serializable]
public class GalaxyTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public enum TooltipClosingType
    {
        Destroy,     //The tooltip game object is not in the background and deactivated, but rather completely destroyed (More Memory Friendly).
        Deactivate     //The tooltip game object is not destroyed, but rather still in the background and deactivated (More Processor Friendly).
    }

    //[Header("Logic Options")]

    /// <summary>
    /// Indicates how the tooltip will close (see the enum definition above to get more details on the closing types).
    /// </summary>
    [SerializeField, HideInInspector]
    private TooltipClosingType closingType = TooltipClosingType.Destroy;

    /// <summary>
    /// Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.
    /// </summary>
    [SerializeField, HideInInspector]
    private Vector2 initialLocalPosition = new Vector2(5, -15);

    /// <summary>
    /// Indicates the amount of space between the text of the tooltip and the edge of the tooltip.
    /// </summary>
    [SerializeField, HideInInspector]
    private Vector2 edgeBuffer = new Vector2(5, 3);
    public Vector2 EdgeBuffer
    {
        get
        {
            return edgeBuffer;
        }
        set
        {
            edgeBuffer = value;
            if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
                UpdateTooltipText();
        }
    }

    /// <summary>
    /// Indicates the amount of delay in seconds between the pointer entering the trigger zone and the tooltip opening.
    /// </summary>
    [SerializeField, HideInInspector]
    private float openDelay = 0;
    public float OpenDelay
    {
        get
        {
            return openDelay;
        }
        set
        {
            openDelay = value;
        }
    }

    /// <summary>
    /// Indicates whether the tooltip will have its location set to the mouse's location every update.
    /// </summary>
    [HideInInspector]
    public bool followsMouse = true;

    /// <summary>
    /// The text that the tooltip displays to the user.
    /// </summary>
    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            text = value;
            if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
                UpdateTooltipText();
        }
    }
    [Header("Text Content"), SerializeField, HideInInspector]
    private string text = "";

    [SerializeField, HideInInspector]
    private DefaultOrCustomOption fontOption = DefaultOrCustomOption.Default;
    /// <summary>
    /// The font that the text of the tooltip will be.
    /// </summary>
    public Font Font
    {
        get
        {
            if (font == null)
                return tooltipPrefab.transform.GetChild(1).GetComponent<Text>().font;
            return font;
        }
        set
        {
            font = value;
            if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
                UpdateTooltipText();
        }
    }
    [SerializeField, HideInInspector]
    private Font font = null;

    [SerializeField, HideInInspector]
    private DefaultOrCustomOption fontSizeOption = DefaultOrCustomOption.Default;
    /// <summary>
    /// Indicates the size of the font of the tooltip text.
    /// </summary>
    public int FontSize
    {
        get
        {
            if(fontSize == 0)
                return tooltipPrefab.transform.GetChild(1).GetComponent<Text>().font.fontSize;
            return fontSize;
        }
        set
        {
            fontSize = value;
            if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
                UpdateTooltipText();
        }
    }
    [SerializeField, HideInInspector]
    private int fontSize = 0;

    //[Header("Text Shadow")]

    /// <summary>
    /// Indicates whether the shadow component of the tooltip's text is enabled.
    /// </summary>
    [SerializeField, HideInInspector]
    private bool textShadowEnabled = false;

    /// <summary>
    /// Indicates the effect distance of the shadow component of the tooltip's text.
    /// </summary>
    public Vector2 TextShadowEffectDistance
    {
        get
        {
            return textShadowEffectDistance;
        }
        set
        {
            textShadowEffectDistance = value;
            if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
                UpdateTooltipText();
        }
    }
    [SerializeField, HideInInspector]
    private Vector2 textShadowEffectDistance = new Vector2(1, -1);

    public enum GalaxyTooltipColorOption
    {
        Default,
        Transparent,
        PlayerEmpireColor,
        CustomColor
    }

    //[Header("Coloring Options")]

    /// <summary>
    /// Indicates the color of the text of the tooltip.
    /// </summary>
    [SerializeField, HideInInspector]
    private GalaxyTooltipColorOption textColorOption = GalaxyTooltipColorOption.Default;

    [SerializeField, HideInInspector]
    private Color textCustomColor = Color.white;

    /// <summary>
    /// Indicates the color of the shadow component of the tooltip's text.
    /// </summary>
    [SerializeField, HideInInspector]
    private GalaxyTooltipColorOption textShadowColorOption = GalaxyTooltipColorOption.Default;

    [SerializeField, HideInInspector]
    private Color textShadowCustomColor = Color.white;

    /// <summary>
    /// Indicates the background color of the tooltip.
    /// </summary>
    [SerializeField, HideInInspector]
    private GalaxyTooltipColorOption backgroundColorOption = GalaxyTooltipColorOption.Default;

    [SerializeField, HideInInspector]
    private Color backgroundCustomColor = Color.white;

    //[Header("Optional Components")]

    [SerializeField, HideInInspector]
    private GalaxyTooltipEventsHandler eventsHandler = null;

    //---------------------------------------------------------------------------------------
    //Non-inspector variables.
    //---------------------------------------------------------------------------------------

    /// <summary>
    /// The actual game object of the tooltip.
    /// </summary>
    private GameObject tooltip = null;
    public Vector2 Position
    {
        set
        {
            if (!Open)
                return;

            tooltip.transform.position = value;
            initialLocalPosition = tooltip.transform.localPosition;
        }
    }

    /// <summary>
    /// The prefab that the tooltip game object will be instantiated from (assigned a value in the start method of the galaxy generator).
    /// </summary>
    private static GameObject tooltipPrefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Tooltip"); }

    /// <summary>
    /// The canvas that the tooltip is under (set in the start method of the tooltip).
    /// </summary>
    private Canvas parentCanvas = null;

    /// <summary>
    /// The parent transform of the tooltip (set in the start method of the tooltip).
    /// </summary>
    public Transform Parent
    {
        get
        {
            return parent;
        }
        set
        {
            parent = value;
            if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
                tooltip.transform.SetParent(parent);
        }
    }
    private Transform parent = null;

    /// <summary>
    /// The camera that this tooltip will be viewed by.
    /// </summary>
    private Camera sceneCamera = null;

    /// <summary>
    /// Indicates whether the tooltip is open or not.
    /// </summary>
    public bool Open { get; private set; } = false;

    /// <summary>
    /// Indicates whether the tooltip component has been applied to a 3D object.
    /// </summary>
    private bool componentAppliedTo3DObject = false;

    /// <summary>
    /// Indicates whether the pointer is in the trigger zone for the tooltip opening.
    /// </summary>
    public bool PointerInTriggerZone
    {
        get
        {
            return pointerInTriggerZone;
        }
        private set
        {
            pointerInTriggerZone = value;
            pointerInTriggerZoneDeltaTime = 0;
            if (pointerInTriggerZone && pointerInTriggerZoneDeltaTime >= OpenDelay)
                OpenTooltip();
            else if (!pointerInTriggerZone)
                CloseTooltip();
        }
    }
    private bool pointerInTriggerZone = false;

    /// <summary>
    /// Indicates the amount of time in seconds that the pointer has been in the trigger zone to open the tooltip.
    /// </summary>
    private float pointerInTriggerZoneDeltaTime = 0;

    // Start is called before the first frame update
    private void Start()
    {
        parent = GetCurrentParent();
        parentCanvas = GeneralHelperMethods.GetParentCanvas(parent);
        sceneCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        componentAppliedTo3DObject = gameObject.GetComponent<Collider>() != null;
    }

    // Update is called once per frame
    private void Update()
    {
        if (pointerInTriggerZone)
        {
            pointerInTriggerZoneDeltaTime += Time.deltaTime;
            if (!Open && pointerInTriggerZoneDeltaTime >= OpenDelay)
                OpenTooltip();
        }

        //Updates the position of the tooltip to the position of the player's mouse if the tooltip is open.
        if (Open)
        {
            if (followsMouse)
            {
                tooltip.transform.position = Input.mousePosition;
                tooltip.transform.localPosition = new Vector2(tooltip.transform.localPosition.x + initialLocalPosition.x, tooltip.transform.localPosition.y + (initialLocalPosition.y - (edgeBuffer.y / 2)));
            }
            PreventTooltipGoingOffscreen();
            if (componentAppliedTo3DObject)
            {
                //Closes the tooltip if the actual component is applied to a 3D object and the mouse is over a 2D ui element.
                if (EventSystem.current.IsPointerOverGameObject())
                    CloseTooltip();
            }
        }
    }

    //Prevents the tooltip from going off of the screen.
    private void PreventTooltipGoingOffscreen()
    {
        if (Open)
        {
            //Left barrier.
            if (tooltip.transform.GetChild(0).position.x < 0)
            {
                tooltip.transform.position = new Vector2(-5, tooltip.transform.position.y);
            }
            //Right barrier.
            else if (tooltip.transform.GetChild(0).position.x + (tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x * parentCanvas.scaleFactor) > sceneCamera.scaledPixelWidth)
            {
                tooltip.transform.position = new Vector2(sceneCamera.scaledPixelWidth - (tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x * parentCanvas.scaleFactor), tooltip.transform.position.y);
            }

            //Top barrier.
            if(tooltip.transform.GetChild(0).position.y + ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor) > sceneCamera.scaledPixelHeight)
            {
                tooltip.transform.position = new Vector2(tooltip.transform.position.x, sceneCamera.scaledPixelHeight - ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor));
            }
            //Bottom barrier.
            else if (tooltip.transform.GetChild(0).position.y < 0 + ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor))
            {
                tooltip.transform.position = new Vector2(tooltip.transform.position.x, 0 + ((tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / 2) * parentCanvas.scaleFactor));
            }
        }
    }

    //Returns the transform that the tooltip will be under when it is open.
    private Transform GetCurrentParent()
    {
        Transform currentParent = null;

        Transform nextTransformToCheck = transform.parent;
        while (currentParent == null)
        {
            IGalaxyTooltipHandler nextTransformToCheckTooltipHandler = nextTransformToCheck.GetComponent<IGalaxyTooltipHandler>();
            if (nextTransformToCheckTooltipHandler != null)
            {
                if (nextTransformToCheckTooltipHandler.tooltipsParent != null)
                {
                    if (!followsMouse || IsTopmostValidParent(nextTransformToCheck))
                    {
                        currentParent = nextTransformToCheckTooltipHandler.tooltipsParent;
                        break;
                    }
                }
            }

            if (nextTransformToCheck.parent != null)
                nextTransformToCheck = nextTransformToCheck.parent;
            else
            {
                currentParent = transform;
                break;
            }
        }

        return currentParent;
    }

    //Indicates whether the parent transform specified is the topmost parent transform that implements the IGalaxyTooltipHandler interface.
    private bool IsTopmostValidParent(Transform parent)
    {
        Transform transformToCheck = parent;

        while (true)
        {
            if (transformToCheck.parent == null)
                break;
            transformToCheck = transformToCheck.parent;
            IGalaxyTooltipHandler tooltipHandler = transformToCheck.GetComponent<IGalaxyTooltipHandler>();
            if (tooltipHandler != null)
            {
                if(tooltipHandler.tooltipsParent != null)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Opens the tooltip whenever the mouse/pointer enters the trigger zone (2D).
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterTriggerZone();
    }

    /// <summary>
    /// Opens the tooltip whenever the mouse/pointer enters the trigger zone (3D).
    /// </summary>
    private void OnMouseEnter()
    {
        //Ensures that the mouse is not over a 2D ui element.
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        OnPointerEnterTriggerZone();
    }

    /// <summary>
    /// This method is called whenever the pointer enters the trigger zone for the tooltip to open whether or not the component is applied to a 2D or 3D object.
    /// </summary>
    private void OnPointerEnterTriggerZone()
    {
        //Logs that the pointer is in the trigger zone for the tooltip to open.
        PointerInTriggerZone = true;
    }

    /// <summary>
    /// Closes the tooltip whenever the mouse/pointer exits the trigger zone (2D).
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitTriggerZone();
    }

    /// <summary>
    /// Closes the tooltip whenever the mouse/pointer exits the trigger zone (3D).
    /// </summary>
    private void OnMouseExit()
    {
        OnPointerExitTriggerZone();
    }

    /// <summary>
    /// This method is called whenever the pointer exits the trigger zone for the tooltip to open whether or not the component is applied to a 2D or 3D object.
    /// </summary>
    private void OnPointerExitTriggerZone()
    {
        //Logs that the pointer is no longer in the trigger zone for the tooltip to open.
        PointerInTriggerZone = false;
    }

    /// <summary>
    /// Instantiates the tooltip game object from the tooltip prefab, sets the parent of the tooltip, resets essential transfrom components, and updates the text of the tooltip.
    /// </summary>
    private void CreateTooltip()
    {
        //Instantiates the tooltip game object from the tooltip prefab.
        tooltip = Instantiate(tooltipPrefab);
        //Sets the parent of the tooltip.
        if(parent != null)
            tooltip.transform.SetParent(parent);
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

    /// <summary>
    /// Opens the tooltip.
    /// </summary>
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
        Open = true;

        //Triggers the OnTooltipOpen() method if an events handler was specified in the inspector.
        if (eventsHandler != null)
            eventsHandler.OnTooltipOpen(this);
    }

    /// <summary>
    /// Closes the tooltip.
    /// </summary>
    public void CloseTooltip()
    {
        //Deactivates the tooltip's game object if it is not destroyed every time the tooltip closes.
        if (closingType == TooltipClosingType.Deactivate)
        {
            if(Open)
                tooltip.SetActive(false);
        }
        //Destroys the tooltip game object if it is supposed to be destroyed every time the tooltip closes.
        else if (closingType == TooltipClosingType.Destroy)
            Destroy(tooltip);

        //Indicates that the tooltip is closed.
        Open = false;

        //Triggers the OnTooltipClose() method if an events handler was specified in the inspector.
        if (eventsHandler != null)
            eventsHandler.OnTooltipClose(this);
    }

    /// <summary>
    /// Updates the text element of the tooltip and makes sure that the width of the background image adjusts to match the width of the text.
    /// </summary>
    private void UpdateTooltipText()
    {
        Text textElementOfTooltip = tooltip.transform.GetChild(1).GetComponent<Text>();
        //Sets the font of the text of the tooltip.
        textElementOfTooltip.font = Font;
        //Sets the font size of the text of the tooltip.
        if (fontSize > 0)
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
        textElementOfTooltip.text = text;
        //Adjusts the background image of the tooltip to the size of the text of the tooltip.
        tooltip.transform.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(tooltip.transform.GetChild(1).GetComponent<Text>().preferredWidth + edgeBuffer.x, tooltip.transform.GetChild(1).GetComponent<Text>().preferredHeight + edgeBuffer.y);
        //Adjusts the local x position of the text component of the tooltip in order to keep the text centered within the edge buffer.
        tooltip.transform.GetChild(1).localPosition = new Vector2(tooltipPrefab.transform.GetChild(1).localPosition.x + (edgeBuffer.x / 2), tooltip.transform.GetChild(1).localPosition.y);
    }

    /// <summary>
    /// Should be called in order to set the background color of the tooltip through code.
    /// </summary>
    /// <param name="colorOption"></param>
    /// <param name="customColor"></param>
    public void SetBackgroundColor(GalaxyTooltipColorOption colorOption, Color customColor)
    {
        backgroundColorOption = colorOption;
        backgroundCustomColor = customColor;
        if(Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            tooltip.transform.GetChild(0).GetComponent<Image>().color = GetColorFromColorOption(backgroundColorOption, "Background");
    }

    /// <summary>
    /// Should be called in order to set the color of the text of the tooltip through code.
    /// </summary>
    /// <param name="colorOption"></param>
    /// <param name="customColor"></param>
    public void SetTextColor(GalaxyTooltipColorOption colorOption, Color customColor)
    {
        textColorOption = colorOption;
        textCustomColor = customColor;
        if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    /// <summary>
    /// Should be called in order to set if the shadow component of the tooltip's text is enabled or not through code.
    /// </summary>
    /// <param name="isTextShadowEnabled"></param>
    public void SetTextShadowEnabled(bool isTextShadowEnabled)
    {
        textShadowEnabled = isTextShadowEnabled;
        if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    public void SetTextShadowColor(GalaxyTooltipColorOption colorOption, Color customColor)
    {
        textShadowColorOption = colorOption;
        textShadowCustomColor = customColor;
        if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    /// <summary>
    /// Returns the appropriate color depending on the color identifier and what component of the tooltip the color corresponds to.
    /// </summary>
    /// <param name="colorOption"></param>
    /// <param name="componentOfTooltip"></param>
    /// <returns></returns>
    private Color GetColorFromColorOption(GalaxyTooltipColorOption colorOption, string componentOfTooltip)
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
                return NewGalaxyManager.empires[NewGalaxyManager.playerID].color;
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

    /// <summary>
    /// Closes the tooltip if the trigger zone is deactivated/disabled.
    /// </summary>
    private void OnDisable()
    {
        PointerInTriggerZone = false;
    }





    //-------------------------------------------------------------------
    //Custom Inspector.
    //-------------------------------------------------------------------





    #region Editor
    #if UNITY_EDITOR
    [CustomEditor(typeof(GalaxyTooltip)), CanEditMultipleObjects]

    public class GalaxyTooltipEdtior : Editor
    {
        private GalaxyTooltip galaxyTooltip;

        private SerializedProperty propClosingType; //Enum.
        private SerializedProperty propInitialLocalPosition;    //Vector2.
        private SerializedProperty propEdgeBuffer;  //Vector2.
        private SerializedProperty propOpenDelay;   //Float.
        private SerializedProperty propFollowsMouse;    //Bool.
        private SerializedProperty propText;    //String.
        private SerializedProperty propFontOption;  //Enum.
        private SerializedProperty propFont;    //Font.
        private SerializedProperty propFontSizeOption;  //Enum.
        private SerializedProperty propFontSize;    //Int.
        private SerializedProperty propTextShadowEnabled;   //Bool.
        private SerializedProperty propTextShadowEffectDistance;    //Vector2.
        private SerializedProperty propTextColorOption; //Enum.
        private SerializedProperty propTextCustomColor; //Color.
        private SerializedProperty propTextShadowColorOption;   //Enum.
        private SerializedProperty propTextShadowCustomColor;   //Color.
        private SerializedProperty propBackgroundColorOption;   //Enum.
        private SerializedProperty propBackgroundCustomColor;   //Color.
        private SerializedProperty propEventsHandler;   //IGalaxyEventsHandler

        private AnimBool showFontExtraField = new AnimBool(true);
        private AnimBool showFontSizeExtraField = new AnimBool(true);
        private AnimBool showTextShadowExtraFields = new AnimBool(true);
        private AnimBool showTextCustomColor = new AnimBool(true);
        private AnimBool showTextShadowCustomColor = new AnimBool(true);
        private AnimBool showBackgroundCustomColor = new AnimBool(true);

        private bool IsAnyAnimBoolAnimating
        {
            get
            {
                return showFontExtraField.isAnimating || showFontSizeExtraField.isAnimating || showTextShadowExtraFields.isAnimating || showTextCustomColor.isAnimating || showTextShadowCustomColor.isAnimating || showBackgroundCustomColor.isAnimating;
            }
        }

        private void OnEnable()
        {
            galaxyTooltip = target as GalaxyTooltip;

            propClosingType = serializedObject.FindProperty("closingType");
            propInitialLocalPosition = serializedObject.FindProperty("initialLocalPosition");
            propEdgeBuffer = serializedObject.FindProperty("edgeBuffer");
            propOpenDelay = serializedObject.FindProperty("openDelay");
            propFollowsMouse = serializedObject.FindProperty("followsMouse");
            propText = serializedObject.FindProperty("text");
            propFontOption = serializedObject.FindProperty("fontOption");
            propFont = serializedObject.FindProperty("font");
            propFontSizeOption = serializedObject.FindProperty("fontSizeOption");
            propFontSize = serializedObject.FindProperty("fontSize");
            propTextShadowEnabled = serializedObject.FindProperty("textShadowEnabled");
            propTextShadowEffectDistance = serializedObject.FindProperty("textShadowEffectDistance");
            propTextColorOption = serializedObject.FindProperty("textColorOption");
            propTextCustomColor = serializedObject.FindProperty("textCustomColor");
            propTextShadowColorOption = serializedObject.FindProperty("textShadowColorOption");
            propTextShadowCustomColor = serializedObject.FindProperty("textShadowCustomColor");
            propBackgroundColorOption = serializedObject.FindProperty("backgroundColorOption");
            propBackgroundCustomColor = serializedObject.FindProperty("backgroundCustomColor");
            propEventsHandler = serializedObject.FindProperty("eventsHandler");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            //GalaxyTooltip galaxyTooltip = (GalaxyTooltip)target;

            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            //---------------------------------------------
            //Logic Options Section.
            //---------------------------------------------

            EditorGUILayout.LabelField("Logic Options", EditorStyles.boldLabel);

            //Closing Type Enum.

            propClosingType.enumValueIndex = (int)(TooltipClosingType)EditorGUILayout.EnumPopup("Closing Type", galaxyTooltip.closingType);

            //Initital Local Position Vector2.

            propInitialLocalPosition.vector2Value = EditorGUILayout.Vector2Field("Initial Local Position", galaxyTooltip.initialLocalPosition);

            //Edge Buffer Vector2.

            propEdgeBuffer.vector2Value = EditorGUILayout.Vector2Field("Edge Buffer", galaxyTooltip.edgeBuffer);

            //Open Delay Float.

            propOpenDelay.floatValue = EditorGUILayout.Slider("Open Delay", galaxyTooltip.openDelay, 0, 2);
            //propOpenDelay.floatValue = EditorGUILayout.FloatField("Open Delay", galaxyTooltip.openDelay);

            //Follows Mouse Bool.

            propFollowsMouse.boolValue = EditorGUILayout.Toggle("Follows Mouse", galaxyTooltip.followsMouse);

            //---------------------------------------------
            //Text Content Section.
            //---------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Content", EditorStyles.boldLabel);

            //Text String.

            EditorGUILayout.LabelField("Text");
            propText.stringValue = EditorGUILayout.TextArea(galaxyTooltip.text, GUILayout.MaxHeight(75));

            //Font Font.

            propFontOption.enumValueIndex = (int)(DefaultOrCustomOption)EditorGUILayout.EnumPopup("Font", galaxyTooltip.fontOption);
            if (propFontOption.enumValueIndex == (int)DefaultOrCustomOption.Default)
                galaxyTooltip.font = null;
            showFontExtraField.target = propFontOption.enumValueIndex == (int)DefaultOrCustomOption.Custom;
            if (EditorGUILayout.BeginFadeGroup(showFontExtraField.faded))
            {
                EditorGUI.indentLevel++;
                propFont.objectReferenceValue = (Font)EditorGUILayout.ObjectField(galaxyTooltip.font, typeof(Font), true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            //Font Size Int.

            propFontSizeOption.enumValueIndex = (int)(DefaultOrCustomOption)EditorGUILayout.EnumPopup("Font Size", galaxyTooltip.fontSizeOption);
            if (propFontSizeOption.enumValueIndex == (int)DefaultOrCustomOption.Default)
                galaxyTooltip.fontSize = 0;
            showFontSizeExtraField.target = propFontSizeOption.enumValueIndex == (int)DefaultOrCustomOption.Custom;
            if (EditorGUILayout.BeginFadeGroup(showFontSizeExtraField.faded))
            {
                EditorGUI.indentLevel++;
                propFontSize.intValue = EditorGUILayout.IntField(galaxyTooltip.fontSize);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            //---------------------------------------------
            //Text Shadow Section.
            //---------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Shadow", EditorStyles.boldLabel);

            //Text Shadow Enabled Bool.

            propTextShadowEnabled.boolValue = EditorGUILayout.Toggle("Text Shadow Enabled", galaxyTooltip.textShadowEnabled);

            //Text Shadow Effect Distance Vector2.

            showTextShadowExtraFields.target = propTextShadowEnabled.boolValue;
            if (EditorGUILayout.BeginFadeGroup(showTextShadowExtraFields.faded))
            {
                EditorGUI.indentLevel++;
                propTextShadowEffectDistance.vector2Value = EditorGUILayout.Vector2Field("Effect Distance", galaxyTooltip.textShadowEffectDistance);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            //---------------------------------------------
            //Coloring Options Section.
            //---------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Coloring Options", EditorStyles.boldLabel);

            //Text Color Option GalaxyTooltipColorOption.

            propTextColorOption.enumValueIndex = (int)(GalaxyTooltipColorOption)EditorGUILayout.EnumPopup("Text Color", galaxyTooltip.textColorOption);
            showTextCustomColor.target = propTextColorOption.enumValueIndex == (int)GalaxyTooltipColorOption.CustomColor;

            //Text Custom Color Color.

            if (EditorGUILayout.BeginFadeGroup(showTextCustomColor.faded))
            {
                EditorGUI.indentLevel++;
                propTextCustomColor.colorValue = EditorGUILayout.ColorField(galaxyTooltip.textCustomColor);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            //Text Shadow Color Option GalaxyTooltipColorOption.

            propTextShadowColorOption.enumValueIndex = (int)(GalaxyTooltipColorOption)EditorGUILayout.EnumPopup("Text Shadow Color", galaxyTooltip.textShadowColorOption);
            showTextShadowCustomColor.target = propTextShadowColorOption.enumValueIndex == (int)GalaxyTooltipColorOption.CustomColor;

            //Text Shadow Custom Color Color.

            if (EditorGUILayout.BeginFadeGroup(showTextShadowCustomColor.faded))
            {
                EditorGUI.indentLevel++;
                propTextShadowCustomColor.colorValue = EditorGUILayout.ColorField(galaxyTooltip.textShadowCustomColor);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            //Background Color Option GalaxyTooltipColorOption.

            propBackgroundColorOption.enumValueIndex = (int)(GalaxyTooltipColorOption)EditorGUILayout.EnumPopup("Background Color", galaxyTooltip.backgroundColorOption);
            showBackgroundCustomColor.target = propBackgroundColorOption.enumValueIndex == (int)GalaxyTooltipColorOption.CustomColor;

            //Background Custom Color Color.

            if (EditorGUILayout.BeginFadeGroup(showBackgroundCustomColor.faded))
            {
                EditorGUI.indentLevel++;
                propBackgroundCustomColor.colorValue = EditorGUILayout.ColorField(galaxyTooltip.backgroundCustomColor);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            //---------------------------------------------
            //Optional Components Section.
            //---------------------------------------------

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Optional Components", EditorStyles.boldLabel);

            //Events Handler IGalaxyTooltipEventsHandler

            propEventsHandler.objectReferenceValue = (GalaxyTooltipEventsHandler)EditorGUILayout.ObjectField("Events Handler", galaxyTooltip.eventsHandler, typeof(GalaxyTooltipEventsHandler), true);

            EditorGUILayout.EndVertical();

            if(GUI.changed)
                serializedObject.ApplyModifiedProperties();

            if(IsAnyAnimBoolAnimating)
                EditorUtility.SetDirty(target);
        }
    }
    #endif
    #endregion
}