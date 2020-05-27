using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public List<GameObject> menus;

    public Sprite unselectedButtonTexture;

    // Start is called before the first frame update
    void Start()
    {
        
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
    }

    public void ResetButtonTexture(Transform button)
    {
        button.GetComponent<Image>().sprite = unselectedButtonTexture;
    }
}
