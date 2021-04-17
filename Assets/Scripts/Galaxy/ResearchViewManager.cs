using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchViewManager : GalaxyViewBehaviour
{
    [Header("Research View Options")]

    public Material skyboxMaterial;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        GalaxyManager.galaxyManager.ResetPopupClosedOnFrame();
    }
}