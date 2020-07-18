using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void Interact (Pill interacting) { }

    public virtual void Interact (Pill interacting, bool turnOn) { }

    public virtual void ReleaseControl (bool voluntary) { }
}
