using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGalaxyPopupBehaviourHandler
{
    Vector2 popupScreenBounds { get; }

    /* Ideal implementation of interface in a class:
    [SerializeField] private Vector2 popupScreenBounds = Vector2.zero;
    public Vector2 PopupScreenBounds
    {
        get
        {
            return popupScreenBounds;
        }
    } */
}
