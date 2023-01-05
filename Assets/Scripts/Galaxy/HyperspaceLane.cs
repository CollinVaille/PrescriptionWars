﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HyperspaceLane : MonoBehaviour
{
    [Header("Material Components")]

    [SerializeField] private Material lineMaterial;

    //Non-inspector variables.

    private LineRenderer line = null;

    private List<GalaxySolarSystem> solarSystemsVar = null;
    /// <summary>
    /// Publicly accessible duplicate list of game objects that the line is attached to. The add and remove functions on this list will not work as intended.
    /// </summary>
    public List<GalaxySolarSystem> solarSystems
    {
        get
        {
            if (solarSystemsVar == null)
                return null;

            List<GalaxySolarSystem> list = new List<GalaxySolarSystem>();
            for(int index = 0; index < solarSystemsVar.Count; index++)
            {
                list.Add(solarSystemsVar[index]);
            }
            return list;
        }
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the start color of the hyperspace lane.
    /// </summary>
    public Color startColor
    {
        get => line == null ? Color.clear : line.startColor;
        set
        {
            if (line != null)
                line.startColor = value;
        }
    }
    /// <summary>
    /// Public property that should be used both to access and mutate the end color of the hyperspace lane.
    /// </summary>
    public Color endColor
    {
        get => line == null ? Color.clear : line.endColor;
        set
        {
            if (line != null)
                line.endColor = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Public method that should be used to initialize the hyperspace lane.
    /// </summary>
    /// <param name="solarSystems"></param>
    /// <param name="startColor"></param>
    /// <param name="endColor"></param>
    public void Initialize(List<GalaxySolarSystem> solarSystems, Color startColor, Color endColor)
    {
        if(line != null)
        {
            Destroy(gameObject.GetComponent<LineRenderer>());
            line = null;
        }

        solarSystemsVar = solarSystems;

        if (solarSystems == null || solarSystems.Count < 2)
            return;
        foreach (GalaxySolarSystem solarSystem in solarSystems)
            if (solarSystem == null)
                return;

        //Add a Line Renderer to the GameObject.
        line = gameObject.AddComponent<LineRenderer>();

        //Set the width of the Line Renderer.
        line.startWidth = 0.20f;
        line.endWidth = 0.20f;

        //Set the number of vertices of the Line Renderer.
        line.positionCount = solarSystems.Count;

        line.material = lineMaterial;

        line.startColor = startColor;
        line.endColor = endColor;

        for(int solarSystemIndex = 0; solarSystemIndex < solarSystems.Count; solarSystemIndex++)
        {
            line.SetPosition(solarSystemIndex, solarSystems[solarSystemIndex].transform.position);
        }
    }
}