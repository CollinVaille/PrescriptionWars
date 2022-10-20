using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyPlanet : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The game object that has the atmosphere renderer applied to it.")] private GameObject atmosphere = null;
    [SerializeField, Tooltip("The game object that has the rings renderer applied to it.")] private GameObject rings = null;

    //Non-inspector variables.

    /// <summary>
    /// Private variable that holds what type of biome the planet belongs to.
    /// </summary>
    private Planet.Biome biomeTypeVar = Planet.Biome.Unknown;
    /// <summary>
    /// Public property that should be used to acces the type of biome that the planet belongs to.
    /// </summary>
    public Planet.Biome biomeType { get => biomeTypeVar; }

    /// <summary>
    /// Private variable that holds the name of the material applied to the planet.
    /// </summary>
    private string materialNameVar = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the material that is applied to the planet.
    /// </summary>
    public string materialName
    {
        get => materialNameVar;
        set
        {
            GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + materialName);
            atmosphere.GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + materialName + " Atmosphere");
            materialNameVar = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method is called by the GenerateSolarSystems method in the galaxy generator and initialized all needed variables for the galaxy planet.
    /// </summary>
    public void InitializeFromGalaxyGenerator(Planet.Biome biomeType, string materialName, bool hasRing, float ringSize, float planetarySize, float cloudSpeed, DualColorSet cloudColorCombo, Color cityColor, DualColorSet ringColorCombo, Light starLight)
    {
        //Initializes the biome type.
        biomeTypeVar = biomeType;
        //Initializes the material of the planet.
        this.materialName = materialName;
        //Initializes whether the planet has ctive rings or not.
        rings.SetActive(hasRing);
        //Initializes the size of the planet's rings.
        rings.GetComponent<Renderer>().material.SetFloat("_Size", ringSize);
        //Initializes the size of the planet.
        transform.parent.localScale = Vector3.one * planetarySize;
        //Initializes the speed of the clouds on the planet.
        GetComponent<Renderer>().material.SetFloat("_CloudSpeed", cloudSpeed);
        //Initializes the color of the planet's clouds and cloud shadow.
        GetComponent<Renderer>().material.SetColor("_ColorA", cloudColorCombo[0]);
        GetComponent<Renderer>().material.SetColor("_ShadowColorA", cloudColorCombo[1]);
        //Initializes the color of the cities on the planet.
        GetComponent<Renderer>().material.SetColor("_Citiescolor", cityColor);
        //Initializes the color of the planet's rings.
        rings.GetComponent<Renderer>().material.SetColor("_BaseColor", ringColorCombo[0]);
        rings.GetComponent<Renderer>().material.SetColor("_Color1", ringColorCombo[1]);
        //Initializes the light source of the planet to the light being emitted from the star in the solar system.
        transform.parent.gameObject.GetComponent<LightSource>().Sun = starLight.gameObject;
    }
}

[System.Serializable]
public class GalaxyPlanetData
{
    public GalaxyPlanetData(NewGalaxyPlanet planet)
    {

    }
}