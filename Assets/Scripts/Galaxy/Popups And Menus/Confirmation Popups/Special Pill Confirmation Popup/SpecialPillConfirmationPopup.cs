using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    }

    /// <summary>
    /// This method should be called in order to properly create the confirmation popup, set the text components, and populate the scroll list.
    /// </summary>
    /// <param name="topText"></param>
    public void CreateConfirmationPopup(string topText, int skillSelectedIndex = -1, int specialPillIDSelected = -1)
    {
        base.CreateConfirmationPopup(topText);

        GalaxySpecialPill specialPill = new GalaxySpecialPill("Bob", "Assault", Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(Empire.empires[GalaxyManager.playerID].empireCulture.ToString()) + "/" + Empire.empires[GalaxyManager.playerID].GetRandomPillSkinName()), null, Empire.empires[GalaxyManager.playerID]);
        skillSelectedIndexVar = skillSelectedIndex;
        PopulateScrollList();
        ChangeSpecialPillIDSelected(specialPillIDSelected);
    }

    /// <summary>
    /// This method should be called by the CreateConfirmationPopup method in order to populate the scroll list with option buttons presenting the player empire's special pills.
    /// </summary>
    private void PopulateScrollList()
    {
        //Deletes any previously existing special pill option buttons.
        if(scrollListContent.childCount > 0)
        {
            for(int childIndex = scrollListContent.childCount - 1; childIndex >= 0; childIndex--)
            {
                Destroy(scrollListContent.GetChild(childIndex).gameObject);
            }
        }

        //Creates the none button at the top of the scroll list.
        SpecialPillOptionButton noneButton = Instantiate(specialPillOptionButtonPrefab).GetComponent<SpecialPillOptionButton>();
        noneButton.transform.SetParent(scrollListContent);
        noneButton.transform.localScale = Vector3.one;
        noneButton.specialPillConfirmationPopup = this;
        noneButton.specialPillID = -1;
        noneButton.skillSelectedIndex = skillSelectedIndex;
        noneButton.selected = noneButton.specialPillID == specialPillIDSelected;

        //Sort all special pills belonging to the player empire into two separate lists saying whether or not they have already been assigned a task.
        List<GalaxySpecialPill> availableSpecialPills = new List<GalaxySpecialPill>();
        List<GalaxySpecialPill> busySpecialPills = new List<GalaxySpecialPill>();
        foreach (GalaxySpecialPill specialPill in Empire.empires[GalaxyManager.playerID].specialPillsList)
        {
            if (specialPill.isBusy)
                busySpecialPills.Add(specialPill);
            else
                availableSpecialPills.Add(specialPill);
        }

        //Special pills that are available and not busy with a task have their buttons added to the scroll list before the special pills that are assigned a task and are busy.
        foreach(GalaxySpecialPill availableSpecialPill in availableSpecialPills)
        {
            SpecialPillOptionButton specialPillOptionButton = Instantiate(specialPillOptionButtonPrefab).GetComponent<SpecialPillOptionButton>();
            specialPillOptionButton.transform.SetParent(scrollListContent);
            specialPillOptionButton.transform.localScale = Vector3.one;
            specialPillOptionButton.specialPillConfirmationPopup = this;
            specialPillOptionButton.specialPillID = availableSpecialPill.specialPillID;
            specialPillOptionButton.skillSelectedIndex = skillSelectedIndex;
            specialPillOptionButton.selected = specialPillOptionButton.specialPillID == specialPillIDSelected;
        }

        //Special pills that are busy with a task are added to the scroll list last, after the special pills that are available and not assigned a task.
        foreach(GalaxySpecialPill busySpecialPill in busySpecialPills)
        {
            SpecialPillOptionButton specialPillOptionButton = Instantiate(specialPillOptionButtonPrefab).GetComponent<SpecialPillOptionButton>();
            specialPillOptionButton.transform.SetParent(scrollListContent);
            specialPillOptionButton.transform.localScale = Vector3.one;
            specialPillOptionButton.specialPillConfirmationPopup = this;
            specialPillOptionButton.specialPillID = busySpecialPill.specialPillID;
            specialPillOptionButton.skillSelectedIndex = skillSelectedIndex;
            specialPillOptionButton.selected = specialPillOptionButton.specialPillID == specialPillIDSelected;
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
        if(!specialPillOptionButton.selected && specialPillOptionButton.gameObject.GetComponent<Button>().interactable)
            AudioManager.PlaySFX(pointerEnterSpecialPillOptionButtonSFX);
    }
}
