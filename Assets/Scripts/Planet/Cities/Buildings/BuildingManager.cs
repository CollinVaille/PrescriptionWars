using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager
{
    public City city;

    //Building construction
    public List<GameObject> buildingPrefabs;
    private Building[] buildingPrototypes; //First instance of each building (parallel array to one above)
    private int totalSpawnChance = 0;

    //Building maintenance
    private int nextAvailableBuilding = 0, nextAvailableBed = -1;
    public List<Building> buildings;

    //Materials
    public Material defaultWallMaterial, defaultFloorMaterial;
    public Material[] wallMaterials, floorMaterials;

    public BuildingManager(City city)
    {
        this.city = city;
    }

    public void LoadBuildingPrototypes()
    {
        buildingPrototypes = new Building[buildingPrefabs.Count];

        for (int x = 0; x < buildingPrefabs.Count; x++)
        {
            buildingPrototypes[x] = GameObject.Instantiate(buildingPrefabs[x]).GetComponent<Building>();
            buildingPrototypes[x].gameObject.SetActive(false);

            if (!buildingPrototypes[x].CompareTag("Special Building"))
                totalSpawnChance += buildingPrototypes[x].spawnChance;
        }
    }

    public void GenerateNewBuildings(int avgBlockLength)
    {
        buildings = new List<Building>();
        GenerateNewSpecialBuildings();
        GenerateNewGenericBuildings(avgBlockLength);
    }

    private Transform InstantiateNewBuilding(int buildingIndex)
    {
        if (!buildingPrototypes[buildingIndex].gameObject.activeSelf) //Is model home available?
        {
            buildingPrototypes[buildingIndex].gameObject.SetActive(true);
            return buildingPrototypes[buildingIndex].transform;
        }
        else //Fine, we'll create a new one
            return GameObject.Instantiate(buildingPrefabs[buildingIndex]).transform;
    }


    private void GenerateNewSpecialBuildings()
    {
        for (int x = 0; x < buildingPrototypes.Length; x++)
        {
            //For each special building that is supposed to be included in the city
            if (buildingPrototypes[x].CompareTag("Special Building"))
            {
                //Keep trying to generate it until we succeed or hit 50 attempts
                for (int attempt = 1; attempt <= 50; attempt++)
                {
                    if (GenerateNewBuilding(x, true, true))
                        break;
                }
            }
        }
    }

    private void GenerateNewGenericBuildings(int averageBlockLength)
    {
        //Go through all the large generic buildings and give each once chance to spawn early (so they doesn't get crowded out by a bunch of small buildings)
        for (int x = 0; x < buildingPrototypes.Length; x++)
        {
            if (buildingPrototypes[x].CompareTag("Special Building"))
                continue;

            if (buildingPrototypes[x].length > averageBlockLength)
                GenerateNewBuilding(x, true, false);
        }

        int buildingIndex = -1;
        for (int x = 0; x < 400; x++)
        {
            //Randomly pick a model to build
            buildingIndex = SelectGenericBuildingPrototype();

            //Attempt to place it somewhere
            GenerateNewBuilding(buildingIndex, buildingPrototypes[buildingIndex].length > averageBlockLength, false);
        }
    }

    //Used to generate a NEW building. Pass in index of model if particular one is desired, else a random model will be selected.
    //Specify aggressive placement to ignore roads--if necessary--during placement. The algorithm will still try to take roads into account if it can.
    //Returns whether building was successfully generated.
    private bool GenerateNewBuilding(int buildingIndex, bool placeInLargestAvailableBlock, bool overrideRoadsIfNeeded)
    {
        AreaManager areaManager = city.areaManager;

        //Find place that can fit model...

        int newX = 0, newZ = 0;
        int buildingRadius = buildingPrototypes[buildingIndex].length + city.newCitySpecifications.extraUsedBuildingRadius;
        int totalRadius = buildingRadius + city.newCitySpecifications.extraBuildingRadiusForSpacing;
        int areaLength = Mathf.CeilToInt(totalRadius * 1.0f / areaManager.areaSize);
        bool foundPlace = false;

        //Placement strategy #1: Center on the largest available city block
        if (placeInLargestAvailableBlock)
        {
            while (areaManager.availableCityBlocks.Count > 0)
            {
                int indexToPop = areaManager.availableCityBlocks.Count - 1;
                CityBlock possibleLocation = areaManager.availableCityBlocks[indexToPop];
                areaManager.availableCityBlocks.RemoveAt(indexToPop);

                //The largest city block left is not big enough, so resort to random placement
                //Keep the >= because same size generates will fail SafeToGenerate (tested)
                if (areaLength >= possibleLocation.GetSmallestDimension())
                    break;

                //Block is large enough for the building, but is there something already in it?
                newX = possibleLocation.coords.x;
                newZ = possibleLocation.coords.y;
                if (areaManager.SafeToGenerate(newX, newZ, areaLength, AreaManager.AreaReservationType.Open, false))
                {
                    foundPlace = true;
                    break;
                }
            }
        }

        //Placement strategy #2: Random placement
        if (!foundPlace)
        {
            int maxAttempts = overrideRoadsIfNeeded ? 200 : 50;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                newX = Random.Range(0, areaManager.areaTaken.GetLength(0));
                newZ = Random.Range(0, areaManager.areaTaken.GetLength(1));

                if (areaManager.SafeToGenerate(newX, newZ, areaLength, AreaManager.AreaReservationType.Open, maxAttempts > 150))
                {
                    foundPlace = true;
                    break;
                }
            }
        }

        //If found place, create model, position it, and call set up on it
        if (foundPlace)
        {
            //Reserve area for building
            areaManager.ReserveAreasRegardlessOfType(newX, newZ, areaLength, AreaManager.AreaReservationType.ReservedByBuilding);

            //Create it
            Transform newBuilding = InstantiateNewBuilding(buildingIndex);
            newBuilding.parent = city.transform;
            newBuilding.localRotation = Quaternion.Euler(0, 0, 0);

            //Rotate it
            bool hasCardinalRotation = SetBuildingRotation(newBuilding, newX + (areaLength / 2), newZ + (areaLength / 2));

            //Position it...
            Vector3 buildingPosition = Vector3.zero;

            //Compute the location for the building: centered within allocated area, converting from area space to local coordinates
            buildingPosition.x = (newX + (areaLength / 2.0f) - areaManager.areaTaken.GetLength(0) / 2.0f) * areaManager.areaSize;
            buildingPosition.z = (newZ + (areaLength / 2.0f) - areaManager.areaTaken.GetLength(1) / 2.0f) * areaManager.areaSize;

            //Apply the computed location to the building and any foundation underneath the building if applicable...
            //This part depends on whether we generate a foundation underneath the building because that changes the building's y position
            Foundation buildingFoundation = city.foundationManager.RightBeforeBuildingGenerated(buildingRadius, hasCardinalRotation, buildingPosition);
            if (buildingFoundation != null) //Has foundation underneath building
            {
                //Place building on top of foundation
                buildingPosition.y += (buildingFoundation.localScale.y / 2.0f);
                newBuilding.localPosition = buildingPosition;
            }
            else //No foundation
            {
                //Place building on whatever is beneath it (could be terrain or foundations previously created)
                newBuilding.localPosition = buildingPosition;
                God.SnapToGround(newBuilding, collidersToCheckAgainst: city.foundationManager.foundationGroundColliders);
            }

            //Remember building and finally, call set up on it
            buildings.Add(newBuilding.GetComponent<Building>());
            newBuilding.GetComponent<Building>().SetUpBuilding(
                city,
                buildingIndex,
                wallMaterials[Random.Range(0, wallMaterials.Length)],
                floorMaterials[Random.Range(0, floorMaterials.Length)]);
        }
        //Otherwise, we fucking give up

        return foundPlace;
    }

    private int SelectGenericBuildingPrototype()
    {
        for (int attempt = 1; attempt <= 50; attempt++)
        {
            for (int x = 0; x < buildingPrototypes.Length; x++)
            {
                //The end of the prototypes array is where the special buildings are. Since this function is to pick generic models, we stop once we hit the special section
                if (buildingPrototypes[x].CompareTag("Special Building"))
                    break;

                if (Random.Range(0, totalSpawnChance) < buildingPrototypes[x].spawnChance)
                    return x;
            }
        }

        return 0;
    }

    //Returns whether building rotation is strictly pointing in a cardinal direction (0, 90, 180, 270 degrees within city coordinate system)
    private bool SetBuildingRotation(Transform building, int xCoord, int zCoord)
    {
        AreaManager areaManager = city.areaManager;

        Vector3 newRotation = Vector3.zero;
        int newMargin;
        bool strictlyCardinal;

        //Find closest horizontal road
        int closestZMargin = 9999;
        bool faceDown = false;
        for (int z = 0; z < areaManager.horizontalRoads.Count; z++)
        {
            newMargin = Mathf.Abs(zCoord - areaManager.horizontalRoads[z]);
            if (newMargin < closestZMargin)
            {
                closestZMargin = newMargin;
                faceDown = zCoord > areaManager.horizontalRoads[z];
            }
        }

        //Find closest vertical road
        int closestXMargin = 9999;
        bool faceLeft = false;
        for (int x = 0; x < areaManager.verticalRoads.Count; x++)
        {
            newMargin = Mathf.Abs(xCoord - areaManager.verticalRoads[x]);
            if (newMargin < closestXMargin)
            {
                closestXMargin = newMargin;
                faceLeft = xCoord > areaManager.verticalRoads[x];
            }
        }

        //Review results and determine rotation
        if (closestXMargin == closestZMargin && Random.Range(0, 2) == 0) //Rotate according to both horizontal and vertical
        {
            strictlyCardinal = false;

            if (faceLeft)
            {
                if (faceDown)
                    newRotation.y = -135;
                else
                    newRotation.y = -45;
            }
            else
            {
                if (faceDown)
                    newRotation.y = 135;
                else
                    newRotation.y = 45;
            }
        }
        else if (closestXMargin < closestZMargin) //Rotate according to closest vertical road
        {
            strictlyCardinal = true;

            if (faceLeft)
                newRotation.y = -90;
            else
                newRotation.y = 90;
        }
        else  //Rotate according to closest horizontal road
        {
            strictlyCardinal = true;

            if (faceDown)
                newRotation.y = 180;
            else
                newRotation.y = 0;
        }

        //Apply rotation
        building.localEulerAngles = newRotation;

        return strictlyCardinal;
    }

    public int GetLongestBuildingLength()
    {
        int longestBuildingLength = 0;
        foreach (Building building in buildingPrototypes)
        {
            if (longestBuildingLength < building.length)
                longestBuildingLength = building.length;
        }

        return longestBuildingLength;
    }

    public int GetMaxLengthBetweenRoadsInLocalUnits()
    {
        return Mathf.Max(70, (int)(GetLongestBuildingLength() * 1.5f));
    }

    public Vector3 GetNewSpawnPoint()
    {
        nextAvailableBed++;

        Building building = null;
        bool foundBed = false;

        for (int attempt = 1; !foundBed && attempt <= 50; attempt++)
        {
            if (nextAvailableBuilding >= buildings.Count)
                nextAvailableBuilding = 0;

            building = buildings[nextAvailableBuilding];

            if (building.HasNoBeds())
            {
                nextAvailableBuilding++;
                continue;
            }

            if (!building.HasBedAtIndex(nextAvailableBed))
            {
                nextAvailableBed = 0;
                nextAvailableBuilding++;
            }
            else
                foundBed = true;
        }

        if (foundBed)
            return building.GetPositionOfBedAtIndex(nextAvailableBed) + Vector3.up * 3;
        else
            return city.transform.position; //Couldn't find bed so shove into center of city as fallback
    }

    public void SetDefaultMaterials()
    {
        defaultWallMaterial = Resources.Load<Material>("Planet/City/Materials/" + city.cityType.defaultWallMaterial);
        defaultFloorMaterial = Resources.Load<Material>("Planet/City/Materials/" + city.cityType.defaultFloorMaterial);
    }
}

