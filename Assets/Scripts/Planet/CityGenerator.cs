using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public static CityGenerator generator;

    public BiomeMaterials[] biomeMaterials;
    public CityType[] cityTypes;

    private void Awake() { generator = this; }

    public void CustomizeCity(City city)
    {
        CityType cityType = SelectCityType();

        //Buildings
        List<string> buildings = new List<string>(cityType.buildings);
        TrimToRandomSubset(buildings, Random.Range(5, 10));
        city.buildingPrefabs = new GameObject[buildings.Count];
        for (int x = 0; x < buildings.Count; x++)
            city.buildingPrefabs[x] = Resources.Load<GameObject>("City/Buildings/" + buildings[x]);

        //Wall materials
        List<string> wallMats;
        if (cityType.wallMaterials != null && cityType.wallMaterials.Length != 0)
            wallMats = new List<string>(cityType.wallMaterials);
        else
            wallMats = new List<string>(GetBiomeMaterials().wallMaterials);

        TrimToRandomSubset(wallMats, Random.Range(3, 6));
        city.wallMaterials = new Material[wallMats.Count];
        for (int x = 0; x < wallMats.Count; x++)
            city.wallMaterials[x] = Resources.Load<Material>("City/Building Materials/" + wallMats[x]);

        //Floor materials
        List<string> floorMats;
        if (cityType.floorMaterials != null && cityType.floorMaterials.Length != 0)
            floorMats = new List<string>(cityType.floorMaterials);
        else
            floorMats = new List<string>(GetBiomeMaterials().floorMaterials);

        TrimToRandomSubset(floorMats, Random.Range(2, 5));
        city.floorMaterials = new Material[floorMats.Count];
        for (int x = 0; x < floorMats.Count; x++)
            city.floorMaterials[x] = Resources.Load<Material>("City/Building Materials/" + floorMats[x]);

        //Customize walls
        if (cityType.wallChance >= Random.Range(0, 1.0f))
        {
            //Wall section
            string wall = cityType.wallSections[Random.Range(0, cityType.wallSections.Length)];
            city.wallSectionPrefab = Resources.Load<GameObject>("City/Wall Sections/" + wall);

            //Gate
            string gate = cityType.gates[Random.Range(0, cityType.gates.Length)];
            city.gatePrefab = Resources.Load<GameObject>("City/Gates/" + gate);

            //Fence posts
            if (cityType.fencePostChance >= Random.Range(0, 1.0f))
            {
                string fencePost = cityType.fencePosts[Random.Range(0, cityType.fencePosts.Length)];
                city.fencePostPrefab = Resources.Load<GameObject>("City/Fence Posts/" + fencePost);
            }
        }
    }

    private CityType SelectCityType()
    {
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

    private void TrimToRandomSubset(List<string> toTrim, int subsetSize)
    {
        while (toTrim.Count > subsetSize)
            toTrim.RemoveAt(Random.Range(0, toTrim.Count));
    }

    private BiomeMaterials GetBiomeMaterials()
    {
        BiomeMaterials toReturn = biomeMaterials[0];
        string biome = Planet.planet.biome.ToString().ToLower();

        for (int x = 0; x < biomeMaterials.Length; x++)
        {
            if (biome.Equals(biomeMaterials[x].name.ToLower()))
            {
                toReturn = biomeMaterials[x];
                break;
            }
        }

        return toReturn;
    }
}


[System.Serializable]
public class BiomeMaterials
{
    public string name = "New Biome Materials"; //Name of the biome these building materials are for
    public string[] wallMaterials;
    public string[] floorMaterials;
}

[System.Serializable]
public class CityType
{
    public string name = "New City Type";
    public float spawnChance = 1.0f; //Higher the number, the more likely to spawn, no limit

    //Biomes
    public string[] biomes; //Leave unitialized to indicate all

    //Buildings
    public string[] buildings;

    //Walls
    public float wallChance = 0.5f; //0 = 0% spawn chance, 1 = 100% spawn chance
    public string[] wallSections;

    //Gates
    public string[] gates;

    //Fence Posts
    public float fencePostChance = 0.5f; //0 = 0% spawn chance, 1 = 100% spawn chance
    public string[] fencePosts;

    //Building materials
    public string[] wallMaterials;
    public string[] floorMaterials;
}
