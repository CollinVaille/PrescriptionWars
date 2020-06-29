using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenu : MonoBehaviour
{
    public Image foregroundImage;

    public List<Image> dividers;

    public Text planetNameText;
    public Text buildingsListText;

    public static GameObject planetSelected;

    public List<Shadow> shadows;

    public List<GameObject> tabs;

    public Scrollbar buildingsCompletedScrollbar;
    public Scrollbar buildingQueueScrollbar;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Closes the whole planet management menu if the user presses escape.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }

        //Sets the color of the foreground and all of the dividers based on the player empire's label color.
        foregroundImage.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        foreach(Image divider in dividers)
        {
            divider.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        }

        buildingsCompletedScrollbar.image.color = Empire.empires[GalaxyManager.playerID].empireColor;
        buildingQueueScrollbar.image.color = Empire.empires[GalaxyManager.playerID].empireColor;

        if(planetSelected != null)
        {
            planetNameText.text = planetSelected.name;

            if (tabs[0].activeInHierarchy)
            {
                SetBuildingsListText();
            }
        }
    }

    public void SetBuildingsListText()
    {
        buildingsListText.text = "";

        if(planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText().Count <= 4)
        {
            for(int x = 0; x < planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText().Count; x++)
            {
                if(x == 0)
                {
                    buildingsListText.text = planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText()[x];
                }
                else
                {
                    buildingsListText.text += "\n" + planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText()[x];
                }
            }
        }
        else
        {
            int possibleValues = planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText().Count - 3;

            int closestIndex = 0;

            for(int x = 0; x < GetValueNumbers(possibleValues).Count; x++)
            {
                if(x == 0)
                {
                    closestIndex = 0;
                }
                else
                {
                    if(Mathf.Abs(GetValueNumbers(possibleValues)[x] - buildingsCompletedScrollbar.value) < Mathf.Abs(GetValueNumbers(possibleValues)[closestIndex] - buildingsCompletedScrollbar.value))
                    {
                        closestIndex = x;
                    }
                }
            }

            for(int x = closestIndex; x < closestIndex + 4; x++)
            {
                if(x == closestIndex)
                {
                    buildingsListText.text = planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText()[x];
                }
                else
                {
                    buildingsListText.text += "\n" + planetSelected.GetComponent<PlanetIcon>().GetBuildingsListText()[x];
                }
            }
        }
    }

    public List<float> GetValueNumbers(int num)
    {
        List<float> valueNumbers = new List<float>();

        for(int x = 0; x < num; x++)
        {
            valueNumbers.Add(1.0f / (num - 1) * x);
        }

        return valueNumbers;
    }

    public void ClickOnTab(int num)
    {
        for(int x = 0; x < tabs.Count; x++)
        {
            if (x == num)
                tabs[x].SetActive(true);
            else
                tabs[x].SetActive(false);
        }
    }

    public void CloseMenu()
    {
        //Resets all of the shadows on text.
        foreach(Shadow shadow in shadows)
        {
            shadow.enabled = false;
        }

        //Sets the selected tab back to the first tab (currently the buildings tab).
        for(int x = 0; x < tabs.Count; x++)
        {
            if (x == 0)
                tabs[x].SetActive(true);
            else
                tabs[x].SetActive(false);
        }

        //Deactivates the whole planet management menu.
        transform.gameObject.SetActive(false);
    }

    public void ToggleShadow(Shadow shadow)
    {
        shadow.effectColor = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        shadow.enabled = !shadow.enabled;
    }
}
