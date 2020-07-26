using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechInterface : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickOnTotem(int num)
    {
        Empire.empires[GalaxyManager.playerID].techManager.techTotemSelected = num;
    }
}
