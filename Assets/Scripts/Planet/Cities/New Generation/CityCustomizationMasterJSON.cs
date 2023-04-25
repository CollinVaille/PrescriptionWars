using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityCustomizationMasterJSON
{
    public BiomeMatchOptionJSON[] cityTypes;
    public BiomeMatchOptionJSON[] biomeCityOptions;


    //---


    public static void GetDataFromJSONFiles(out CityTypeJSON cityType, out BiomeCityOptionsJSON biomeCityOptions)
    {
        string cityMasterCustomizationAsString = GeneralHelperMethods.GetTextAsset("Planet/City/City Customization Master File", startPathFromGeneralTextFolder: false).text;
        CityCustomizationMasterJSON cityCustomizationMaster = JsonUtility.FromJson<CityCustomizationMasterJSON>(cityMasterCustomizationAsString);

        string biomeName = Planet.planet.biome.ToString().ToLower();
        string subBiomeName = Planet.planet.subBiome;

        cityType = cityCustomizationMaster.GetCityType(biomeName, subBiomeName);
        biomeCityOptions = cityCustomizationMaster.GetBiomeCityOptions(biomeName, subBiomeName);
    }

    private CityTypeJSON GetCityType(string biomeName, string subBiomeName)
    {
        BiomeMatchOptionJSON cityTypeMatch = BiomeMatchOptionJSON.GetBestMatch(biomeName, subBiomeName, cityTypes, cityTypes[0]);

        string cityTypePath = CityTypeJSON.GetResourcePathPrefix(true, cityTypeMatch.matchName) + cityTypeMatch.matchName + " City Type";
        string cityTypeJSONAsString = GeneralHelperMethods.GetTextAsset(cityTypePath, startPathFromGeneralTextFolder: false).text;
        return JsonUtility.FromJson<CityTypeJSON>(cityTypeJSONAsString);
    }

    private BiomeCityOptionsJSON GetBiomeCityOptions(string biomeName, string subBiomeName)
    {
        BiomeMatchOptionJSON biomeCityOptionsMatch = BiomeMatchOptionJSON.GetBestMatch(biomeName, subBiomeName, biomeCityOptions, biomeCityOptions[0]);

        string biomeCityOptionsPath = "Planet/City/Biome City Options/" + biomeCityOptionsMatch.matchName;
        string biomeCityOptionsAsString = GeneralHelperMethods.GetTextAsset(biomeCityOptionsPath, startPathFromGeneralTextFolder: false).text;
        return JsonUtility.FromJson<BiomeCityOptionsJSON>(biomeCityOptionsAsString);
    }
}
