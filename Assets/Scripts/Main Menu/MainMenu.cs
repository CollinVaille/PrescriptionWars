using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public List<GameObject> menus;

    public AudioSettingsMenu audioSettingsMenu;
    public VideoSettingsMenu videoSettingsMenu;

    [Header("Scene Components")]

    [SerializeField]
    private Camera mainMenuSceneCamera = null;
    public static Camera SceneCamera
    {
        get
        {
            return mainMenu.mainMenuSceneCamera;
        }
    }

    [Header("Prefabs")]

    [SerializeField]
    private GameObject tooltipPrefab = null;

    //Non-inspector variables.

    private static MainMenu mainMenu = null;

    // Start is called before the first frame update
    void Start()
    {
        audioSettingsMenu.LoadSettings();
        videoSettingsMenu.LoadSettings();

        FlagCreationMenu.Initialize();
    }

    private void Awake()
    {
        mainMenu = this;

        GalaxyTooltip.tooltipPrefab = tooltipPrefab;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonClicked(int numOfButtonClicked)
    {
        if(numOfButtonClicked >= 0 && numOfButtonClicked < menus.Count)
        {
            gameObject.SetActive(false);
            menus[numOfButtonClicked].SetActive(true);
        }
        if(numOfButtonClicked == menus.Count)
        {
            Application.Quit();
        }
    }
}
