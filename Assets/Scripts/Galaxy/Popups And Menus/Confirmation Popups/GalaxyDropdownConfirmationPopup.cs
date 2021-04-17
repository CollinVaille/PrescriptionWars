using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyDropdownConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Dropdown Confirmation Popup Audio Options")]

    //The sound effect that plays whenever the mouse enters a dropdown option.
    [SerializeField]
    private AudioClip mouseOverDropdownOptionSFX = null;
    //The sound effect that plays whenever a click is performed on a dropdown option.
    [SerializeField]
    private AudioClip clickDropdownOptionSFX = null;

    [Header("Dropdown Confirmation Popup Components")]

    //Dropdown that the player will use to give their response (should be assigned through the inspector).
    [SerializeField]
    private Dropdown dropdown = null;

    //Non-inspector variables.

    //The string that indicates what option the user has selected when pressing the confirm button.
    private string returnValue;

    //The prefab that galaxy dropdown confirmation popups must be instantiated from (value assigned in the start method of the galaxy generator class).
    public static GameObject galaxyDropdownConfirmationPopupPrefab;

    // Start is called before the first frame update
    public override void Start()
    {
        //Executes the start logic of the base class.
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        //Executes the update logic of the base class.
        base.Update();
    }

    //Adds a new dropdown option for the player to selected that has the passed through string displayed on it.
    public void AddDropdownOption(string optionText)
    {
        //Creates a new dropdown option data object (default Unity class).
        Dropdown.OptionData newDropdownOptionData = new Dropdown.OptionData();
        //Sets the text of the new dropdown option to the passed through string value.
        newDropdownOptionData.text = optionText;
        //Adds the new dropdown data object to the actual dropdown.
        dropdown.options.Add(newDropdownOptionData);
    }

    //Sets the option selected in the dropdown to the option that has the same text as the string passed through (if no option has the same text then it will do nothing).
    public void SetDropdownOptionSelected(string optionText)
    {
        for(int x = 0; x < dropdown.options.Count; x++)
        {
            if (dropdown.options[x].text == optionText)
                dropdown.value = x;
        }
    }

    public override void Confirm()
    {
        //Sets the string that indicates what option the user has selected when pressing the confirm button.
        if (dropdown.options.Count > 0)
            returnValue = dropdown.options[dropdown.value].text;
        else
            returnValue = "No valid dropdown option selected.";

        //Executes the confirm logic of the base class.
        base.Confirm();
    }

    //Returns the string that indicates what option the user has selected when pressing the confirm button.
    public string GetReturnValue()
    {
        return returnValue;
    }

    //This method is called whenever the mouse goes over a dropdown option (called through an event trigger) and plays the mouse over dropdown option sound effect.
    public void MouseOverDropdownOption()
    {
        AudioManager.PlaySFX(mouseOverDropdownOptionSFX);
    }

    //This method is called whenever a dropdown option is clicked (called through an event trigger) and plays the appropriate sound effect.
    public void ClickDropdownOption()
    {
        AudioManager.PlaySFX(clickDropdownOptionSFX);
    }
}
