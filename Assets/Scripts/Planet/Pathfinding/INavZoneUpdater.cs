using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INavZoneUpdater
{
    void GenerateNavMesh();

    AsyncOperation UpdateNavMesh();
}
