using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public static Building playerInside = null;

    public int length = 5;
    public int spawnChance = 10;
    public Transform[] beds;
    public PropPlacement[] propPlacements;
    [HideInInspector] public City city;
    [HideInInspector] public int buildingIndex;
    [HideInInspector] public Material wall, floor;

    private int openDoors = 0;

    public void SetUpBuilding (City city, int buildingIndex, Material newWall, Material newFloor)
    {
        this.city = city;
        this.buildingIndex = buildingIndex;

        //Place random props
        for (int x = 0; x < propPlacements.Length; x++)
            propPlacements[x].ChooseAndPlaceProp(transform);

        //Flip house to introduce more variety
        if (Random.Range(0, 2) == 0)
            FlipHorizontally();

        //Repaint
        wall = newWall;
        floor = newFloor;
        RepaintRecursive(transform);
    }

    public void DoorOpened ()
    {
        //Lost air seal
        if (playerInside == this && openDoors == 0)
            Planet.planet.ambientVolume *= 4;

        openDoors++;
    }

    public void DoorClosed ()
    {
        //Gained air seal
        if (playerInside == this && openDoors == 1)
            Planet.planet.ambientVolume *= 0.25f;

        openDoors--;
    }

    public bool AirTight () { return openDoors <= 0; }

    public void RepaintRecursive (Transform t)
    {
        foreach(Transform child in t)
        {
            MeshRenderer theRenderer = child.GetComponent<MeshRenderer>();
            if (theRenderer && theRenderer.sharedMaterial == city.defaultWallMaterial)
                theRenderer.sharedMaterial = wall;
            else if (theRenderer && theRenderer.sharedMaterial == city.defaultFloorMaterial)
            {
                theRenderer.sharedMaterial = floor;
                if (floor.name.Contains("Wood"))
                    child.tag = "Wood";
            }

            if (child.childCount > 0)
                RepaintRecursive(child);
        }
    }

    public bool HasNoBeds () { return beds == null || beds.Length == 0; }

    public bool HasBedAtIndex (int bedIndex) { return bedIndex < beds.Length; }

    public Vector3 GetPositionOfBedAtIndex (int bedIndex) { return beds[bedIndex].position; }

    public void FlipHorizontally ()
    {
        Vector3 newScale = transform.localScale;
        newScale.x = -newScale.x;
        transform.localScale = newScale;

        /*
        //Flip door
        door.closePosition = -door.closePosition;
        door.openPosition = -door.openPosition;

        //Flip everything else
        Vector3 childPosition;
        foreach(Transform child in transform)
        {
            childPosition = child.localPosition;
            childPosition.x = -childPosition.x;
            child.localPosition = childPosition;
        }
        */
    }
}

[System.Serializable]
public class PropPlacement
{
    private int propCandidateIndex = -1;
    public PropCandidate[] propCandidates;

    public void ChooseAndPlaceProp (Transform building)
    {
        //Choose which prop to place
        propCandidateIndex = Random.Range(0, propCandidates.Length);

        //Then place it
        PlaceProp(building);
    }

    public void ChooseAndPlaceProp(Transform building, int propCandidateIndex)
    {
        //Choose which prop to place
        this.propCandidateIndex = propCandidateIndex;

        //Then place it
        PlaceProp(building);
    }

    public void PlaceProp (Transform building)
    {
        PropCandidate propCandidate = propCandidates[propCandidateIndex];

        //Spawn prop
        Transform placedProp = GameObject.Instantiate(propCandidate.prop).transform;

        //Adjust prop
        placedProp.parent = building;
        placedProp.localPosition = propCandidate.position;
        placedProp.localEulerAngles = propCandidate.rotation;
        placedProp.name = placedProp.name.Substring(0, placedProp.name.Length - 7);
    }

    public int GetPropCandidateIndex () { return propCandidateIndex; }
}

[System.Serializable]
public class PropCandidate
{
    public GameObject prop;
    public Vector3 position;
    public Vector3 rotation;
}

[System.Serializable]
public class BuildingJSON
{
    public BuildingJSON (Building building)
    {
        buildingIndex = building.buildingIndex;

        flipped = building.transform.localScale.x < 0;

        localPosition = building.transform.localPosition;
        localRotation = building.transform.localRotation;

        wall = building.wall.name;
        floor = building.floor.name;

        props = new List<int>(building.propPlacements.Length);
        for (int x = 0; x < building.propPlacements.Length; x++)
            props.Add(building.propPlacements[x].GetPropCandidateIndex());
    }

    public void RestoreBuilding (Building building, City city)
    {
        building.city = city;

        building.buildingIndex = buildingIndex;

        if (flipped)
            building.FlipHorizontally();

        building.transform.parent = city.transform;
        building.transform.localPosition = localPosition;
        building.transform.localRotation = localRotation;

        for(int x = 0; x < city.wallMaterials.Length; x++)
        {
            if (wall.Equals(city.wallMaterials[x].name))
            {
                building.wall = city.wallMaterials[x];
                break;
            }
        }

        for (int x = 0; x < city.floorMaterials.Length; x++)
        {
            if (floor.Equals(city.floorMaterials[x].name))
            {
                building.floor = city.floorMaterials[x];
                break;
            }
        }

        building.RepaintRecursive(building.transform);

        for (int x = 0; x < props.Count; x++)
            building.propPlacements[x].ChooseAndPlaceProp(building.transform, props[x]);
    }

    public int buildingIndex;
    public bool flipped;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public string wall, floor;
    public List<int> props;
}
