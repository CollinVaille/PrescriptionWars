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
            if(mainMenu != null)
                return mainMenu.mainMenuSceneCamera;
            return null;
        }
        set
        {
            if (mainMenu != null)
                mainMenu.mainMenuSceneCamera = value;
        }
    }

    [Header("Prefabs")]

    [SerializeField]
    private GameObject tooltipPrefab = null;
    [SerializeField]
    private GameObject backArrowPrefab = null;

    //Non-inspector variables.

    private static MainMenu mainMenu = null;

    // Start is called before the first frame update
    void Start()
    {
        audioSettingsMenu.LoadSettings();
        videoSettingsMenu.LoadSettings();
    }

    public void Awake()
    {
        mainMenu = this;

        GalaxyTooltip.tooltipPrefab = tooltipPrefab;
        GalaxyMenuBehaviour.backArrowPrefab = backArrowPrefab;

        if (mainMenuSceneCamera == null)
            mainMenuSceneCamera = Camera.main;
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
