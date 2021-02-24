using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchViewManager : MonoBehaviour
{
    [Header("Options")]

    public Material skyboxMaterial;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GalaxyManager.ResetPopupClosedOnFrame();
    }
}