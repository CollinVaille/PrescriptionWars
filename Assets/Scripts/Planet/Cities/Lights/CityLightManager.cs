using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityLightManager
{
    public City city;
    public GameObject cityLightPrefab;
    public List<PlanetLight> cityLights;

    public CityLightManager(City city)
    {
        this.city = city;
    }

    public void GenerateNewCityLights()
    {
        int targetPlacementCount = Mathf.CeilToInt(city.foundationManager.GetEstimatedUsableSquareMetersForCity() * 0.00004f);
        float highestAllowedPlacementHeightInGlobal = city.buildingManager.GetHighestBuildingElevationInLocal() + city.transform.position.y + 20.0f;

        int attempt = 1, lightsPlaced = 0;
        for (; attempt <= 200 && lightsPlaced < targetPlacementCount; attempt++)
        {
            Vector3 newPlaceInGlobal = GetRandomPlaceForNewCityLight();

            if (NewCityLightPlaceTooCloseToOtherLights(newPlaceInGlobal, 125.0f))
                continue;

            if (NewCityLightPlaceClashesWithOtherStuff(newPlaceInGlobal, highestAllowedPlacementHeightInGlobal, out float yCoordInGlobal))
                continue;

            newPlaceInGlobal.y = yCoordInGlobal;
            GenerateCityLight(newPlaceInGlobal);
            lightsPlaced++;
        }
    }

    public Vector3 GetRandomPlaceForNewCityLight()
    {
        List<Collider> foundationGroundColliders = city.foundationManager.foundationGroundColliders;
        if (foundationGroundColliders != null && foundationGroundColliders.Count != 0 && Random.Range(0, 2) == 0)
        {
            Collider foundationGroundCollider = foundationGroundColliders[Random.Range(0, foundationGroundColliders.Count)];
            Vector3 maxPoint = foundationGroundCollider.bounds.max;
            Vector3 minPoint = foundationGroundCollider.bounds.min;

            return new Vector3(Random.Range(minPoint.x, maxPoint.x), 0.0f, Random.Range(minPoint.z, maxPoint.z));
        }
        else
        {
            Vector3 newPlace = city.transform.position;

            //For circular cities, we don't want to go outside the circular perimeter, so our range for both x and z is going to be the median of the hypotenuse = 1/2 hypotenuse.
            float offsetRange = city.circularCity ? Mathf.Sqrt(city.radius * city.radius * 2.0f) * 0.5f : city.radius;
            newPlace.x += Random.Range(-offsetRange, offsetRange);
            newPlace.z += Random.Range(-offsetRange, offsetRange);

            return newPlace;
        }
    }

    public bool NewCityLightPlaceClashesWithOtherStuff(Vector3 globalCoordinate, float highestAllowedHeightInGlobal, out float yCoordInGlobal)
    {
        globalCoordinate.y = 6969.69f;

        if (Physics.Raycast(globalCoordinate, Vector3.down, out RaycastHit hitInfo, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
        {
            yCoordInGlobal = hitInfo.point.y;
            Collider hitCollider = hitInfo.collider;

            if (hitCollider.GetComponentInParent<Building>())
                return true;

            if (hitCollider.GetComponentInParent<Bridge>())
                return true;

            if (hitCollider.GetComponentInParent<VerticalScaler>())
                return true;

            if (hitCollider.GetComponentInParent<WallSection>())
                return true;

            if (hitCollider.GetComponent<Terrain>() && city.foundationManager.foundationType != FoundationManager.FoundationType.NoFoundations)
                return true;

            //Prevent lights from being placed on the top of tall spires
            if (yCoordInGlobal > highestAllowedHeightInGlobal)
                return true;

            return false;
        }
        else
        {
            yCoordInGlobal = 0.0f;
            return true;
        }
    }

    public bool NewCityLightPlaceTooCloseToOtherLights(Vector3 globalCoordinate, float tooCloseThreshold)
    {
        if (cityLights == null || cityLights.Count == 0)
            return false;

        foreach(PlanetLight cityLight in cityLights)
        {
            if (Vector3.Distance(cityLight.transform.position, globalCoordinate) < tooCloseThreshold)
                return true;
        }

        return false;
    }

    public void GenerateCityLight(Vector3 globalCoordinate)
    {
        //Create the light
        GameObject newLight = GameObject.Instantiate(cityLightPrefab);

        //Position it
        newLight.transform.parent = city.transform;
        newLight.transform.position = globalCoordinate;

        //Get the component on it that actually controls the lighting (needed for next parts)
        PlanetLight newPlanetLight = newLight.GetComponentInChildren<PlanetLight>();

        //Add it to the save system
        if (cityLights == null)
            cityLights = new List<PlanetLight>();
        cityLights.Add(newPlanetLight);
    }
}

[System.Serializable]
public class CityLightManagerJSON
{
    public string cityLightPrefab;
    public List<Vector3> cityLightGlobalPositions;

    public CityLightManagerJSON(CityLightManager cityLightManager)
    {
        cityLightPrefab = cityLightManager.cityLightPrefab.name;

        cityLightGlobalPositions = new List<Vector3>();
        foreach (PlanetLight cityLight in cityLightManager.cityLights)
            cityLightGlobalPositions.Add(cityLight.GetObjectRoot().position);
    }

    public void RestoreCityLightManager(CityLightManager cityLightManager, string cityTypePathPrefix)
    {
        cityLightManager.cityLightPrefab = Resources.Load<GameObject>(cityTypePathPrefix + "Lights/" + cityLightPrefab);

        foreach (Vector3 cityLightGlobalPosition in cityLightGlobalPositions)
            cityLightManager.GenerateCityLight(cityLightGlobalPosition);
    }
}