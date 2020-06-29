using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenu : MonoBehaviour
{
    public Image foregroundImage;

    public List<Image> dividers;

    public Text planetNameText;

    public static GameObject planetSelected;

    public List<Shadow> shadows;

    public List<GameObject> tabs;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }

        foregroundImage.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        foreach(Image divider in dividers)
        {
            divider.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        }

        if(planetSelected != null)
        {
            planetNameText.text = planetSelected.name;
        }
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
        foreach(Shadow shadow in shadows)
        {
            shadow.enabled = false;
        }
        transform.gameObject.SetActive(false);
    }

    public void ToggleShadow(Shadow shadow)
    {
        shadow.effectColor = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        shadow.enabled = !shadow.enabled;
    }
}
