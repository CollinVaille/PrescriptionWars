using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlanetPooledObject
{
    void OneTimeSetUp();

    void CleanUp();
}
