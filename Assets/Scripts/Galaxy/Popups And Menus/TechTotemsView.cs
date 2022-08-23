using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechTotemsView : GalaxyMenuBehaviour
{
    [Header("Tech Totems View Raw Image Components")]

    [SerializeField]
    private List<RawImage> researchProgressRawImages = null;

    [Header("Tech Totems View Image Components")]

    [SerializeField]
    private List<Image> techTotemImages = null;
    [SerializeField]
    private List<Image> techTotemSelectedOutlineImages = null;

    [Header("Tech Totems View Text Components")]

    [SerializeField]
    private List<Text> techTotemTopTexts = null;
    [SerializeField]
    private List<Text> techNameTexts = null;
    [SerializeField]
    private List<Text> techDescriptionTexts = null;
    [SerializeField]
    private List<Text> techLevelTexts = null;
    [SerializeField]
    private List<Text> techCostTexts = null;

    [Header("Tech Totems View Options")]

    [SerializeField]
    private TechTotemDetailsView techTotemDetailsView = null;

    [SerializeField]
    private string defaultTechTotemSpriteName = null;

    // Start is called before the first frame update
    public override void Start()
    {
        //Executes the logic of the base class for the start method.
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        //Executes the logic of the base class for the update method.
        base.Update();
    }

    private void OnEnable()
    {
        UpdateTechTotems();

        SetTechTotemSelected(Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected);
    }

    public override void SwitchToPreviousMenu()
    {
        //Executes the logic of the base class for switching to the previous menu.
        base.SwitchToPreviousMenu();

        //Changes the skybox of the game back to the galaxy view's skybox.
        RenderSettings.skybox = GetPreviousMenu().GetComponent<GalaxyGenerator>().skyboxMaterial;
    }

    public void ClickOnTotem(int num)
    {
        SetTechTotemSelected(num);

        GalaxyManager.galaxyManager.WarningRightSideNotificationsUpdate();
    }

    public void ClickOnTotemTechListButton(int num)
    {
        gameObject.SetActive(false);

        techTotemDetailsView.gameObject.SetActive(true);

        techTotemDetailsView.OnSwitchToTechTotemDetailsView(num);
    }

    public void SetTechTotemSelected(int newTechTotemSelected)
    {
        if(newTechTotemSelected >= 0 && newTechTotemSelected < Empire.empires[GalaxyManager.playerID].techManager.techTotems.Count && Empire.empires[GalaxyManager.playerID].techManager.techTotems[newTechTotemSelected].techsAvailable.Count != 0)
            Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected = newTechTotemSelected;

        for (int x = 0; x < techTotemSelectedOutlineImages.Count; x++)
        {
            if (x != newTechTotemSelected)
                techTotemSelectedOutlineImages[x].gameObject.SetActive(false);
            else if (newTechTotemSelected >= 0 && newTechTotemSelected < Empire.empires[GalaxyManager.playerID].techManager.techTotems.Count && Empire.empires[GalaxyManager.playerID].techManager.techTotems[newTechTotemSelected].techsAvailable.Count != 0)
                techTotemSelectedOutlineImages[x].gameObject.SetActive(true);
            else
                techTotemSelectedOutlineImages[x].gameObject.SetActive(false);
        }

        for (int x = 0; x < researchProgressRawImages.Count; x++)
        {
            if (x != newTechTotemSelected)
                researchProgressRawImages[x].transform.localPosition = new Vector2(researchProgressRawImages[x].transform.localPosition.x, -400);
            else if (newTechTotemSelected >= 0 && newTechTotemSelected < Empire.empires[GalaxyManager.playerID].techManager.techTotems.Count && Empire.empires[GalaxyManager.playerID].techManager.techTotems[newTechTotemSelected].techsAvailable.Count != 0)
                researchProgressRawImages[x].transform.localPosition = new Vector2(researchProgressRawImages[x].transform.localPosition.x, Empire.empires[GalaxyManager.playerID].science / Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected].techDisplayed]].cost * 350 + -400);
            else
                researchProgressRawImages[x].transform.localPosition = new Vector2(researchProgressRawImages[x].transform.localPosition.x, -400);
        }
    }

    private void UpdateTechTotems()
    {
        //Updates each tech totem's images.
        for (int x = 0; x < techTotemImages.Count; x++)
        {
            if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techTotemImages[x].sprite = Resources.Load<Sprite>("Galaxy/Tech Totems/" + Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].spriteName);
            }
            else
            {
                techTotemImages[x].sprite = Resources.Load<Sprite>("Galaxy/Tech Totems/" + defaultTechTotemSpriteName);
            }
        }

        //Updates each research progress raw image's position.
        for (int x = 0; x < researchProgressRawImages.Count; x++)
        {
            bool goodTotem = true;

            if (Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected == x)
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
        for (int x = 0; x < techTotemTopTexts.Count; x++)
        {
            techTotemTopTexts[x].text = Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].name;
        }

        //Updates each tech totem's tech name text.
        for (int x = 0; x < techNameTexts.Count; x++)
        {
            if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable.Count > 0)
            {
                techNameTexts[x].text = Tech.entireTechList[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techsAvailable[Empire.empires[GalaxyManager.playerID].techManager.techTotems[x].techDisplayed]].name;
            }
            else
            {
                techNameTexts[x].text = "No Valid Tech";
            }
        }

        //Updates each tech totem's tech description text.
        for (int x = 0; x < techDescriptionTexts.Count; x++)
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
        for (int x = 0; x < techLevelTexts.Count; x++)
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
        for (int x = 0; x < techCostTexts.Count; x++)
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
}
