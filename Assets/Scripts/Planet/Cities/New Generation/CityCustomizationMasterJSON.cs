using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityCustomizationMasterJSON
{
    public BiomeMatchOptionJSON[] cityTypes;
    public BiomeMatchOptionJSON[] cityStylings;


    //---


    public static void GetDataFromJSONFiles(out CityTypeJSON cityType, out CityStylingJSON cityStyling)
    {
        string cityMasterCustomizationAsString = GeneralHelperMethods.GetTextAsset("Planet/City/City Customization Master File", startPathFromGeneralTextFolder: false).text;
        CityCustomizationMasterJSON cityCustomizationMaster = JsonUtility.FromJson<CityCustomizationMasterJSON>(cityMasterCustomizationAsString);

        string biomeName = Planet.planet.biome.ToString().ToLower();
        string subBiomeName = Planet.planet.subBiome;

        cityType = cityCustomizationMaster.GetCityType(biomeName, subBiomeName);

        cityStyling = cityType.styling; //1st choice: Get styling directly from city type
        cityStyling.FillEmptyFieldsWithTheseValues(cityCustomizationMaster.GetCityStyling(biomeName, subBiomeName)); //2nd choice, get styling based on biome and/or sub-biome
        cityStyling.FillEmptyFieldsWithTheseValues(cityCustomizationMaster.GetDefaultStyling()); //Last resort, use default styling
    }

    private CityTypeJSON GetCityType(string biomeName, string subBiomeName)
    {
        BiomeMatchOptionJSON cityTypeMatch = BiomeMatchOptionJSON.GetBestMatch(biomeName, subBiomeName, cityTypes, cityTypes[0]);

        string cityTypePath = CityTypeJSON.GetResourcePathPrefix(true, cityTypeMatch.matchName) + cityTypeMatch.matchName;
        string cityTypeJSONAsString = GeneralHelperMethods.GetTextAsset(cityTypePath, startPathFromGeneralTextFolder: false).text;
        return JsonUtility.FromJson<CityTypeJSON>(cityTypeJSONAsString);
    }

    private CityStylingJSON GetCityStyling(string biomeName, string subBiomeName)
    {
        BiomeMatchOptionJSON cityStylingMatch = BiomeMatchOptionJSON.GetBestMatch(biomeName, subBiomeName, cityStylings, cityStylings[0]);
        return LoadInCityStyling(cityStylingMatch.matchName); 
    }

    private CityStylingJSON GetDefaultStyling()
    {
        return LoadInCityStyling(cityStylings[0].matchName);
    }

    private CityStylingJSON LoadInCityStyling(string stylingName)
    {
        string stylingPath = "Planet/City/City Stylings/" + stylingName;
        string stylingAsString = GeneralHelperMethods.GetTextAsset(stylingPath, startPathFromGeneralTextFolder: false).text;
        return JsonUtility.FromJson<CityStylingJSON>(stylingAsString);
    }
}
