using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationManager
{
    public enum FoundationType { NoFoundations, SingularSlab }

    public City city;
    public FoundationType foundationType = FoundationType.NoFoundations;
    public int foundationHeight = 0;

    public Material largeSlabMaterial, largeGroundMaterial;
    public Material slabMaterial, groundMaterial;
    public List<FoundationJSON> foundations;
    public List<Collider> foundationColliders;
    public List<VerticalScaler> verticalScalers;

    public FoundationManager(City city)
    {
        this.city = city;
        largeGroundMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Ground Material");
        largeSlabMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Slab Material");
    }

    public void GenerateNewFoundations()
    {
        if (foundationType == FoundationType.NoFoundations)
            return;

        FoundationSelections foundationSelections = new FoundationSelections(CityGenerator.generator.foundationOptions);

        if (foundationType == FoundationType.SingularSlab)
            GenerateNewSingleSlabFoundation(foundationSelections);

        //---

        //Update the slab and ground materials
        float scaling = city.radius / 10.0f;
        God.CopyMaterialValues(slabMaterial, largeSlabMaterial, scaling, foundationHeight / 20.0f, true);
        God.CopyMaterialValues(groundMaterial, largeGroundMaterial, scaling, scaling, true);

        //Updates the physics colliders based on changes to transforms.
        //Needed for raycasts to work correctly for the remainder of the city generation (since its all done in one frame).
        Physics.SyncTransforms();
    }

    private void GenerateNewSingleSlabFoundation(FoundationSelections foundationSelections)
    {
        Vector3 foundationScale = Vector3.one * city.radius * 2.15f;
        foundationScale.y = foundationHeight;
        GenerateNewFoundation(Vector3.zero, foundationScale, city.circularCity, foundationSelections);

        GenerateEntrancesForCardinalDirections(foundationSelections);
    }

    private void GenerateEntrancesForCardinalDirections(FoundationSelections foundationSelections)
    {
        bool circularEntrances = Random.Range(0, 2) == 0;
        float distanceFromCityCenter = city.radius * 1.075f;

        //Prepare for X gates
        city.GetWidestCardinalRoad(true, !city.circularCity, out float widestRoadWidth, out float widestRoadCenteredAt);
        Vector3 foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;
        
        //-X gate
        Vector3 foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, -distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, foundationSelections);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 180);

        //+X gate
        foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, foundationSelections);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 0);

        //Prepare for Z gates
        city.GetWidestCardinalRoad(false, !city.circularCity, out widestRoadWidth, out widestRoadCenteredAt);
        foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;

        //-Z gate
        foundationPosition = new Vector3(-distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, foundationSelections);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, -90);

        //+Z gate
        foundationPosition = new Vector3(distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, foundationSelections);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 90);
    }

    private void GenerateVerticalScalerBesideEntrance(Vector3 entrancePosition, Vector3 entranceScale, bool generateOnNegativeSide, int yAxisRotation)
    {
        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler("Planet/City/Miscellaneous/Elevator", city.transform, this);
        Transform verticalScalerTransform = verticalScaler.transform;

        //Rotate it
        verticalScaler.SetYAxisRotation(yAxisRotation);

        //Position it
        verticalScalerTransform.localPosition = entrancePosition;
        Vector3 translation = verticalScalerTransform.right * ((entranceScale.x / 2.0f) + (verticalScaler.width / 2.0f));
        if (generateOnNegativeSide)
            translation *= -1;
        verticalScalerTransform.Translate(translation, Space.World);
        verticalScalerTransform.Translate(verticalScalerTransform.forward * (verticalScaler.width / 2.0f), Space.World);

        //Scale it and connect it to the entrance foundation
        verticalScaler.ScaleToHeightAndConnect(foundationHeight / 2.0f, !generateOnNegativeSide);
    }

    private void GenerateNewFoundation(Vector3 localPosition, Vector3 localScale, bool circular, FoundationSelections foundationSelections)
    {
        //Take notes so we can save and restore the foundation later
        FoundationJSON foundation = new FoundationJSON();

        //Load in the customization
        foundation.prefab = foundationSelections.GetFoundationPrefab(circular, localScale);
        foundation.localPosition = localPosition;
        foundation.localScale = localScale;

        //Instantiate and place the foundation using the customization. This will also add it to the lists it needs to be in.
        foundation.GenerateFoundation(city);
    }
}

[System.Serializable]
public class FoundationManagerJSON
{
    //Materials
    public string slabMaterial, groundMaterial;
    public Vector2 slabMaterialTiling, groundMaterialTiling;

    //Foundations
    public List<FoundationJSON> foundations;

    //Vertical scalers
    public List<VerticalScalerJSON> verticalScalers;

    public FoundationManagerJSON(FoundationManager foundationManager)
    {
        slabMaterial = foundationManager.slabMaterial.name;
        slabMaterialTiling = foundationManager.largeSlabMaterial.mainTextureScale;

        groundMaterial = foundationManager.groundMaterial.name;
        groundMaterialTiling = foundationManager.largeGroundMaterial.mainTextureScale;

        foundations = foundationManager.foundations;

        verticalScalers = new List<VerticalScalerJSON>();
        foreach (VerticalScaler verticalScaler in foundationManager.verticalScalers)
            verticalScalers.Add(new VerticalScalerJSON(verticalScaler));
    }

    public void RestoreFoundationManager(FoundationManager foundationManager)
    {
        foundationManager.slabMaterial = Resources.Load<Material>("Planet/City/Materials/" + slabMaterial);
        God.CopyMaterialValues(foundationManager.slabMaterial, foundationManager.largeSlabMaterial, slabMaterialTiling.x, slabMaterialTiling.y, false);

        foundationManager.groundMaterial = Resources.Load<Material>("Planet/City/Materials/" + groundMaterial);
        God.CopyMaterialValues(foundationManager.groundMaterial, foundationManager.largeGroundMaterial, groundMaterialTiling.x, groundMaterialTiling.y, false);

        foreach (FoundationJSON foundation in foundations)
            foundation.GenerateFoundation(foundationManager.city);

        foreach (VerticalScalerJSON verticalScaler in verticalScalers)
            verticalScaler.RestoreVerticalScaler(foundationManager);

        Physics.SyncTransforms();
    }
}
