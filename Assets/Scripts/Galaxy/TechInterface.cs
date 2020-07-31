using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechInterface : MonoBehaviour
{
    public GameObject galaxyView;

    public List<Sprite> techSprites;

    public List<RawImage> researchProgressRawImages;
    public List<Image> techTotemImages;
    public List<int> techTotemImageIndexes;

    public List<Text> techTotemTopTexts;
    public List<Text> techNameTexts;
    public List<Text> techLevelTexts;
    public List<Text> techDescriptionTexts;

    public AudioSource sfxSource;
    public AudioClip bubblingAudioClip;

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
    }

    public void SwitchToGalaxy()
    {
        galaxyView.SetActive(true);
        transform.gameObject.SetActive(false);
    }

    public void ClickOnTotem(int num)
    {
        Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected = num;
    }
}
