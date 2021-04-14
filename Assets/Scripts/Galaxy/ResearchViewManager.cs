using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchViewManager : MonoBehaviour, IGalaxyPopupBehaviourHandler
{
    [Header("Galaxy Popup Behaviour Handler")]

    [SerializeField] private Vector2 popupScreenBounds = Vector2.zero;
    public Vector2 PopupScreenBounds
    {
        get
        {
            return popupScreenBounds;
        }
    }

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