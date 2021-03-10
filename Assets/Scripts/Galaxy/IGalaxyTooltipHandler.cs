using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This interface exists so that you do not have to specify a parent on each galaxy tooltip component.
public interface IGalaxyTooltipHandler
{
    Transform TooltipsParent { get; }

    /* Ideal implementation of interface in a class (taken from GalaxyManager script):
    [SerializeField] private Transform tooltipsParent = null;
    public Transform TooltipsParent
    {
        get
        {
            return tooltipsParent;
        }
    } */
}
