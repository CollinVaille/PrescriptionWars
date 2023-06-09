using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlanetNavMeshZoneUpdater
{
    AsyncOperation GenerateOrUpdateNavMesh(bool initialGeneration);
}
