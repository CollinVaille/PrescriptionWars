using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This stores all the customization for cities that need to exist on a planet-wide basis. For example, all cities on a planet should have the same CityType and building materials.
public class PlanetwideCityCustomization
{
    //City type
    public CityTypeJSON cityType;

    //Materials
    public Material[] wallMaterials, floorMaterials; //Used for buildings and city walls
    public Material slabMaterial, groundMaterial; //Used for foundations

    public void GenerateNewPlanetwideCityCustomization()
    {
        //Load in customization options based on biome and sub-biome
        CityCustomizationMasterJSON.GetDataFromJSONFiles(out cityType, out BiomeCityOptionsJSON biomeOptions);

        //Wall materials--------------------------------------------------------------------------------------------------------------
        List<string> wallMats;
        if (cityType.wallMaterials != null && cityType.wallMaterials.Length != 0)
            wallMats = new List<string>(cityType.wallMaterials);
        else
            wallMats = new List<string>(biomeOptions.wallMaterials);

        wallMaterials = new Material[wallMats.Count];
        for (int x = 0; x < wallMats.Count; x++)
            wallMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + wallMats[x]);

        CityGenerator.TrimToRandomSubset(wallMats, Random.Range(3, 6));

        //Floor materials--------------------------------------------------------------------------------------------------------------
        List<string> floorMats;
        if (cityType.floorMaterials != null && cityType.floorMaterials.Length != 0)
            floorMats = new List<string>(cityType.floorMaterials);
        else
            floorMats = new List<string>(biomeOptions.floorMaterials);

        CityGenerator.TrimToRandomSubset(floorMats, Random.Range(2, 5));
        floorMaterials = new Material[floorMats.Count];
        for (int x = 0; x < floorMats.Count; x++)
            floorMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + floorMats[x]);

        //Slab materials--------------------------------------------------------------------------------------------------------------
        List<string> slabMats = new List<string>(biomeOptions.slabMaterials);
        slabMaterial = Resources.Load<Material>("Planet/City/Materials/" + slabMats[Random.Range(0, slabMats.Count)]);

        //Ground materials--------------------------------------------------------------------------------------------------------------
        List<string> groundMats = new List<string>(biomeOptions.groundMaterials);
        groundMaterial = Resources.Load<Material>("Planet/City/Materials/" + groundMats[Random.Range(0, groundMats.Count)]);
    }

}



[System.Serializable]
public class PlanetwideCityCustomizationJSON
{
    //City type
    public CityTypeJSON cityTypeJSON;

    //Materials
    public string[] wallMaterials, floorMaterials; //Used for buildings and city walls
    public string slabMaterial, groundMaterial; //Used for foundations

    public PlanetwideCityCustomizationJSON(PlanetwideCityCustomization planetCityCustomization)
    {
        cityTypeJSON = planetCityCustomization.cityType;

        wallMaterials = new string[planetCityCustomization.wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            wallMaterials[x] = planetCityCustomization.wallMaterials[x].name;

        floorMaterials = new string[planetCityCustomization.floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            floorMaterials[x] = planetCityCustomization.floorMaterials[x].name;

        slabMaterial = planetCityCustomization.slabMaterial.name;
        groundMaterial = planetCityCustomization.groundMaterial.name;
    }

    public void RestorePlanetwideCityCustomization(PlanetwideCityCustomization planetCityCustomization)
    {
        planetCityCustomization.cityType = cityTypeJSON;

        planetCityCustomization.wallMaterials = new Material[wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            planetCityCustomization.wallMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + wallMaterials[x]);

        planetCityCustomization.floorMaterials = new Material[floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            planetCityCustomization.floorMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + floorMaterials[x]);

        planetCityCustomization.slabMaterial = Resources.Load<Material>("Planet/City/Materials/" + slabMaterial);
        planetCityCustomization.groundMaterial = Resources.Load<Material>("Planet/City/Materials/" + groundMaterial);
    }
}