[System.Serializable]
public class BuildingManagerJSON
{
    //Buildings
    public List<string> buildingPrefabs;
    public List<BuildingJSON> buildings;

    //Materials
    public string[] wallMaterials, floorMaterials;

    public BuildingManagerJSON(BuildingManager buildingManager)
    {
        wallMaterials = new string[buildingManager.wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            wallMaterials[x] = buildingManager.wallMaterials[x].name;

        floorMaterials = new string[buildingManager.floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            floorMaterials[x] = buildingManager.floorMaterials[x].name;

        buildingPrefabs = new List<string>(buildingManager.buildingPrefabs.Count);
        foreach (GameObject buildingPrefab in buildingManager.buildingPrefabs)
            buildingPrefabs.Add(buildingPrefab.name);

        buildings = new List<BuildingJSON>(buildingManager.buildings.Count);
        for (int x = 0; x < buildingManager.buildings.Count; x++)
            buildings.Add(new BuildingJSON(buildingManager.buildings[x]));
    }

    public void RestoreBuildingManager(BuildingManager buildingManager, string cityTypePathSuffix)
    {
        buildingManager.SetDefaultMaterials();

        buildingManager.wallMaterials = new Material[wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            buildingManager.wallMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + wallMaterials[x]);

        buildingManager.floorMaterials = new Material[floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            buildingManager.floorMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + floorMaterials[x]);

        buildingManager.buildingPrefabs = new List<GameObject>();
        for (int x = 0; x < buildingPrefabs.Count; x++)
            buildingManager.buildingPrefabs.Add(Resources.Load<GameObject>("Planet/City/Buildings/" + cityTypePathSuffix + buildingPrefabs[x]));

        buildingManager.buildings = new List<Building>(buildings.Count);
        for (int x = 0; x < buildings.Count; x++)
        {
            Building newBuilding = GameObject.Instantiate(buildingManager.buildingPrefabs[
                buildings[x].buildingIndex]).GetComponent<Building>();
            buildings[x].RestoreBuilding(newBuilding, buildingManager.city);
            buildingManager.buildings.Add(newBuilding);
        }
    }
}
