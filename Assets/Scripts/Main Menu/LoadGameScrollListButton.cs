using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadGameScrollListButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Components")]

    [SerializeField] private Text saveNameText = null;
    [SerializeField] private Text galaxyShapeText = null;
    [SerializeField] private Image galaxyShapeImage = null;

    /// <summary>
    /// Private holder variable that indicates the name of the save game that the button represents.
    /// </summary>
    private string saveGameName = null;

    /// <summary>
    /// Private holder variable that holds the save game data that the button is representing.
    /// </summary>
    private GalaxyData saveGameDataVar = null;
    /// <summary>
    /// Public property that should be used to access the save game data of the button.
    /// </summary>
    public GalaxyData saveGameData { get => saveGameDataVar; }

    /// <summary>
    /// Private holder variable that contains the original normal color of the button.
    /// </summary>
    private Color normalColor;

    // Start is called before the first frame update
    void Start()
    {
        //Sets the variable that indicates the initial normal color of the button.
        normalColor = GetComponent<Button>().colors.normalColor;
    }

    // Update is called once per frame
    void Update()
    {
        //Loads the save game data from the save game name if the button is visible to the player.
        if (saveGameDataVar == null && transform.localPosition.y >= -500)
            LoadSaveGameData();
    }

    /// <summary>
    /// This public method should be called by the load game menu right after instantiating the load game scroll list button from its prefab and initializes all needed variables.
    /// </summary>
    public void Initialize(string saveGameName)
    {
        //Initializes the save game name variable.
        this.saveGameName = saveGameName;
        //Updates the save name text component to accurately reflect the name of the save game that the button is representing.
        saveNameText.text = "Save Name: " + saveGameName;
    }

    /// <summary>
    /// Private method that should be called to load the save game data from its file and make the button's UI components represent the save game data.
    /// </summary>
    private void LoadSaveGameData()
    {
        //Loads in the save game data from the galaxy save system using the previously stored save game name.
        saveGameDataVar = GalaxySaveSystem.LoadGalaxy(saveGameName);

        //Logs a warning and returns if there is no valid save game data.
        if(saveGameDataVar == null)
        {
            Debug.LogWarning("Cannot load save game data of save: " + saveGameName + " to a load game menu scroll list button.");
            return;
        }

        //Updates the galaxy shape text component to accurately reflect the name of the shape that the galaxy was previously generated to fit.
        galaxyShapeText.text = "Galaxy Shape: " + saveGameDataVar.galaxyShape;
        //Updates the galaxy shape image component to accurately reflect the image of the shape that the galaxy was previously generated to fit.
        galaxyShapeImage.sprite = Resources.Load<Sprite>("Galaxy/Galaxy Shapes/" + saveGameDataVar.galaxyShape);
    }

    /// <summary>
    /// This public method should be called by an event trigger whenever the main button is clicked and sets the load game menu's selected save to the save represented by this load game scroll list button.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (LoadGameMenu.saveGameData != saveGameDataVar)
        {
            if (LoadGameMenu.saveGameData != null)
                LoadGameMenu.DeselectSaveGameData();

            LoadGameMenu.saveGameData = saveGameDataVar;

            ColorBlock buttonColors = GetComponent<Button>().colors;
            buttonColors.normalColor = buttonColors.selectedColor;
            GetComponent<Button>().colors = buttonColors;
        }
    }

    /// <summary>
    /// This public method should be called by the load game menu whenever the same game attached to this button is deselected.
    /// </summary>
    public void OnSaveGameDeselected()
    {
        ColorBlock buttonColors = GetComponent<Button>().colors;
        buttonColors.normalColor = normalColor;
        GetComponent<Button>().colors = buttonColors;
    }
}
