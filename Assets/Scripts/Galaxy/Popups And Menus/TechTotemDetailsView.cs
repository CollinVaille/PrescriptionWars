using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechTotemDetailsView : GalaxyMenuBehaviour
{
    public enum TechDetailsViewDisplayMode
    {
        TechsAvailable,
        TechsCompleted
    }
    private TechDetailsViewDisplayMode displayMode;

    private int techTotemDisplayed;

    [Header("View Components")]

    [SerializeField]
    private Text titleText = null;

    [SerializeField]
    private Dropdown displayModeDropdown = null;

    [Header("View Options")]

    [SerializeField]
    [Tooltip("Indicates whether the view will display techs available or techs completed every time the view in enabled.")]
    private TechDetailsViewDisplayMode defaultDisplayMode = TechDetailsViewDisplayMode.TechsAvailable;

    [Header("Scroll List Components")]

    [SerializeField]
    [Tooltip("The transform component that will be the parent of all of the buttons in the scroll list.")]
    private Transform scrollListContentTransform = null;

    [SerializeField]
    [Tooltip("The prefab that the tech details buttons in the view's scroll list will be instatiated from.")]
    private GameObject techDetailsButtonPrefab = null;

    [Header("SFX Options")]

    [SerializeField]
    [Tooltip("The sound effect that will be played whenever the mouse hovers over an option of a dropdown.")]
    private AudioClip mouseOverDropdownOptionSFX = null;

    [SerializeField]
    [Tooltip("The sound effect that will be played whenever the player clicks on an option of a dropdown.")]
    private AudioClip clickDropdownOptionSFX = null;

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

    public override void SwitchToPreviousMenu()
    {
        //Executes the logic of the base class for switching to the previous menu.
        base.SwitchToPreviousMenu();

        //Clears the scroll list of all of the tech details buttons.
        ClearScrollList();
    }

    //Sets the display mode of the view and the scroll list to either show the techs available or techs completed of the currently selected tech totem.
    public void SetDisplayMode(TechDetailsViewDisplayMode newDisplayMode, bool forceUpdateScrollList)
    {
        if(newDisplayMode != displayMode || forceUpdateScrollList)
        {
            //Sets the variable that indicates the display mode to the specified value.
            displayMode = newDisplayMode;

            //Updates the scroll list.
            UpdateScrollList();
        }
    }

    //Updates the scroll list by clearing it and then populating it.
    private void UpdateScrollList()
    {
        //Clears the view's scroll list of any child buttons.
        ClearScrollList();
        //Populates the view's scroll list with the appropriate child buttons.
        PopulateScrollList();
    }

    //Clears the view's scroll list of any child buttons.
    private void ClearScrollList()
    {
        for(int childIndex = scrollListContentTransform.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(scrollListContentTransform.GetChild(childIndex).gameObject);
        }
    }

    //Populates the view's scroll list with the appropriate child buttons.
    private void PopulateScrollList()
    {
        List<int> techsToDisplayByID = new List<int>();

        if(displayMode == TechDetailsViewDisplayMode.TechsAvailable)
        {
            techsToDisplayByID = new List<int>(Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemDisplayed].techsAvailable);
        }
        else if (displayMode == TechDetailsViewDisplayMode.TechsCompleted)
        {
            techsToDisplayByID = new List<int>(Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemDisplayed].techsCompleted);
        }
        
        techsToDisplayByID = Tech.GetSortedTechIDListByLevel(techsToDisplayByID);

        foreach(int techID in techsToDisplayByID)
        {
            GameObject techDetailsButton = Instantiate(techDetailsButtonPrefab);
            TechDetailsButton techDetailsButtonScript = techDetailsButton.GetComponent<TechDetailsButton>();

            techDetailsButtonScript.SetBackgroundSpriteName(Tech.entireTechList[techID].spriteName);
            techDetailsButtonScript.SetTechNameText(Tech.entireTechList[techID].name);
            techDetailsButtonScript.SetTechDescriptionText(Tech.entireTechList[techID].description);
            techDetailsButtonScript.SetTechLevelText(Tech.entireTechList[techID].level.ToString());
            techDetailsButtonScript.SetTechCostText(Tech.entireTechList[techID].cost.ToString());

            techDetailsButton.transform.SetParent(scrollListContentTransform);
            techDetailsButton.name = Tech.entireTechList[techID].name + " Tech Details Button";
            techDetailsButton.transform.localScale = Vector3.one;
        }
    }

    //This method is called upon the switch to this view.
    public void OnSwitchToTechTotemDetailsView(int newTechTotemDisplayed)
    {
        //Sets the variable that indicates the tech totem that is having its tech displayed to the specified value.
        techTotemDisplayed = newTechTotemDisplayed;
        //Sets the title text of the view to be the name of the tech totem that is having the details of its tech displayed.
        titleText.text = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemDisplayed].name;

        //Sets the display mode of the view to the default display mode and forced the update of the scroll list.
        SetDisplayMode(defaultDisplayMode, true);
        //Sets the value of the display mode dropdown to represent the default display mode being selected.
        displayModeDropdown.SetValueWithoutNotify((int)displayMode);
    }

    //This method will be called whenever the player selects a new dropdown option.
    public void OnDisplayModeDropdownValueChange()
    {
        SetDisplayMode((TechDetailsViewDisplayMode)displayModeDropdown.value, false);
    }

    //This method will be called (through an event trigger) whenever the mouse hovers over an option of a dropdown.
    public void OnMouseOverDropdownOption()
    {
        //Plays the appropriate sound effect.
        if(mouseOverDropdownOptionSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverDropdownOptionSFX);
    }

    //This method will be called (through an event trigger) whenever the player clicks on an option of a dropdown.
    public void OnMouseClickDropdownOption()
    {
        //Plays the appropriate sound effect.
        if(clickDropdownOptionSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(clickDropdownOptionSFX);
    }
}
