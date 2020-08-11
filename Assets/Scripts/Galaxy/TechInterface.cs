using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInterface : MonoBehaviour
{
    public GameObject galaxyView;
    public GameObject techListMenu;

    public Material skyboxMaterial;

    public Scrollbar techListMenuScrollbar;

    public List<Sprite> techSprites;

    public List<RawImage> researchProgressRawImages;
    public List<Image> techTotemImages;
    public List<int> techTotemImageIndexes;

    public List<Text> techTotemTopTexts;
    public List<Text> techNameTexts;
    public List<Text> techDescriptionTexts;
    public List<Text> techLevelTexts;
    public List<Text> techCostTexts;

    public Text techListMenuTopText;
    public Text techNamesListText;
    public Text techLevelsListText;
    public Text techCostsListText;

    public AudioSource sfxSource;
    public AudioClip bubblingAudioClip;
    public AudioClip openTechListMenuAudioClip;

    int techTotemTechListSelected = -1;

    bool techListMenuPreviouslyActive = false;
    bool techListMenuMoving = false;
    Vector2 mouseToMenuDistance;

    // Start is called before the first frame update
    void Start()
    {
        UpdateTechTotems();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
                if(techTotemImageIndexes[x] != Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].spriteNum)
                {
                    techTotemImages[x].sprite = techSprites[Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].spriteNum];
                    techTotemImageIndexes[x] = Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].spriteNum;
                }
            }
            else
            {
                techTotemImages[x].sprite = techSprites[0];
                techTotemImageIndexes[x] = -1;
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

        //Updates the tech list menu's position if the player is currently dragging it.
        if (techListMenuMoving)
        {
            techListMenu.transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if(techListMenu.transform.localPosition.x < -275)
            {
                techListMenu.transform.localPosition = new Vector2(-275, techListMenu.transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - techListMenu.transform.position.x;

                if (mouseToMenuDistance.x < -245)
                    mouseToMenuDistance.x = -245;
            }
            //Right barrier.
            if(techListMenu.transform.localPosition.x > 275)
            {
                techListMenu.transform.localPosition = new Vector2(275, techListMenu.transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - techListMenu.transform.position.x;

                if (mouseToMenuDistance.x > 245)
                    mouseToMenuDistance.x = 245;
            }
            //Top barrier.
            if(techListMenu.transform.localPosition.y > 150)
            {
                techListMenu.transform.localPosition = new Vector2(techListMenu.transform.localPosition.x, 150);

                mouseToMenuDistance.y = Input.mousePosition.y - techListMenu.transform.position.y;

                if (mouseToMenuDistance.y > 150)
                    mouseToMenuDistance.y = 150;
            }
            //Bottom barrier.
            if (techListMenu.transform.localPosition.y < -150)
            {
                techListMenu.transform.localPosition = new Vector2(techListMenu.transform.localPosition.x, -150);

                mouseToMenuDistance.y = Input.mousePosition.y - techListMenu.transform.position.y;

                if (mouseToMenuDistance.y < -150)
                    mouseToMenuDistance.y = -150;
            }
        }

        //Updates the tech list menu's ui if it is up.
        if (techListMenu.activeInHierarchy && !techListMenuPreviouslyActive && techTotemTechListSelected >= 0 && techTotemTechListSelected < Empire.empires[GalaxyManager.playerID].techManager.techTotems.Count)
        {
            UpdateTechListMenu();
        }

        techListMenuPreviouslyActive = techListMenu.activeInHierarchy;
    }

    public void UpdateTechListMenu()
    {
        techNamesListText.text = "";
        techLevelsListText.text = "";
        techCostsListText.text = "";

        techListMenuTopText.text = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemTechListSelected].name;
        if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemTechListSelected].techsAvailable.Count <= 10)
        {
            List<int> techListInOrder = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemTechListSelected].GetTechsInOrderList();

            for (int x = 0; x < techListInOrder.Count; x++)
            {
                if (x > 0)
                {
                    techNamesListText.text += "\n";
                    techLevelsListText.text += "\n";
                    techCostsListText.text += "\n";
                }
                techNamesListText.text += Tech.entireTechList[techListInOrder[x]].name;
                techLevelsListText.text += Tech.entireTechList[techListInOrder[x]].level;
                techCostsListText.text += Tech.entireTechList[techListInOrder[x]].cost;
            }
        }
        else
        {
            List<float> possibleValues = GalaxyHelperMethods.GetScrollbarValueNumbers(Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemTechListSelected].techsAvailable.Count - 9);
            int closestIndex = 0;
            for (int x = 1; x < possibleValues.Count; x++)
            {
                if (Mathf.Abs(possibleValues[x] - techListMenuScrollbar.value) <= Mathf.Abs(possibleValues[closestIndex] - techListMenuScrollbar.value))
                    closestIndex = x;
            }

            List<int> techListInOrder = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemTechListSelected].GetTechsInOrderList();

            bool firstLoop = true;
            for (int x = closestIndex; x < closestIndex + 10; x++)
            {
                if (!firstLoop)
                {
                    techNamesListText.text += "\n";
                    techLevelsListText.text += "\n";
                    techCostsListText.text += "\n";
                }
                techNamesListText.text += Tech.entireTechList[techListInOrder[x]].name;
                techLevelsListText.text += Tech.entireTechList[techListInOrder[x]].level;
                techCostsListText.text += Tech.entireTechList[techListInOrder[x]].cost;

                firstLoop = false;
            }
        }
    }

    public void SwitchToGalaxy()
    {
        if (techListMenu.activeInHierarchy)
            CloseTechListMenu();

        galaxyView.SetActive(true);
        RenderSettings.skybox = galaxyView.GetComponent<GalaxyGenerator>().skyboxMaterial;
        transform.gameObject.SetActive(false);
    }

    public void ClickOnTotem(int num)
    {
        Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected = num;
    }

    public void ClickOnTotemTechListButton(int num)
    {
        //Sets which tech totem the menu is displaying info for.
        techTotemTechListSelected = num;
        //Activates the tech list menu object.
        techListMenu.SetActive(true);

        //Plays the open/close sound effect.
        sfxSource.PlayOneShot(openTechListMenuAudioClip);
    }

    public void CloseTechListMenu()
    {
        //Resets the scrollbar's value.
        techListMenuScrollbar.value = 0;
        //Resets whether the tech list menu is being dragged by the player.
        PointerUpTechListMenu();
        //Resets the tech list menu's location.
        techListMenu.transform.localPosition = Vector2.zero;
        //Deactivates the tech list menu object.
        techListMenu.SetActive(false);

        //Plays the open/close sound effect.
        sfxSource.PlayOneShot(openTechListMenuAudioClip);
    }

    public void PointerDownTechListMenu()
    {
        //Tells the update function that the player is dragging the menu.
        techListMenuMoving = true;

        //Tells the update function the set difference between the mouse position and the menu's position.
        mouseToMenuDistance.x = Input.mousePosition.x - techListMenu.transform.position.x;
        mouseToMenuDistance.y = Input.mousePosition.y - techListMenu.transform.position.y;
    }

    public void PointerUpTechListMenu()
    {
        //Tells the update function that the player is no longer dragging the menu.
        techListMenuMoving = false;

        //Resets the vector that says the difference between the mouse position and the menu's position.
        mouseToMenuDistance = Vector2.zero;
    }
}
