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
        if (Random.Range(0, 90) < city.radius && cityType.wallSections.Length > 0)
        {
            //Wall section
            string wall = cityType.wallSections[Random.Range(0, cityType.wallSections.Length)];
            city.wallSectionPrefab = Resources.Load<GameObject>("City/Wall Sections/" + wall);

            //Horizontal Gate
            string gate = cityType.gates[Random.Range(0, cityType.gates.Length)];
            city.horGatePrefab = Resources.Load<GameObject>("City/Gates/" + gate);

            //Vertical gate
            gate = cityType.gates[Random.Range(0, cityType.gates.Length)];
            city.verGatePrefab = Resources.Load<GameObject>("City/Gates/" + gate);

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

    //Not guaranteed to be unique
    public static string GenerateCityName(Planet.Biome biome, int radius)
    {
        string cityName = "";

        if (radius < 60) //Station
        {
            List<string> stationSuffixes = new List<string>(18){" Station", " Outpost", " Installation", " Base", " Point"};

            if (biome == Planet.Biome.Temperate)
            {
                stationSuffixes.Add(" Village");
                stationSuffixes.Add(" Lookout");
                stationSuffixes.Add(" Holdout");
                stationSuffixes.Add(" Camp");
                stationSuffixes.Add(" Settlement");
                stationSuffixes.Add(" Retreat");
                stationSuffixes.Add(" Post");

                //Get list of prefixes
                string[] prefixes = GeneralHelperMethods.GetLinesFromFile("Location Names/Village Name Prefixes");

                //Get list of suffixes
                string[] suffixes = GeneralHelperMethods.GetLinesFromFile("Location Names/Village Name Suffixes");

                //Determine prefix and suffix
                string suffix = suffixes[Random.Range(0, suffixes.Length)];
                string prefix = prefixes[Random.Range(0, prefixes.Length)];
                if (Random.Range(0, 2) == 0 || prefix.Contains("'"))
                    suffix = " " + suffix;
                else
                    suffix = suffix.ToLower();

                //Put them together
                cityName = prefix + suffix;
            }
            else if(biome == Planet.Biome.Swamp)
            {
                stationSuffixes.Add(" Village");
                stationSuffixes.Add(" Ruins");

                string[] babble = new string[] { "Wagga", "DiDi", "Uuka", "Wooka", "Yauli", "Uga", "Yadda", "Waka",
                "Tata", "Zidi", "Eika", "Wi", "Jawa"};

                //Create name that's a random mash up of babble
                cityName = babble[Random.Range(0, babble.Length)];
                for (int x = 1; x <= Random.Range(2, 4); x++)
                {
                    if (Random.Range(0, 2) == 0)
                        cityName += " " + babble[Random.Range(0, babble.Length)];
                    else
                        cityName += babble[Random.Range(0, babble.Length)].ToLower();
                }
            }
            else if (biome == Planet.Biome.Frozen)
            {
                stationSuffixes.Add(" Ruins");
            }
            else if(biome == Planet.Biome.Desert)
            {
                stationSuffixes.Add(" Spaceport");
                stationSuffixes.Add(" Cosmodrome");
            }
            else if(biome == Planet.Biome.Hell)
            {
                stationSuffixes.Add(" Juncture");
                stationSuffixes.Add(" Firebase");
                stationSuffixes.Add(" Hell Hole");
                stationSuffixes.Add(" Compound");
                stationSuffixes.Add(" Complex");

                string[] lexicons = new string[] { "Xar", "Zeta", "Yax", "Ra", "Tarn", "Rhol", "Psii", "Zaao", "Xu", "Kaarn",
                "Har", "Fax", "Qar"};

                //Create name that's a random mash up of babble
                cityName = lexicons[Random.Range(0, lexicons.Length)];
                if(Random.Range(0, 2) == 0)
                {
                    if (Random.Range(0, 3) == 0)
                        cityName += " " + lexicons[Random.Range(0, lexicons.Length)];
                    else if(Random.Range(0, 2) == 0)
                        cityName += "-" + lexicons[Random.Range(0, lexicons.Length)];
                    else
                        cityName += lexicons[Random.Range(0, lexicons.Length)].ToLower();
                }
            }

            //Default station names are defined in military station names file
            if (cityName.Equals(""))
                cityName = GeneralHelperMethods.GetLineFromFile("Location Names/Military Station Names");

            //Finish the city name with a suffix indicating it's not a major city
            cityName += stationSuffixes[Random.Range(0, stationSuffixes.Count)];
        }
        else //Major city
        {
            if (biome == Planet.Biome.Temperate)
            {
                if(Random.Range(0, 3) == 0)
                {
                    string[] part1 = new string[] { "East", "West", "North", "South", "White", "Gray", "Pale",
                    "Black", "Mourn", "Hjaal", "Grey", "Frost", "Way", "Storm", "Baren", "Falk" };

                    string[] part2 = new string[] { "march", "reach", "hold", "rest", "haven", "fold", "garden",
                    "fingar", "run", "port", " Seed", " Harbour", " Solace" };

                    cityName = part1[Random.Range(0, part1.Length)] + part2[Random.Range(0, part2.Length)];
                }
            }
            else if (biome == Planet.Biome.Swamp)
            {
                if (Random.Range(0, 2) == 0)
                    cityName = GeneralHelperMethods.GetLineFromFile("Location Names/Swamp Ass City Names");
                else
                {
                    string[] part1 = new string[] { "Eika", "Weigga", "Gieiga", "Eeiita", "Weiika", "Yykieka", "Wakka-waka" };

                    string[] part2 = new string[] { " Eiiga", " Weika", " Ooka", " Ahga", " Eiita", " Yiekah", " Yah",
                        " Yugha", " Lakha", " Tata", " Xita", " Uiyga" };

                    cityName = part1[Random.Range(0, part1.Length)] + part2[Random.Range(0, part2.Length)];
                }
            }
            else if(biome == Planet.Biome.Hell)
                cityName = GeneralHelperMethods.GetLineFromFile("Location Names/Hellish City Names");
            else if(biome == Planet.Biome.Spirit)
            {
                string[] part1 = new string[] { "Staavan", "Volks", "Korvan", "Weyro", "Teyro", "Vail", "Rhen",
                    "Bhor", "Vel", "Galto", "Vogh", "Mons", "Forel" };

                string[] part2 = new string[] { "gar", "gaard", "var", "boro", "baro", " Koros", "kura",
                    "brunnr", "kyyge", "kuldhir", "touhm", "thume", "heiligen", "semane" };

                cityName = part1[Random.Range(0, part1.Length)] + part2[Random.Range(0, part2.Length)];
            }

            //Default city names are defined in the city names file
            if (cityName.Equals(""))
                cityName = GeneralHelperMethods.GetLineFromFile("Location Names/City Names");
        }

        return cityName;
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
