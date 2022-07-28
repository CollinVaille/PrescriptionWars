using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyInputFieldConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Input Field Confirmation Popup Components")]

    //Input field that the player will type their response in (should be assigned through the inspector).
    [SerializeField]
    private InputField inputField = null;

    //Non-inspector variables.

    //The prefab that galaxy input field confirmation popups must be instantiated from (value assigned in the start method of the galaxy generator class).
    public static GameObject galaxyInputFieldConfirmationPopupPrefab;

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

    new public void CreateConfirmationPopup(string topText)
    {
        base.CreateConfirmationPopup(topText);
    }

    //Can be called in order to set the max number of characters the input field will allow for the user to type.
    public void SetCharacterLimit(int maxNumberOfCharacters)
    {
        inputField.characterLimit = maxNumberOfCharacters;
    }

    //Can be called in order to set the place holder text of the input field.
    public void SetPlaceHolderText(string placeHolderText)
    {
        inputField.placeholder.GetComponent<Text>().text = placeHolderText;
    }

    //Returns the text that the user has inputted into the input field.
    public string GetInputFieldText()
    {
        return inputField.text;
    }

    //Sets the text of the input field to the passed through value.
    public void SetInputFieldText(string newInputFieldText)
    {
        inputField.text = newInputFieldText;
    }
}
