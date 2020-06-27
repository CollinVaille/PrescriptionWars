using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenu : MonoBehaviour
{
    public Image foregroundImage;

    public List<Image> dividers;

    public static GameObject planetSelected;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            transform.gameObject.SetActive(false);
        }

        foregroundImage.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        foreach(Image divider in dividers)
        {
            divider.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        }
    }
}
