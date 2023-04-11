using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatConsoleHistoryLog : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The text component that displays the cheat command that the player submitted.")] private Text cheatCommandText = null;
    [SerializeField, Tooltip("The text component that displays the return of the cheat command that the player submitted indicating whether it was successful or not.")] private Text returnText = null;

    //Non-inspector variables.

    /// <summary>
    /// Private static property that returns the prefab game object that all cheat console history logs should be instantiated from.
    /// </summary>
    private static GameObject cheatConsoleHistoryLogPrefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Cheat Console/Cheat Console History Log"); }

    /// <summary>
    /// Public static method that should be called in order to instantiate a new cheat console history log and obtain a reference to its script.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="cheatCommand"></param>
    /// <param name="cheatCommandReturn"></param>
    /// <returns></returns>
    public static CheatConsoleHistoryLog InstantiateLog(Transform parent, string cheatCommand, string cheatCommandReturn)
    {
        //Instantiates a new cheat console history log from the cheat console history log prefab.
        CheatConsoleHistoryLog cheatConsoleHistoryLog = Instantiate(cheatConsoleHistoryLogPrefab).GetComponent<CheatConsoleHistoryLog>();

        //Sets the value of the text on the cheat console history log.
        cheatConsoleHistoryLog.cheatCommandText.text = cheatCommand;
        cheatConsoleHistoryLog.returnText.text = cheatCommandReturn;

        //Sets the parent of the cheat console history log and resets its scale.
        cheatConsoleHistoryLog.transform.SetParent(parent);
        cheatConsoleHistoryLog.transform.localScale = Vector3.one;

        //Returns the cheat console history log just created.
        return cheatConsoleHistoryLog;
    }
}