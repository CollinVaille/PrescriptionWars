using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialPillConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Special Pill Confirmation Popup Components")]

    [SerializeField] private GameObject specialPillOptionButtonPrefab = null;

    [SerializeField] private Transform scrollListContent = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip pointerEnterSpecialPillOptionButtonSFX = null;
    [SerializeField] private AudioClip changeSpecialPillSelectedSFX = null;

    /// <summary>
    /// Public int property that is access only and returns the final special pill id that the user selected.
    /// </summary>
    public int returnValue { get => specialPillIDSelected; }
    private int specialPillIDSelected = -1;

    /// <summary>
    /// Public int property that should be used both to access and mutate what skill the confirmation popup should show each special pill's experience level with.
    /// </summary>
    public int skillSelectedIndex
    {
        get => skillSelectedIndexVar;
        set
        {
            skillSelectedIndexVar = value;
            for(int childIndex = 0; childIndex < scrollListContent.childCount; childIndex++)
            {
                scrollListContent.GetChild(childIndex).GetComponent<SpecialPillOptionButton>().skillSelectedIndex = value;
            }
        }
    }
    private int skillSelectedIndexVar = -1;

    private bool created = false;

    /// <summary>
    /// The prefab that all special pill confirmation popups should be instantiated from (set in the GalaxyGenerator).
    /// </summary>
    public static GameObject specialPillConfirmationPopupPrefab = null;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (!created)
        {
            CreateConfirmationPopup("Test", 0);
            created = true;
        }
    }

    /// <summary>
    /// This method should be called in order to properly create the confirmation popup, set the text components, and populate the scroll list.
    /// </summary>
    /// <param name="topText"></param>
    public void CreateConfirmationPopup(string topText, int skillSelectedIndex, int specialPillIDSelected = -1)
    {
        CreateConfirmationPopup(topText);

        GalaxySpecialPill specialPill = new GalaxySpecialPill("Bob", "Assault", Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(Empire.empires[GalaxyManager.PlayerID].empireCulture.ToString()) + "/" + Empire.empires[GalaxyManager.PlayerID].GetRandomPillSkinName()), null, Empire.empires[GalaxyManager.PlayerID]);
        skillSelectedIndexVar = skillSelectedIndex;
        PopulateScrollList();
        ChangeSpecialPillIDSelected(specialPillIDSelected);
    }

    /// <summary>
    /// This method should be called by the CreateConfirmationPopup method in order to populate the scroll list with option buttons presenting the player empire's special pills.
    /// </summary>
    private void PopulateScrollList()
    {
        List<GalaxySpecialPill> availableSpecialPills = new List<GalaxySpecialPill>();
        List<GalaxySpecialPill> busySpecialPills = new List<GalaxySpecialPill>();
        foreach (GalaxySpecialPill specialPill in Empire.empires[GalaxyManager.PlayerID].specialPillsList)
        {
            if (specialPill.isBusy)
                busySpecialPills.Add(specialPill);
            else
                availableSpecialPills.Add(specialPill);
        }
        foreach(GalaxySpecialPill availableSpecialPill in availableSpecialPills)
        {
            SpecialPillOptionButton specialPillOptionButton = Instantiate(specialPillOptionButtonPrefab).GetComponent<SpecialPillOptionButton>();
            specialPillOptionButton.transform.SetParent(scrollListContent);
            specialPillOptionButton.transform.localScale = Vector3.one;
            specialPillOptionButton.specialPillConfirmationPopup = this;
            specialPillOptionButton.specialPillID = availableSpecialPill.specialPillID;
            specialPillOptionButton.skillSelectedIndex = skillSelectedIndex;
        }
        foreach(GalaxySpecialPill busySpecialPill in busySpecialPills)
        {
            SpecialPillOptionButton specialPillOptionButton = Instantiate(specialPillOptionButtonPrefab).GetComponent<SpecialPillOptionButton>();
            specialPillOptionButton.transform.SetParent(scrollListContent);
            specialPillOptionButton.transform.localScale = Vector3.one;
            specialPillOptionButton.specialPillConfirmationPopup = this;
            specialPillOptionButton.specialPillID = busySpecialPill.specialPillID;
            specialPillOptionButton.skillSelectedIndex = skillSelectedIndex;
        }
    }

    /// <summary>
    /// This method should be called via the special pill option button whenever it is clicked in order to set its special pill as the one that is selected.
    /// </summary>
    /// <param name="specialPillID"></param>
    public void OnClickSpecialPillOptionButton(int specialPillID)
    {
        if (specialPillID == specialPillIDSelected)
            return;
        ChangeSpecialPillIDSelected(specialPillID);
        AudioManager.PlaySFX(changeSpecialPillSelectedSFX);
    }

    private void ChangeSpecialPillIDSelected(int specialPillID)
    {
        for (int childIndex = 0; childIndex < scrollListContent.childCount; childIndex++)
        {
            if (scrollListContent.GetChild(childIndex).GetComponent<SpecialPillOptionButton>().specialPillID == specialPillIDSelected)
            {
                scrollListContent.GetChild(childIndex).GetComponent<SpecialPillOptionButton>().selected = false;
                break;
            }
        }
        specialPillIDSelected = specialPillID;
        for (int childIndex = 0; childIndex < scrollListContent.childCount; childIndex++)
        {
            if (scrollListContent.GetChild(childIndex).GetComponent<SpecialPillOptionButton>().specialPillID == specialPillID)
            {
                scrollListContent.GetChild(childIndex).GetComponent<SpecialPillOptionButton>().selected = true;
                break;
            }
        }
    }

    /// <summary>
    /// This method should be called (via the special pill option button) whenever the player hovers over a special pill option button and plays the appropriate sound effect if the button was no already selected.
    /// </summary>
    public void OnPointerEnterSpecialPillOptionButton(SpecialPillOptionButton specialPillOptionButton)
    {
        if(!specialPillOptionButton.selected)
            AudioManager.PlaySFX(pointerEnterSpecialPillOptionButtonSFX);
    }
}
