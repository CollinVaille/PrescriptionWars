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
    private bool flipped = false;

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
            if (theRenderer && theRenderer.sharedMaterial == city.buildingManager.defaultWallMaterial)
                theRenderer.sharedMaterial = wall;
            else if (theRenderer && theRenderer.sharedMaterial == city.buildingManager.defaultFloorMaterial)
            {
                theRenderer.sharedMaterial = floor;
                PlanetMaterial.SetMaterialTypeBasedOnName(floor.name, child.gameObject);
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
        flipped = true;

        //Commented out approach is simpler but causes box collider warnings for negative scales
        /*
        Vector3 newScale = transform.localScale;
        newScale.x = -newScale.x;
        transform.localScale = newScale;
        */

        //Math for this is beyond my comprehension. Recursively performs flipping throughout child hierarchy
        FlipBuildingSection(transform);
    }

    private void FlipBuildingSection(Transform t)
    {
        foreach (Transform child in t)
        {
            //Flip door
            Door door = child.GetComponent<Door>();
            if (door && door.doorMotion == Door.DoorMotion.SlideX)
            {
                door.closePosition = -door.closePosition;
                door.openPosition = -door.openPosition;
            }

            //Flip position
            Vector3 childPosition = child.localPosition;
            childPosition.x = -childPosition.x;
            child.localPosition = childPosition;

            //Flip rotation
            Vector3 childRotation = child.localEulerAngles;
            childRotation.y = -childRotation.y;
            childRotation.z = -childRotation.z;
            child.localEulerAngles = childRotation;

            //Recurse to child
            if (child.childCount > 0)
                FlipBuildingSection(child);
        }
    }

    public bool Flipped () { return flipped; }
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

        flipped = building.Flipped();

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

        for(int x = 0; x < city.buildingManager.wallMaterials.Length; x++)
        {
            if (wall.Equals(city.buildingManager.wallMaterials[x].name))
            {
                building.wall = city.buildingManager.wallMaterials[x];
                break;
            }
        }

        for (int x = 0; x < city.buildingManager.floorMaterials.Length; x++)
        {
            if (floor.Equals(city.buildingManager.floorMaterials[x].name))
            {
                building.floor = city.buildingManager.floorMaterials[x];
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
