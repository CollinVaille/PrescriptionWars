using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPillSkinConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Pill Skin Confirmation Popup Components")]

    [SerializeField] private Text bodyText = null;

    [SerializeField] private Transform scrollListContent = null;

    [Header("Pill Skin Confirmation Popup Options")]

    [SerializeField] private GameObject pillSkinOptionButtonPrefab = null;

    //Non-inspector variables.

    private Material[] pillSkins = null;

    private int pillSkinSeletced = 0;
    public Material ReturnValue
    {
        get
        {
            return pillSkins[pillSkinSeletced];
        }
    }

    public static GameObject galaxyPillSkinConfirmationPopupPrefab = null;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    //This method is called whenever the value of the scrollbar changes (when the scrollbar moves).
    public void OnScrollbarValueChange()
    {
        for(int contentIndex = 0; contentIndex < scrollListContent.childCount; contentIndex++)
        {
            if (scrollListContent.GetChild(contentIndex).GetComponent<PillSkinOptionButton>() != null)
            {
                scrollListContent.GetChild(contentIndex).GetComponent<PillSkinOptionButton>().OnScrollbarValueChange();
            }
        }
    }

    //This method should be called in order to properly create the confirmation popup and set the text components.
    public void CreateConfirmationPopup(string topText, string bodyText, Material[] pillSkins)
    {
        CreateConfirmationPopup(topText);
        this.bodyText.text = bodyText;
        this.pillSkins = pillSkins;

        if (pillSkins != null)
            PopulateScrollList();
    }

    //This method should be called in order to populate the scroll list with pill skin option buttons.
    private void PopulateScrollList()
    {
        foreach (Material pillSkin in pillSkins)
        {
            PillSkinOptionButton.CreatePillSkinOptionButton(pillSkinOptionButtonPrefab, scrollListContent, this, pillSkin);
        }
    }

    //This method is called whenever a pill skin option button is clicked and sets the pill skin selected to the pill skin at the specified index.
    public void SetPillSkinSelected(int index)
    {
        scrollListContent.GetChild(pillSkinSeletced).GetComponent<PillSkinOptionButton>().SetBackgroundImageColor(new Color(82 / 255.0f, 137 / 255.0f, 126 / 255.0f, 1));

        pillSkinSeletced = index;

        scrollListContent.GetChild(pillSkinSeletced).GetComponent<PillSkinOptionButton>().SetBackgroundImageColor(new Color(68 / 255.0f, 99 / 255.0f, 93 / 255.0f, 1));
    }

    //This method is called in order to set the pill skin selected index to the index that the specified pill skin is located at.
    public void SetPillSkinSelected(Material pillSkin)
    {
        int skinIndex = -1;
        for(int x = 0; x < pillSkins.Length; x++)
        {
            if(pillSkin == pillSkins[x])
            {
                skinIndex = x;
                break;
            }
        }
        if (skinIndex < 0)
            return;

        scrollListContent.GetChild(pillSkinSeletced).GetComponent<PillSkinOptionButton>().SetBackgroundImageColor(new Color(82 / 255.0f, 137 / 255.0f, 126 / 255.0f, 1));

        pillSkinSeletced = skinIndex;

        scrollListContent.GetChild(pillSkinSeletced).GetComponent<PillSkinOptionButton>().SetBackgroundImageColor(new Color(68 / 255.0f, 99 / 255.0f, 93 / 255.0f, 1));
    }
}
