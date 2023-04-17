using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This stores all the customization for cities that need to exist on a planet-wide basis. For example, all cities on a planet should have the same CityType and building materials.
public class PlanetCityCustomization
{
    //City type
    public CityType cityType;

    //Materials
    public Material[] wallMaterials, floorMaterials; //Used for buildings and city walls
    public Material slabMaterial, groundMaterial; //Used for foundations

    public void GenerateCityCustomizationForNewPlanet()
    {
        //Get biome-city options
        BiomeCityOptions biomeOptions = CityGenerator.generator.GetBiomeCityOptions();
        BiomeCityOptions defaultBiomeOptions = CityGenerator.generator.biomeCityOptions[0];

        //City type--------------------------------------------------------------------------------------------------------------
        cityType = SelectCityType();

        //Wall materials--------------------------------------------------------------------------------------------------------------
        List<string> wallMats;
        if (cityType.wallMaterials != null && cityType.wallMaterials.Length != 0)
            wallMats = new List<string>(cityType.wallMaterials);
        else
            wallMats = new List<string>(biomeOptions.wallMaterials.Length != 0 ? biomeOptions.wallMaterials : defaultBiomeOptions.wallMaterials);

        wallMaterials = new Material[wallMats.Count];
        for (int x = 0; x < wallMats.Count; x++)
            wallMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + wallMats[x]);

        CityGenerator.TrimToRandomSubset(wallMats, Random.Range(3, 6));

        //Floor materials--------------------------------------------------------------------------------------------------------------
        List<string> floorMats;
        if (cityType.floorMaterials != null && cityType.floorMaterials.Length != 0)
            floorMats = new List<string>(cityType.floorMaterials);
        else
            floorMats = new List<string>(biomeOptions.floorMaterials.Length != 0 ? biomeOptions.floorMaterials : defaultBiomeOptions.floorMaterials);

        CityGenerator.TrimToRandomSubset(floorMats, Random.Range(2, 5));
        floorMaterials = new Material[floorMats.Count];
        for (int x = 0; x < floorMats.Count; x++)
            floorMaterials[x] = Resources.Load<Material>("Planet/City/Materials/" + floorMats[x]);

        //Slab materials--------------------------------------------------------------------------------------------------------------
        List<string> slabMats = new List<string>(biomeOptions.slabMaterials.Length != 0 ? biomeOptions.slabMaterials : defaultBiomeOptions.slabMaterials);
        slabMaterial = Resources.Load<Material>("Planet/City/Materials/" + slabMats[Random.Range(0, slabMats.Count)]);

        //Ground materials--------------------------------------------------------------------------------------------------------------
        List<string> groundMats = new List<string>(biomeOptions.groundMaterials.Length != 0 ? biomeOptions.groundMaterials : defaultBiomeOptions.groundMaterials);
        groundMaterial = Resources.Load<Material>("Planet/City/Materials/" + groundMats[Random.Range(0, groundMats.Count)]);
    }

    private CityType SelectCityType()
    {
        CityType[] cityTypes = CityGenerator.generator.cityTypes;
        string biome = Planet.planet.biome.ToString().ToLower();

        float totalProbability = 0.0f;
        List<int> matches = new List<int>();

        //Determine possible city types based off biome
        for (int typeIndex = 0; typeIndex < cityTypes.Length; typeIndex++)
        {
            bool match = false;

            //Determine if biome matches city type
            if (cityTypes[typeIndex].biomes == null || cityTypes[typeIndex].biomes.Length == 0)
                match = true;
            else
                for (int biomeIndex = 0; biomeIndex < cityTypes[typeIndex].biomes.Length; biomeIndex++)
                {
                    if (biome.Equals(cityTypes[typeIndex].biomes[biomeIndex].ToLower()))
                    {
                        match = true;
                        break;
                    }
                }

            //If so, add city type to list of possible city types
            if (match)
            {
                totalProbability += cityTypes[typeIndex].spawnChance;
                matches.Add(typeIndex);
            }
        }

        //Make selection based off spawn chance for each matching city type
        if (totalProbability == 0 || matches.Count == 0)
            return cityTypes[0];
        else
        {
            float selection = Random.Range(0.0f, totalProbability);
            float passed = 0.0f;
            CityType cityType = null;

            for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
            {
                cityType = cityTypes[matches[matchIndex]];

                if (selection >= passed && selection < passed + cityType.spawnChance)
                    break;

                passed += cityType.spawnChance;
            }

            return cityType;
        }
    }

}



[System.Serializable]
public class PlanetCityCustomizationJSON
{
    //City type
    public int cityTypeIndex;

    //Materials
    public string[] wallMaterials, floorMaterials; //Used for buildings and city walls
    public string slabMaterial, groundMaterial; //Used for foundations

    public PlanetCityCustomizationJSON(PlanetCityCustomization planetCityCustomization)
    {
        for (int x = 0; x < CityGenerator.generator.cityTypes.Length; x++)
        {
            if (CityGenerator.generator.cityTypes[x] == planetCityCustomization.cityType)
            {
                cityTypeIndex = x;
                break;
            }
        }

        wallMaterials = new string[planetCityCustomization.wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            wallMaterials[x] = planetCityCustomization.wallMaterials[x].name;

        floorMaterials = new string[planetCityCustomization.floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            floorMaterials[x] = planetCityCustomization.floorMaterials[x].name;

        slabMaterial = planetCityCustomization.slabMaterial.name;
        groundMaterial = planetCityCustomization.groundMaterial.name;
    }

    public void RestorePlanetCityCustomization(PlanetCityCustomization planetCityCustomization)
    {
        planetCityCustomization.cityType = CityGenerator.generator.cityTypes[cityTypeIndex];

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