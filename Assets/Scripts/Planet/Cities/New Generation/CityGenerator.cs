using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public static CityGenerator generator;

    public BiomeCityOptions[] biomeCityOptions;
    public CityType[] cityTypes;
    public FoundationOptions foundationOptions;

    private void Awake() { generator = this; }

    public void CustomizeCity(City city)
    {
        NewCitySpecifications newCitySpecifications = city.newCitySpecifications;

        //Get city type
        CityType cityType = Planet.planet.planetWideCityCustomization.cityType;
        string cityTypePathSuffix = cityType.name + "/";

        //Get biome-city options
        BiomeCityOptions biomeOptions = GetBiomeCityOptions();

        //Buildings--------------------------------------------------------------------------------------------------------------
        string buildingPath = "Planet/City/Buildings/" + cityTypePathSuffix;
        List<string> genericBuildings = new List<string>(cityType.buildings);
        TrimToRandomSubset(genericBuildings, newCitySpecifications.smallCompound ? Random.Range(3, 5) : Random.Range(5, 10));
        city.buildingManager.buildingPrefabs = new List<GameObject>();
        for (int x = 0; x < genericBuildings.Count; x++)
            city.buildingManager.buildingPrefabs.Add(Resources.Load<GameObject>(buildingPath + genericBuildings[x]));

        //Special buildings--------------------------------------------------------------------------------------------------------------
        if(newCitySpecifications.smallCompound)
        {
            if(!string.IsNullOrEmpty(newCitySpecifications.compoundMainBuilding))
                AddSpecialBuilding(buildingPath + newCitySpecifications.compoundMainBuilding, city);
        }
        else
        {
            AddSpecialBuilding(buildingPath + "Prescriptor", city);
            AddSpecialBuilding(buildingPath + "Depot", city);
            AddSpecialBuilding(buildingPath + "Research Facility", city);
            AddSpecialBuilding(buildingPath + "Trade Post", city);
        }

        //Default materials--------------------------------------------------------------------------------------------------------------
        city.buildingManager.SetDefaultMaterials();

        //Customize walls--------------------------------------------------------------------------------------------------------------
        if (city.radius > Random.Range(130, 150) && cityType.wallSections.Length > 0 && !newCitySpecifications.tryToMakeEasyAccessToTerrain)
        {
            //Wall section
            string wall = cityType.wallSections[Random.Range(0, cityType.wallSections.Length)];
            city.cityWallManager.wallSectionPrefab = Resources.Load<GameObject>("Planet/City/Wall Sections/" + cityTypePathSuffix + wall);

            //Horizontal Gate
            string gate = cityType.gates[Random.Range(0, cityType.gates.Length)];
            city.cityWallManager.horGatePrefab = Resources.Load<GameObject>("Planet/City/Gates/" + cityTypePathSuffix + gate);

            //Vertical gate
            gate = cityType.gates[Random.Range(0, cityType.gates.Length)];
            city.cityWallManager.verGatePrefab = Resources.Load<GameObject>("Planet/City/Gates/" + cityTypePathSuffix + gate);

            //Fence posts
            if (cityType.fencePostChance >= Random.Range(0, 1.0f))
            {
                string fencePost = cityType.fencePosts[Random.Range(0, cityType.fencePosts.Length)];
                city.cityWallManager.fencePostPrefab = Resources.Load<GameObject>("Planet/City/Fence Posts/" + cityTypePathSuffix + fencePost);
            }
        }

        //City shape--------------------------------------------------------------------------------------------------------------
        if (city.cityWallManager.fencePostPrefab) //Need fence posts to hide the seams between wall sections when the walls are circular
            city.circularCity = Random.Range(0.0f, 1.0f / cityType.fencePostChance) > 0.5f;

        //Foundations--------------------------------------------------------------------------------------------------------------
        if (biomeOptions.foundationChance > Random.Range(0.0f, 1.0f) && !newCitySpecifications.tryToMakeEasyAccessToTerrain) //Chance for non-zero foundation height (and thus the presence of foundations)
        {
            if(Random.Range(0, 3) != 0)
                city.foundationManager.foundationHeight = (int)Random.Range(biomeOptions.foundationHeightRange.x, biomeOptions.foundationHeightRange.y);
            else
                city.foundationManager.foundationHeight = (int)Random.Range(biomeOptions.foundationHeightRange.z, biomeOptions.foundationHeightRange.w);
        }
        else //Zero foundation height (and thus zero foundations). This could be overruled later if the city needs foundations to be above water
            city.foundationManager.foundationHeight = 0;

        //Bridges--------------------------------------------------------------------------------------------------------------
        city.bridgeManager.bridgePrefabPaths = new List<string>(cityType.bridges);
        TrimToRandomSubset(city.bridgeManager.bridgePrefabPaths, Random.Range(1, 4));
        city.bridgeManager.bridgePrefabPaths.Add("Special Connector");

        //Lights--------------------------------------------------------------------------------------------------------------
        string cityLight = cityType.lights[Random.Range(0, cityType.lights.Length)];
        city.cityLightManager.cityLightPrefab = Resources.Load<GameObject>("Planet/City/Lights/" + cityTypePathSuffix + cityLight);

    }

    public static void TrimToRandomSubset(List<string> toTrim, int subsetSize)
    {
        while (toTrim.Count > subsetSize)
            toTrim.RemoveAt(Random.Range(0, toTrim.Count));
    }

    public BiomeCityOptions GetBiomeCityOptions()
    {
        BiomeCityOptions toReturn = biomeCityOptions[0];
        Planet.Biome biome = Planet.planet.biome;

        for (int x = 0; x < biomeCityOptions.Length; x++)
        {
            if (biome == biomeCityOptions[x].biome)
            {
                toReturn = biomeCityOptions[x];
                break;
            }
        }

        return toReturn;
    }

    private void AddSpecialBuilding(string specialBuildingPathName, City city)
    {
        GameObject specialBuildingPrefab = Resources.Load<GameObject>(specialBuildingPathName);
        if(specialBuildingPrefab)
            city.buildingManager.buildingPrefabs.Add(specialBuildingPrefab);
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
