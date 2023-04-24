using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityWallManager
{
    public City city;

    public GameObject wallSectionPrefab, horGatePrefab, verGatePrefab, fencePostPrefab;
    public Transform walls;
    public int wallMaterialIndex;
    public Material cityWallMaterial;
    public List<NewCityWallRequest> newCityWallRequests;

    public CityWallManager(City city)
    {
        this.city = city;
        cityWallMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/City Walls");
        newCityWallRequests = new List<NewCityWallRequest>();
    }

    public void PrepareWalls(int wallMaterialIndex)
    {
        this.wallMaterialIndex = wallMaterialIndex;

        //Prepare for wall creation
        walls = new GameObject("City Walls").transform;
        walls.parent = city.transform;
        walls.localPosition = Vector3.zero;
        walls.localRotation = Quaternion.Euler(0, 0, 0);

        //Set texture of walls using reference material
        Material referenceMaterial = Planet.planet.planetWideCityCustomization.wallMaterials[wallMaterialIndex];
        God.CopyMaterialValues(referenceMaterial, cityWallMaterial, 3.0f, 1.5f, true);
    }

    public void GenerateNewWalls()
    {
        if (!wallSectionPrefab)
            return;

        PrepareWalls(Random.Range(0, Planet.planet.planetWideCityCustomization.wallMaterials.Length));

        ProcessNewCityWallRequests();

        if (city.newCitySpecifications.shouldGenerateCityPerimeterWalls)
            GenerateNewWallsAroundCityPerimeter();
    }

    private void ProcessNewCityWallRequests()
    {
        foreach (NewCityWallRequest cityWallRequest in newCityWallRequests)
        {
            if (cityWallRequest.circular)
                GenerateNewCircularWalls(cityWallRequest.localCenter, cityWallRequest.halfLength);
            else
                GenerateNewSquareWalls(cityWallRequest.localCenter, cityWallRequest.halfLength);
        }
    }

    private void GenerateNewWallsAroundCityPerimeter()
    {
        if (city.circularCity) //Circular perimeters walls
            GenerateNewCircularWalls(walls.localPosition, city.radius);
        else //Square perimeter walls
        {
            float squareWallHalfLength = city.areaManager.areaSize * city.areaManager.areaTaken.GetLength(0) * 0.5f;
            GenerateNewSquareWalls(walls.localPosition, squareWallHalfLength);
        }
    }

    private void GenerateNewCircularWalls(Vector3 localCenter, float radius)
    {
        float wallLength = wallSectionPrefab.GetComponent<WallSection>().XScale;

        Transform temporaryRotatingBase = new GameObject("Temp - Delete After City Generation").transform;
        temporaryRotatingBase.parent = walls.parent;
        temporaryRotatingBase.localPosition = localCenter;
        temporaryRotatingBase.localRotation = walls.localRotation;

        //Determine how many wall sections to place, the spacing, the angular math...
        Vector3 fenceOffsetFromBase = Vector3.forward * radius;
        float circumference = 2 * Mathf.PI * radius;
        int targetWallCount = (int)(circumference / wallLength);
        float currentEulerAngle = 0.0f;
        float eulerAngleStep = 360.0f / targetWallCount;

        //Precalculations for when to make the wall section a gate
        //The algorithm is to make a section a gate when its rotation is within 7.5 degrees of a cardinal direction (up, down, left, right)
        float eulerAngleGateThreshold = 7.5f;
        float[] gateAngles = new float[5];
        for (int x = 0; x < gateAngles.Length; x++)
            gateAngles[x] = x * (360.0f / 4.0f);

        //Place the wall sections, one section per iteration
        bool lastWasGate = false;
        for (int wallsPlaced = 0; wallsPlaced < targetWallCount; wallsPlaced++)
        {
            //Compute the new fence location
            Vector3 newFenceLocation = temporaryRotatingBase.TransformPoint(fenceOffsetFromBase);
            newFenceLocation = walls.InverseTransformPoint(newFenceLocation);

            //Determine its angle
            int fencePostRotation = (int)(currentEulerAngle + eulerAngleStep / 2.0f);

            //Determine whether it should be a gate
            bool isGate = false;
            for (int x = 0; x < gateAngles.Length; x++)
            {
                if (Mathf.Abs(gateAngles[x] + currentEulerAngle) < eulerAngleGateThreshold)
                {
                    isGate = true;
                    break;
                }
            }

            //Place it
            PlaceWallSection(isGate, lastWasGate,
                newFenceLocation.x, newFenceLocation.y, newFenceLocation.z,
                (int)temporaryRotatingBase.localEulerAngles.y, true, wallLength, fencePostRotation);

            //Rotate the base for the next iteration
            currentEulerAngle -= eulerAngleStep;
            temporaryRotatingBase.localEulerAngles = Vector3.up * currentEulerAngle;

            //Other preparation for next iteration
            lastWasGate = isGate;
        }

        GameObject.Destroy(temporaryRotatingBase.gameObject);
    }

    private void GenerateNewSquareWalls(Vector3 localCenter, float halfLength)
    {
        //Determine height information
        int placementHeight = (int)localCenter.y;
        
        //Determine wall length and number of sections one way
        float wallLength = wallSectionPrefab.GetComponent<WallSection>().XScale;
        int sectionsOneWay = Mathf.CeilToInt((halfLength * 2) / wallLength);
        wallLength = (halfLength * 2) / sectionsOneWay;

        //Determine x,z starting positions
        float startingOffsetAbsoluteValue = halfLength - wallLength * 0.5f;
        float startX = localCenter.x - startingOffsetAbsoluteValue;
        float startZ = localCenter.z - startingOffsetAbsoluteValue;

        //TEMPORARY. Used for debugging
        /*Vector3 bottomLeft = city.transform.TransformPoint(localCenter + Vector3.left * halfLength + Vector3.back * halfLength);
        God.PlaceDebugMarker(bottomLeft, markerName: "Bottom Left Marker");
        Vector3 topRight = city.transform.TransformPoint(localCenter + Vector3.right * halfLength + Vector3.forward * halfLength);
        God.PlaceDebugMarker(topRight, markerName: "Top Right Marker"); */

        //HORIZONTAL WALLS-------------------------------------------------------------------------------------

        bool[] skipWallSection = new bool[sectionsOneWay];

        //Find the largest road/gap
        city.areaManager.GetWidestCardinalRoad(true, true, out _, out float largestGapCenteredAt);

        //Find the wall section closest to the gap and remove it
        int closestWallSectionIndex = 0;
        float closestWallSectionDist = Mathf.Infinity;
        for (int x = 0; x < skipWallSection.Length; x++)
        {
            float wallSectionCenteredAt = startX + x * wallLength;
            float dist = Mathf.Abs(wallSectionCenteredAt - largestGapCenteredAt);
            if (dist < closestWallSectionDist)
            {
                closestWallSectionIndex = x;
                closestWallSectionDist = dist;
            }
        }
        skipWallSection[closestWallSectionIndex] = true;

        //Now that the wall section locations have been determined, place them
        float minZ = startZ - wallLength * 0.5f;
        float maxZ = minZ + sectionsOneWay * wallLength;
        bool previousWasGate = false;
        for (int x = 0; x < skipWallSection.Length; x++)
        {
            bool nextIsGate = false;
            if (x < sectionsOneWay - 1)
                nextIsGate = skipWallSection[x + 1];

            //Front walls
            PlaceWallSection(skipWallSection[x], previousWasGate,
                startX + x * wallLength, placementHeight, minZ, 180, true, wallLength);

            //Back walls
            PlaceWallSection(skipWallSection[x], nextIsGate,
                startX + x * wallLength, placementHeight, maxZ, 0, true, wallLength);

            previousWasGate = skipWallSection[x];
        }

        bool firstHorSectionIsGate = skipWallSection[0];
        bool lastHorSectionIsGate = skipWallSection[sectionsOneWay - 1];

        //VERTICAL WALLS-------------------------------------------------------------------------------------

        skipWallSection = new bool[sectionsOneWay];

        //Find the largest road/gap
        city.areaManager.GetWidestCardinalRoad(false, true, out _, out largestGapCenteredAt);

        //Find the wall section closest to the gap and remove it
        closestWallSectionIndex = 0;
        closestWallSectionDist = Mathf.Infinity;
        for (int z = 0; z < skipWallSection.Length; z++)
        {
            float wallSectionCenteredAt = startZ + z * wallLength;
            float dist = Mathf.Abs(wallSectionCenteredAt - largestGapCenteredAt);
            if (dist < closestWallSectionDist)
            {
                closestWallSectionIndex = z;
                closestWallSectionDist = dist;
            }
        }
        skipWallSection[closestWallSectionIndex] = true;

        //Now that the wall section locations have been determined, place them
        float minX = startX - wallLength * 0.5f;
        float maxX = minX + sectionsOneWay * wallLength;
        previousWasGate = false;
        for (int z = 0; z < skipWallSection.Length; z++)
        {
            bool nextIsGate = false;
            if (z < sectionsOneWay - 1)
                nextIsGate = skipWallSection[z + 1];

            PlaceWallSection(skipWallSection[z], nextIsGate,
                minX, placementHeight, startZ + z * wallLength, -90, true, wallLength);

            PlaceWallSection(skipWallSection[z], previousWasGate,
                maxX, placementHeight, startZ + z * wallLength, 90, true, wallLength);

            previousWasGate = skipWallSection[z];
        }
    }

    public void PlaceWallSection(bool gate, bool skipFencePost, float x, float y, float z, int rotation, bool snapToGround, float xScale, int fencePostRotation = 9000)
    {
        int absRotation = Mathf.Abs(rotation);
        bool horizontalSection = (absRotation == 0 || absRotation == 180);

        //Place wall section
        Transform newWallSection;
        if (gate)
            newWallSection = GameObject.Instantiate(horizontalSection ? horGatePrefab : verGatePrefab, walls).transform;
        else
            newWallSection = GameObject.Instantiate(wallSectionPrefab, walls).transform;

        //Set rotation
        newWallSection.localRotation = Quaternion.Euler(0, rotation, 0);

        //Set position
        Vector3 wallPosition = new Vector3(x, y, z);
        newWallSection.localPosition = wallPosition;
        if (snapToGround)
            God.SnapToGround(newWallSection, collidersToCheckAgainst: city.foundationManager.foundationGroundColliders);

        //Set scale
        newWallSection.GetComponent<WallSection>().XScale = xScale;

        //Place fence post correlating to wall section
        if (fencePostPrefab && !gate && !skipFencePost)
        {
            Vector3 fencePostPosition = newWallSection.TransformPoint(new Vector3(0.5f, 0.0f, 0.0f));
            fencePostPosition = walls.InverseTransformPoint(fencePostPosition);

            /*
            if (horizontalSection)
                fencePostPosition.x += newWallSection.localScale.x / 2.0f;
            else
                fencePostPosition.z += newWallSection.localScale.x / 2.0f;  */

            PlaceFencePost(fencePostPosition, fencePostRotation < 9000 ? fencePostRotation : newWallSection.localEulerAngles.y);
        }
    }

    public void PlaceFencePost(Vector3 position, float rotation)
    {
        Transform fencePost = GameObject.Instantiate(fencePostPrefab, walls).transform;
        fencePost.localEulerAngles = new Vector3(0, rotation, 0);
        fencePost.localPosition = position;
    }

}

