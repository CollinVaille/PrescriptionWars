using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultButtonManager : MonoBehaviour
{
    public Sprite unselectedButtonTexture;
    public Sprite selectedButtonTexture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonEnter(Transform button)
    {
        button.GetComponent<Image>().sprite = selectedButtonTexture;
    }

    public void OnButtonExit(Transform button)
    {
        button.GetComponent<Image>().sprite = unselectedButtonTexture;
    }
}
