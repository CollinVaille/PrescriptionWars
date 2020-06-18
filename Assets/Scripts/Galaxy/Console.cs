using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    public InputField commandInputField;

    public Text commandHistoryText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterCommand()
    {
        commandHistoryText.text += "\n" + commandInputField.text;
        RunThroughCommands(commandInputField.text);
    }

    void RunThroughCommands(string command)
    {

    }

    void ChangeEmpireBasedOnCulture()
    {

    }
}
