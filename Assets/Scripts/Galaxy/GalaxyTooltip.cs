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

    //Indicates how the tooltip will close (see the enum definition above to get more details on the closing types).
    //[Tooltip("Indicates how the tooltip will close (see the enum definition above to get more details on the closing types).")]
    [SerializeField, HideInInspector]
    private TooltipClosingType closingType = TooltipClosingType.Destroy;

    //Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.
    //[SerializeField, Tooltip("Indicates the inital position that the tooltip will spawn at when it is created, but if the tooltip's position is supposed to update to the mouse's position every update then it will indicate the offset between the mouse and the tooltip.")]
    [SerializeField, HideInInspector]
    private Vector2 initialLocalPosition = new Vector2(5, -15);

    //Indicates the amount of space between the text of the tooltip and the edge of the tooltip.
    //[SerializeField, Tooltip("Indicates the amount of space between the text of the tooltip and the edge of the tooltip.")]
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

    //Indicates whether the tooltip will have its location set to the mouse's location every update.
    //[Tooltip("Indicates whether the tooltip will have its location set to the mouse's location every update.")]
    [HideInInspector]
    public bool followsMouse = true;

    [Header("Text Content")]

    //The text that the tooltip displays to the user.
    //[SerializeField, TextArea, Tooltip("The text that the tooltip displays to the user.")]
    [SerializeField, HideInInspector]
    private string text = "";
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

    //The font that the text of the tooltip will be.
    //[SerializeField, Tooltip("The font that the text of the tooltip will be.")]
    [SerializeField, HideInInspector]
    private DefaultOrCustomOption fontOption = DefaultOrCustomOption.Default;
    [SerializeField, HideInInspector]
    private Font font = null;
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

    //Indicates the size of the font of the tooltip text.
    //[SerializeField, Tooltip("Indicates the size of the font of the tooltip text.")]
    [SerializeField, HideInInspector]
    private DefaultOrCustomOption fontSizeOption = DefaultOrCustomOption.Default;
    [SerializeField, HideInInspector]
    private int fontSize = 0;
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

    //[Header("Text Shadow")]

    //Indicates whether the shadow component of the tooltip's text is enabled.
    //[Tooltip("Indicates whether the shadow component of the tooltip's text is enabled.")]
    [SerializeField, HideInInspector]
    private bool textShadowEnabled = false;

    //Indicates the effect distance of the shadow component of the tooltip's text.
    //[SerializeField, Tooltip("Indicates the effect distance of the shadow component of the tooltip's text."), ConditionalField("textShadowEnabled", true, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    [SerializeField, HideInInspector]
    private Vector2 textShadowEffectDistance = new Vector2(1, -1);
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

    public enum GalaxyTooltipColorOption
    {
        Default,
        Transparent,
        PlayerEmpireColor,
        CustomColor
    }

    //[Header("Coloring Options")]

    //Indicates the color of the text of the tooltip.
    //[SerializeField, LabelOverride("Text Color"), Tooltip("Indicates the color of the text of the tooltip.")]
    [SerializeField, HideInInspector]
    private GalaxyTooltipColorOption textColorOption = GalaxyTooltipColorOption.Default;

    //[SerializeField, ConditionalField("textColorOption", GalaxyTooltipColorOption.CustomColor, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    [SerializeField, HideInInspector]
    private Color textCustomColor = Color.white;

    //Indicates the color of the shadow component of the tooltip's text.
    //[LabelOverride("Text Shadow Color"), Tooltip("Indicates the color of the shadow component of the tooltip's text.")]
    [SerializeField, HideInInspector]
    private GalaxyTooltipColorOption textShadowColorOption = GalaxyTooltipColorOption.Default;

    //[SerializeField, ConditionalField("textShadowColorOption", GalaxyTooltipColorOption.CustomColor, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    [SerializeField, HideInInspector]
    private Color textShadowCustomColor = Color.white;

    //Indicates the background color of the tooltip.
    //[LabelOverride("Background Color"), Tooltip("Indicates the background color of the tooltip.")]
    [SerializeField, HideInInspector]
    private GalaxyTooltipColorOption backgroundColorOption = GalaxyTooltipColorOption.Default;

    //[SerializeField, ConditionalField("backgroundColorOption", GalaxyTooltipColorOption.CustomColor, ConditionalFieldComparisonType.Equals, ConditionalFieldDisablingType.Disappear)]
    [SerializeField, HideInInspector]
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

    //The parent transform of the tooltip (set in the start method of the tooltip).
    private Transform parent = null;
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

    //The camera that this tooltip will be viewed by.
    private Camera sceneCamera = null;

    //Indicates whether the tooltip is open or not.
    public bool Open { get; private set; } = false;

    // Start is called before the first frame update
    private void Start()
    {
        parentCanvas = GetCurrentParentCanvas();
        parent = GetCurrentParent();
        sceneCamera = GetCurrentSceneCamera();
    }

    // Update is called once per frame
    private void Update()
    {
        //Updates the position of the tooltip to the position of the player's mouse if the tooltip is open.
        if (Open && followsMouse)
        {
            tooltip.transform.position = Input.mousePosition;
            tooltip.transform.localPosition = new Vector2(tooltip.transform.localPosition.x + initialLocalPosition.x, tooltip.transform.localPosition.y + (initialLocalPosition.y - (edgeBuffer.y / 2)));
            PreventTooltipGoingOffscreen();
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
                if (nextTransformToCheckTooltipHandler.TooltipsParent != null)
                {
                    if (!followsMouse || IsTopmostValidParent(nextTransformToCheck))
                    {
                        currentParent = nextTransformToCheckTooltipHandler.TooltipsParent;
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

    //Returns the camera of the scene that the tooltip is currently in.
    private Camera GetCurrentSceneCamera()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Galaxy":
                return GalaxyManager.GalaxyCamera;
            case "Main Menu":
                return MainMenu.SceneCamera;

            default:
                return null;
        }
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
                if(tooltipHandler.TooltipsParent != null)
                    return false;
            }
        }

        return true;
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
        Open = true;
    }

    //Closes the tooltip.
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
    }

    //Updates the text element of the tooltip and makes sure that the width of the background image adjusts to match the width of the text.
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

    //Should be called in order to set the background color of the tooltip through code.
    public void SetBackgroundColor(GalaxyTooltipColorOption colorOption, Color customColor)
    {
        backgroundColorOption = colorOption;
        backgroundCustomColor = customColor;
        if(Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            tooltip.transform.GetChild(0).GetComponent<Image>().color = GetColorFromColorOption(backgroundColorOption, "Background");
    }

    //Should be called in order to set the color of the text of the tooltip through code.
    public void SetTextColor(GalaxyTooltipColorOption colorOption, Color customColor)
    {
        textColorOption = colorOption;
        textCustomColor = customColor;
        if (Open || (closingType == TooltipClosingType.Deactivate && tooltip != null))
            UpdateTooltipText();
    }

    //Should be called in order to set if the shadow component of the tooltip's text is enabled or not through code.
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

    //Returns the appropriate color depending on the color identifier and what component of the tooltip the color corresponds to.
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
                return Empire.empires[GalaxyManager.PlayerID].EmpireColor;
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

    //Closes the tooltip if the trigger zone is deactivated/disabled.
    private void OnDisable()
    {
        CloseTooltip();
    }

    //Custom Inspector.
    #region Editor
    #if UNITY_EDITOR
    [CustomEditor(typeof(GalaxyTooltip)), CanEditMultipleObjects]

    public class GalaxyTooltipEdtior : Editor
    {
        private GalaxyTooltip galaxyTooltip;

        private SerializedProperty propClosingType; //Enum.
        private SerializedProperty propInitialLocalPosition;    //Vector2.
        private SerializedProperty propEdgeBuffer;  //Vector2.
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
