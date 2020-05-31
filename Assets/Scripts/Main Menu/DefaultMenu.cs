using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultMenu : MonoBehaviour
{
    public GameObject previousMenu;

    public List<Button> buttons;

    public Sprite unselectedButtonTexture;

    public List<GameObject> menus;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
            previousMenu.SetActive(true);
            for(int x = 0; x < buttons.Count; x++)
            {
                buttons[x].GetComponent<Image>().sprite = unselectedButtonTexture;
            }
        }
    }

    public void ButtonClicked(int numOfButtonClicked)
    {
        if (numOfButtonClicked >= 0 && numOfButtonClicked < menus.Count)
        {
            gameObject.SetActive(false);
            menus[numOfButtonClicked].SetActive(true);
        }
        if (numOfButtonClicked == menus.Count)
        {
            Application.Quit();
        }
    }
}
