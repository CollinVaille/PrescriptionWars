using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyDropdownConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Dropdown Confirmation Popup Audio Options")]

    [SerializeField, Tooltip("The sound effect that plays whenever the mouse enters a dropdown option.")] private AudioClip mouseOverDropdownOptionSFX = null;
    [SerializeField, Tooltip("The sound effect that plays whenever a click is performed on a dropdown option.")] private AudioClip clickDropdownOptionSFX = null;

    [Header("Dropdown Confirmation Popup Components")]

    [SerializeField, Tooltip("Dropdown that the player will use to give their response (should be assigned through the inspector).")] private Dropdown dropdown = null;

    //Non-inspector variables.

    /// <summary>
    /// Publicly accessible property that should be accessed in order to determine the option that the user selected before pressing the confirm buttom.
    /// </summary>
    public string returnValue { get; protected set; } = null;

    /// <summary>
    /// Public static property that should be accessed in order to obtain the dropdown confirmation popup prefab that all dropdown confirmation popups should be instantiated from.
    /// </summary>
    public static GameObject dropdownConfirmationPopupPrefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Confirmation Popups/Dropdown Confirmation Popup"); }

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

    public override void CreateConfirmationPopup(string topText)
    {
        base.CreateConfirmationPopup(topText);
    }

    /// <summary>
    /// Adds a new dropdown option for the player to select that has the specified string displayed on it.
    /// </summary>
    /// <param name="optionText"></param>
    public void AddDropdownOption(string optionText)
    {
        //Creates a new dropdown option data object (default Unity class).
        Dropdown.OptionData newDropdownOptionData = new Dropdown.OptionData();
        //Sets the text of the new dropdown option to the passed through string value.
        newDropdownOptionData.text = optionText;
        //Adds the new dropdown data object to the actual dropdown.
        dropdown.options.Add(newDropdownOptionData);
    }

    /// <summary>
    /// Sets the option selected in the dropdown to the option that has the same text as the specified string (if no option has the same text then it will do nothing).
    /// </summary>
    /// <param name="optionText"></param>
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

    /// <summary>
    /// This method is called through an event trigger in the inspector whenever the mouse goes over a dropdown option and plays the mouse over dropdown option sound effect.
    /// </summary>
    public void MouseOverDropdownOption()
    {
        AudioManager.PlaySFX(mouseOverDropdownOptionSFX);
    }

    /// <summary>
    /// This method is called through an event trigger in the inspector whenever a dropdown option is clicked and plays the appropriate sound effect.
    /// </summary>
    public void ClickDropdownOption()
    {
        AudioManager.PlaySFX(clickDropdownOptionSFX);
    }
}
