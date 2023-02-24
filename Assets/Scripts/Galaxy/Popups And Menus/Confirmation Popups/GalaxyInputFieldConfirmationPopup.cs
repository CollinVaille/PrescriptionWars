using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyInputFieldConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Input Field Confirmation Popup Components")]

    //Input field that the player will type their response in (should be assigned through the inspector).
    [SerializeField] private InputField inputField = null;

    //Non-inspector variables.

    /// <summary>
    /// Public property that should be used both in order to access and mutate the text currently in the input field.
    /// </summary>
    public string inputFieldText { get => inputField.text; set => inputField.text = value; }

    /// <summary>
    /// Public property that should be used both in order to access and mutate whether special characters are allowed in the input field text upon the player pressing the confirm button.
    /// </summary>
    public bool specialCharactersAllowed { get; set; } = true;

    /// <summary>
    /// Public static property that should be accessed in order to obtain the prefab that all input field confirmation popups should be instantiated from.
    /// </summary>
    public static GameObject inputFieldConfirmationPopupPrefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Confirmation Popups/Input Field Confirmation Popup"); }

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
    /// Public method that can be called in order to set the maximum number of characters the input field will allow for the user to input.
    /// </summary>
    /// <param name="maxNumberOfCharacters"></param>
    public void SetCharacterLimit(int maxNumberOfCharacters)
    {
        inputField.characterLimit = maxNumberOfCharacters;
    }

    /// <summary>
    /// Public method that should be called in order to set the place holder text of the input field, which is the grayed out text that shows up in the input field before the player types anything.
    /// </summary>
    /// <param name="placeHolderText"></param>
    public void SetPlaceHolderText(string placeHolderText)
    {
        inputField.placeholder.GetComponent<Text>().text = placeHolderText;
    }

    public override void Confirm()
    {
        //Returns and does not confirm anything if the input field's text is either null or only contains white spaces.
        if (string.IsNullOrWhiteSpace(inputFieldText) || (!specialCharactersAllowed && new string(inputFieldText.Where(c => Char.IsLetterOrDigit(c) || c == '-' || c == '_' || Char.IsWhiteSpace(c)).ToArray()) != inputFieldText))
            return;
        base.Confirm();
    }
}