[System.Serializable]
public class CityWallManagerJSON
{
    public bool hasWalls;
    public int wallMaterialIndex;
    public string wallSection, horGate, verGate, fencePost;

    public List<WallSectionJSON> wallSectionJSONs;

    public CityWallManagerJSON(CityWallManager cityWallManager)
    {
        //City walls
        Transform cityWalls = cityWallManager.walls;
        hasWalls = cityWalls;
        if (hasWalls)
        {
            wallMaterialIndex = cityWallManager.wallMaterialIndex;

            wallSection = cityWallManager.wallSectionPrefab.name;
            horGate = cityWallManager.horGatePrefab.name;
            verGate = cityWallManager.verGatePrefab.name;
            if (cityWallManager.fencePostPrefab)
                fencePost = cityWallManager.fencePostPrefab.name;
            else
                fencePost = "";

            wallSectionJSONs = new List<WallSectionJSON>();
            foreach (Transform wallSectionTransform in cityWalls)
            {
                WallSection wallSection = wallSectionTransform.GetComponent<WallSection>();
                if(wallSection)
                    wallSectionJSONs.Add(new WallSectionJSON(wallSection));
            }
        }
        else
        {
            wallMaterialIndex = -1;

            wallSection = "";
            horGate = "";
            verGate = "";
            fencePost = "";

            wallSectionJSONs = null;
        }
    }

    public void RestoreCityWallManager(CityWallManager cityWallManager, string cityTypePathSuffix)
    {
        if (hasWalls)
        {
            cityWallManager.wallSectionPrefab = Resources.Load<GameObject>("Planet/City/Wall Sections/" + cityTypePathSuffix + wallSection);
            cityWallManager.horGatePrefab = Resources.Load<GameObject>("Planet/City/Gates/" + cityTypePathSuffix + horGate);
            cityWallManager.verGatePrefab = Resources.Load<GameObject>("Planet/City/Gates/" + cityTypePathSuffix + verGate);
            if (!fencePost.Equals(""))
                cityWallManager.fencePostPrefab = Resources.Load<GameObject>("Planet/City/Fence Posts/" + cityTypePathSuffix + fencePost);

            cityWallManager.PrepareWalls(wallMaterialIndex);

            foreach (WallSectionJSON wallSectionJSON in wallSectionJSONs)
                wallSectionJSON.RestoreWallSection(cityWallManager);
        }
    }
}