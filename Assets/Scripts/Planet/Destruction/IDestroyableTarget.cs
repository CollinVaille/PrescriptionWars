using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface used by bot pills to represent their target to attack (or in theory do other things to/with).
/// </summary>
public interface IDestroyableTarget
{
    Transform GetTargetTransform();

    bool IsDestroyed();
}
