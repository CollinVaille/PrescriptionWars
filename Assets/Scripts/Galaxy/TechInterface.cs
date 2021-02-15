using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInterface : MonoBehaviour
{
    public GameObject galaxyView;
    public GameObject techTotemsView;
    public TechTotemDetailsView techTotemDetailsView;

    public GameObject techListMenuPrefab;
    public static GameObject techListMenuPrefabGlobal;
    public Transform popupsParent;
    public static Transform popupsParentGlobal;

    public Material skyboxMaterial;

    public List<RawImage> researchProgressRawImages;
    public List<Image> techTotemImages;
    public List<Image> techTotemSelectedOutlineImages;

    public List<Text> techTotemTopTexts;
    public List<Text> techNameTexts;
    public List<Text> techDescriptionTexts;
    public List<Text> techLevelTexts;
    public List<Text> techCostTexts;

    public AudioSource sfxSource;
    public AudioClip bubblingAudioClip;

    public string defaultTechTotemSpriteName;

    // Start is called before the first frame update
    void Start()
    {
        techListMenuPrefabGlobal = techListMenuPrefab;
        popupsParentGlobal = popupsParent;

        UpdateTechTotems();
    }

    // Update is called once per frame
    void Update()
    {
        GalaxyManager.ResetPopupClosedOnFrame();

        if (Input.GetKeyDown(KeyCode.Escape) && techTotemsView.activeInHierarchy)
        {
            if (!TechListMenu.IsATechListMenuOpen())
                SwitchToGalaxy();
        }

        UpdateTechTotems();
    }

    void UpdateTechTotems()
    {
        //Updates each tech totem's images.
        for(int x = 0; x < techTotemImages.Count; x++)
        {
            if(Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techTotemImages[x].sprite = Resources.Load<Sprite>("Galaxy/Tech Totems/" + Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].spriteName);
            }
            else
            {
                techTotemImages[x].sprite = Resources.Load<Sprite>("Galaxy/Tech Totems/" + defaultTechTotemSpriteName);
            }
        }

        //Updates each research progress raw image's position.
        for(int x = 0; x < researchProgressRawImages.Count; x++)
        {
            bool goodTotem = true;

            if(Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected == x)
            {
                if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected].techsAvailable.Count > 0)
                {
                    researchProgressRawImages[x].transform.localPosition = new Vector3(researchProgressRawImages[x].transform.localPosition.x, (Empire.empires[GalaxyManager.playerID].science / Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected].techDisplayed]].cost) * 350 + -400, researchProgressRawImages[x].transform.localPosition.z);
                    if (researchProgressRawImages[x].transform.localPosition.y > -50)
                        researchProgressRawImages[x].transform.localPosition = new Vector3(researchProgressRawImages[x].transform.localPosition.x, -50, researchProgressRawImages[x].transform.localPosition.z);
                }
                else
                    goodTotem = false;
            }
            else
                goodTotem = false;

            if (!goodTotem)
            {
                researchProgressRawImages[x].transform.localPosition = new Vector3(researchProgressRawImages[x].transform.localPosition.x, -400, researchProgressRawImages[x].transform.localPosition.z);
            }
        }

        //Updates each tech totem's top/title text.
        for(int x = 0; x < techTotemTopTexts.Count; x++)
        {
            techTotemTopTexts[x].text = Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].name;
        }

        //Updates each tech totem's tech name text.
        for(int x = 0; x < techNameTexts.Count; x++)
        {
            if(Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techNameTexts[x].text = Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].name;
            }
            else
            {
                techNameTexts[x].text = "No Valid Tech";
            }
        }

        //Updates each tech totem's tech description text.
        for(int x = 0; x < techDescriptionTexts.Count; x++)
        {
            if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techDescriptionTexts[x].text = Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].description;
            }
            else
            {
                techDescriptionTexts[x].text = "This tech totem has been fully completed.";
            }
        }

        //Updates each tech totem's tech level text.
        for(int x = 0; x < techLevelTexts.Count; x++)
        {
            if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techLevelTexts[x].text = "Level: " + Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].level;
            }
            else
            {
                techLevelTexts[x].text = "Level: None";
            }
        }


        //Updates each tech totem's tech cost text.
        for(int x = 0; x < techCostTexts.Count; x++)
        {
            if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techCostTexts[x].text = "Cost: " + Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].cost;
            }
            else
            {
                techCostTexts[x].text = "Cost: None";
            }
        }
    }

    //Switches the game from the research view to the galaxy view (exiting this view back to the main view).
    public void SwitchToGalaxy()
    {
        //Detects if a tech list menu is open, if so then it closes all tech list menus.
        if (TechListMenu.IsATechListMenuOpen())
            TechListMenu.CloseAllTechListMenus();

        //Activates the galaxy view's game object.
        galaxyView.SetActive(true);
        //Changes the skybox of the game back to the galaxy view's skybox.
        RenderSettings.skybox = galaxyView.GetComponent<GalaxyGenerator>().skyboxMaterial;
        //Deactivates the research view's game object.
        transform.gameObject.SetActive(false);
    }

    public void ClickOnTotem(int num)
    {
        SetTechTotemSelected(num);
    }

    public void ClickOnTotemTechListButton(int num)
    {
        //TechListMenu.CreateNewTechListMenu(num);
        techTotemsView.SetActive(false);
        techTotemDetailsView.gameObject.SetActive(true);
        techTotemDetailsView.OnSwitchToTechTotemDetailsView(num);
    }

    public void SetTechTotemSelected(int newTechTotemSelected)
    {
        int previousTechTotemSelected = Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected;
        Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected = newTechTotemSelected;

        if(newTechTotemSelected != previousTechTotemSelected)
        {
            if(previousTechTotemSelected != -1)
                techTotemSelectedOutlineImages[previousTechTotemSelected].gameObject.SetActive(false);
            if(newTechTotemSelected != -1)
                techTotemSelectedOutlineImages[newTechTotemSelected].gameObject.SetActive(true);
        }
    }
}