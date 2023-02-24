using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadGameMenu : GalaxyMenuBehaviour
{
    [Header("Components")]

    [SerializeField] private Transform scrollListParent = null;
    [SerializeField] private Text noSaveGameDataText = null;

    [Header("Prefabs")]

    [SerializeField] private GameObject loadGameMenuScrollListButtonPrefab = null;

    //Non-inspector variables.

    /// <summary>
    /// Holds the save game data that is passed over to the galaxy view. Only populated once the player presses the load game button.
    /// </summary>
    public static GalaxyData saveGameData = null;

    /// <summary>
    /// Private static variable used to hold the instance of the load game menu if it exists currently.
    /// </summary>
    private static LoadGameMenu loadGameMenu = null;

    public override void Awake()
    {
        base.Awake();

        //Sets the static instance.
        loadGameMenu = this;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //Populates the scroll list of the load game menu with the appropriate buttons.
        PopulateScrollList();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Private method that should be called in the start method in order to populate the scroll list of the load game menu with the appropriate buttons.
    /// </summary>
    private void PopulateScrollList()
    {
        //Gets the file path of every save game file in the application's persistent data path.
        string[] saveNames = Directory.GetFiles(Application.persistentDataPath + "/", "*." + GalaxySaveSystem.saveFileExtension);
        //Substring method is used to extract the name of the save game file from the file path.
        for (int saveNameIndex = 0; saveNameIndex < saveNames.Length; saveNameIndex++)
            saveNames[saveNameIndex] = saveNames[saveNameIndex].Substring((Application.persistentDataPath + "/").Length, saveNames[saveNameIndex].Length - ((Application.persistentDataPath + "/").Length + ("." + GalaxySaveSystem.saveFileExtension).Length));

        //Loops through each save name and instantiates a new loadGameScrollListButton to represent it in the load game menu scroll list.
        foreach (string saveName in saveNames)
        {
            LoadGameScrollListButton loadGameScrollListButton = Instantiate(loadGameMenuScrollListButtonPrefab).GetComponent<LoadGameScrollListButton>();
            loadGameScrollListButton.transform.localPosition = Vector3.zero;
            loadGameScrollListButton.transform.SetParent(scrollListParent);
            loadGameScrollListButton.transform.localScale = Vector3.one;
            loadGameScrollListButton.Initialize(saveName);
        }

        //Activates the text in the center of the scroll that says "No Save Game Data" if there are no save game data buttons in the scroll list.
        noSaveGameDataText.gameObject.SetActive(scrollListParent.childCount == 0);
    }

    /// <summary>
    /// This public method should be called through an event trigger whenever the play button is clicked and starts up the galaxy scene.
    /// </summary>
    public void OnClickPlayButton()
    {
        //Returns and does nothing if there is no save game currently selected.
        if (saveGameData == null)
            return;

        //Loads the galaxy scene.
        SceneManager.LoadScene("New Galaxy");
    }

    public override void SwitchToPreviousMenu()
    {
        base.SwitchToPreviousMenu();

        DeselectSaveGameData();
    }

    /// <summary>
    /// This private method is called whenever the load game menu instance is destroyed and resets the static instance.
    /// </summary>
    private void OnDestroy()
    {
        //Resets the static instance.
        loadGameMenu = null;
    }

    /// <summary>
    /// Public static method that should be used in order to deselect the save game data properly and ensure that the scroll list button representing the save game data knows that it has been deselected.
    /// </summary>
    public static void DeselectSaveGameData()
    {
        if (loadGameMenu != null && saveGameData != null)
        {
            for(int buttonIndex = 0; buttonIndex < loadGameMenu.scrollListParent.childCount; buttonIndex++)
            {
                LoadGameScrollListButton button = loadGameMenu.scrollListParent.GetChild(buttonIndex).GetComponent<LoadGameScrollListButton>();
                if (button.saveGameData == saveGameData)
                {
                    button.OnSaveGameDeselected();
                    break;
                }
            }
        }
        saveGameData = null;
    }
}
