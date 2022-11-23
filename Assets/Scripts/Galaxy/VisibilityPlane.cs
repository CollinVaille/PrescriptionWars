using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityPlane : MonoBehaviour
{
    /// <summary>
    /// Private list that holds all of the functions that need to be executed whenever the visibility plane becomes visible to the main camera.
    /// </summary>
    private List<Action> onBecameVisibleFunctions = null;
    /// <summary>
    /// Private list that holds all of the functions that need to be executed whenever the visibility plane becomes invisible to the main camera.
    /// </summary>
    private List<Action> onBecameInvisibleFunctions = null;

    /// <summary>
    /// Public method that should be used in order to add a function to the list of functions to be executed whenever the visibility plane becomes visible to the main camera.
    /// </summary>
    /// <param name="onBecameVisibleFunction"></param>
    public void AddOnBecameVisibleFunction(Action onBecameVisibleFunction)
    {
        if (onBecameVisibleFunctions == null)
            onBecameVisibleFunctions = new List<Action>();
        onBecameVisibleFunctions.Add(onBecameVisibleFunction);
    }

    /// <summary>
    /// Public method that should be used in order to add a function to the list of functions to be executed whenever the visibility plane becomes invisible to the main camera.
    /// </summary>
    /// <param name="onBecameInvisibleFunction"></param>
    public void AddOnBecameInvisibleFunction(Action onBecameInvisibleFunction)
    {
        if (onBecameInvisibleFunctions == null)
            onBecameInvisibleFunctions = new List<Action>();
        onBecameInvisibleFunctions.Add(onBecameInvisibleFunction);
    }

    /// <summary>
    /// Public method that should be used in order to remove a function from the list of functions to be executed whenever the visibility plane becomes visible to the main camera.
    /// </summary>
    /// <param name="onBecameVisibleFunction"></param>
    public void RemoveOnBecameVisibleFunction(Action onBecameVisibleFunction)
    {
        if (onBecameVisibleFunctions != null && onBecameVisibleFunctions.Contains(onBecameVisibleFunction))
        {
            onBecameVisibleFunctions.Remove(onBecameVisibleFunction);
            if (onBecameVisibleFunctions.Count == 0)
                onBecameVisibleFunctions = null;
        }
    }

    /// <summary>
    /// Public method that should be used in order to remove a function from the list of functions to be executed whenever the visibility plane becomes invisible to the main camera.
    /// </summary>
    /// <param name="onBecameInvisibleFunction"></param>
    public void RemoveOnBecameInvisibleFunction(Action onBecameInvisibleFunction)
    {
        if(onBecameInvisibleFunctions != null && onBecameInvisibleFunctions.Contains(onBecameInvisibleFunction))
        {
            onBecameInvisibleFunctions.Remove(onBecameInvisibleFunction);
            if (onBecameInvisibleFunctions.Count == 0)
                onBecameInvisibleFunctions = null;
        }
    }

    /// <summary>
    /// Private method that is called whenever the visibility plane becomes visible to the main camera and executes all functions in the list of OnBecameVisibleFunctions.
    /// </summary>
    private void OnBecameVisible()
    {
        if (onBecameVisibleFunctions != null)
            foreach (Action onBecameVisibleFunction in onBecameVisibleFunctions)
                onBecameVisibleFunction();
    }

    /// <summary>
    /// Private method that is called whenever the visibility plane becomes invisible to the main camera and executes all functions in the list of OnBecameInvisibleFunctions.
    /// </summary>
    private void OnBecameInvisible()
    {
        if (onBecameInvisibleFunctions != null)
            foreach (Action onBecameInvisibleFunction in onBecameInvisibleFunctions)
                onBecameInvisibleFunction();
    }
}
