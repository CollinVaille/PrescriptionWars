using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PillSkinOptionButton : MonoBehaviour
{
    [Header("Pill Skin Option Button Components")]

    [SerializeField] private RawImage pillSkinRawImage = null;
    [SerializeField] private RawImage pillViewRawImage = null;

    [SerializeField] private Image backgroundImage = null;

    [Header("Pill Skin Option Button Options")]

    [SerializeField] private Texture2D mouseOverPillViewCursor = null;
    public Texture2D MouseOverPillViewCursor
    {
        get
        {
            return mouseOverPillViewCursor;
        }
        set
        {
            mouseOverPillViewCursor = value;
        }
    }

    [SerializeField] private float pillViewRotationSpeed = 2.5f;
    public float PillViewRotationSpeed
    {
        get
        {
            return pillViewRotationSpeed;
        }
        set
        {
            pillViewRotationSpeed = value;
        }
    }

    //Non-inspector variables.

    private bool hasBecameVisible = false;

    private string pillSkinName = null;

    private PillView pillView = null;
    public PillView PillView
    {
        get
        {
            return pillView;
        }
        set
        {
            pillView = value;
        }
    }

    private GalaxyPillSkinConfirmationPopup assignedConfirmationPopup = null;

    private float LocalPositionX
    {
        get
        {
            return 95 + (195 * transform.GetSiblingIndex());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Detects if the pill skin option is visible for the first time and executes the appropriate loading logic if true.
        if (!hasBecameVisible && LocalPositionX - (((RectTransform)transform).sizeDelta.x / 2) <= Mathf.Abs(transform.parent.localPosition.x) + 415)
        {
            OnOptionBecameVisible();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //This method is called whenever the scrollbar of the pill skin confirmation popup moves or changes value.
    public void OnScrollbarValueChange()
    {
        //Detects if the pill skin option is visible for the first time and executes the appropriate loading logic if true.
        if (!hasBecameVisible && LocalPositionX - (((RectTransform)transform).sizeDelta.x / 2) <= Mathf.Abs(transform.parent.transform.localPosition.x) + 415)
        {
            OnOptionBecameVisible();
        }
    }

    //This method is called whenever the pill skin option button becomes visible to the user.
    private void OnOptionBecameVisible()
    {
        //Assigns the texture of the pill skin raw image.
        pillSkinRawImage.texture = pillSkinName != null ? Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(Empire.empires[GalaxyManager.PlayerID].empireCulture.ToString()) + "/" + pillSkinName).mainTexture : null;
        if (pillSkinName == null)
            pillSkinRawImage.gameObject.SetActive(false);

        //Creates the pill view and assigns the pill view raw image texture.
        if(pillSkinName != null)
        {
            //Test pill creation.
            GalaxyPill testPill = new GalaxyPill("Test Pill", "Assault");
            GalaxySquad testSquad = new GalaxySquad("Test Squad");
            testSquad.AddPill(testPill);
            GalaxyArmy testArmy = new GalaxyArmy("Test Army", GalaxyManager.PlayerID, pillSkinName);
            testArmy.AddSquad(testSquad);

            //Pill view creation.
            pillView = PillViewsManager.GetNewPillView(testPill);
            pillViewRawImage.texture = pillView.RenderTexture;
        }
        else
        {
            pillViewRawImage.gameObject.SetActive(false);
        }

        //Logs that this pill skin option button has became visible.
        hasBecameVisible = true;
    }

    //This method is called whenever the pill skin option button is clicked.
    public void OnPointerClick()
    {
        assignedConfirmationPopup.SetPillSkinSelected(transform.GetSiblingIndex());
    }

    //This method should be called in order to assign a color to the background image.
    public void SetBackgroundImageColor(Color color)
    {
        backgroundImage.color = color;
    }

    //This method is called whenever the pill skin option button is destroyed.
    private void OnDestroy()
    {
        if (pillView != null)
            pillView.Delete();
    }

    //This method should be called in order to create a new pill skin option button.
    public static PillSkinOptionButton CreatePillSkinOptionButton(GameObject prefab, Transform parent, GalaxyPillSkinConfirmationPopup assignedConfirmationPopup, string pillSkinName)
    {
        GameObject pillSkinOptionButton = Instantiate(prefab);
        PillSkinOptionButton pillSkinOptionButtonScript = pillSkinOptionButton.GetComponent<PillSkinOptionButton>();

        pillSkinOptionButtonScript.transform.SetParent(parent);
        pillSkinOptionButtonScript.transform.localScale = Vector3.one;

        pillSkinOptionButtonScript.assignedConfirmationPopup = assignedConfirmationPopup;

        pillSkinOptionButtonScript.pillSkinName = pillSkinName;

        return pillSkinOptionButtonScript;
    }
}
