using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * When adding a new material type, you must do the following:
 * 1. Add the material to the PlanetMaterialType enum.
 * 2. Update the function PlanetMaterial.GetMaterialName, supplying the name you wish to pair with this enum value.
 * 3. Add the material sound files to "Assets/Resources/Planet/Environment/(Footsteps or Impacts)" using the same name you used with step 2.
 * 4. Pair the material to the stuff in the game that should have it. How you do this depends on whether it is used for terrain or game objects.
 * 4A. For game objects, just slap the PlanetMaterial component on them and select that correct value in the dropdown.
 * 4B. For terrain, you'll have to update the PlanetGenerator class with the correct logic. See how it is done with previous materials.
 * 
 * Optionally, you can add the material to:
 * *PlanetMaterial.GetMaterialTypeBasedOnName() //Objects skinned at runtime have to use their Renderer's material name to determine their PlanetMaterialType.
 * *PlanetMaterial.IsFlammable() //Determines which materials can catch on fire.
 * */

public enum PlanetMaterialType : int {
    NoMaterial = 0,
    Rock = 1,
    Water = 2,
    Swimming = 3,
    Ice = 4,
    Metal = 5,
    Wood = 6,
    Grass = 7,
    Sand = 8,
    Snow = 9,
    Swamp = 10,
    MartianDirt = 11,
    HollowMetal = 12,
    Glass = 13
}

public enum PlanetMaterialInteractionType { Walking, Running, MediumImpact, HardImpact }

public class PlanetMaterial : MonoBehaviour
{
    public PlanetMaterialType planetMaterialType = PlanetMaterialType.Rock;

    public static AudioClip GetMaterialAudio(PlanetMaterialType planetMaterialType, PlanetMaterialInteractionType interactionType)
    {
        GetMaterialPathParts(interactionType, out PlanetMaterialPathParts pathParts);
        string resourcePath = pathParts.prefix + GetMaterialName(planetMaterialType) + pathParts.suffix;

        return Resources.Load<AudioClip>(resourcePath);
    }

    public static bool IsFlammable(PlanetMaterialType planetMaterialType)
    {
        switch (planetMaterialType)
        {
            case PlanetMaterialType.Wood: return true;
            default: return false;
        }
    }

    private static string GetMaterialName(PlanetMaterialType planetMaterialType)
    {
        switch(planetMaterialType)
        {
            case PlanetMaterialType.Rock: return "Rock";
            case PlanetMaterialType.Water: return "Water";
            case PlanetMaterialType.Swimming: return "Swimming";
            case PlanetMaterialType.Ice: return "Ice";
            case PlanetMaterialType.HollowMetal: return "Hollow Metal";
            case PlanetMaterialType.Metal: return "Metal";
            case PlanetMaterialType.Wood: return "Wood";
            case PlanetMaterialType.Grass: return "Grass";
            case PlanetMaterialType.Sand: return "Sand";
            case PlanetMaterialType.Snow: return "Snow";
            case PlanetMaterialType.Swamp: return "Swamp";
            case PlanetMaterialType.MartianDirt: return "Martian Dirt";
            case PlanetMaterialType.Glass: return "Glass";
            default: return "Rock";
        }
    }

    private static void GetMaterialPathParts(PlanetMaterialInteractionType interactionType, out PlanetMaterialPathParts pathParts)
    {
        switch(interactionType)
        {
            case PlanetMaterialInteractionType.Running:
                pathParts = new PlanetMaterialPathParts("Planet/Environment/Footsteps/", " Running");
                break;
            case PlanetMaterialInteractionType.Walking:
                pathParts = new PlanetMaterialPathParts("Planet/Environment/Footsteps/", " Walking");
                break;
            case PlanetMaterialInteractionType.MediumImpact:
                pathParts = new PlanetMaterialPathParts("Planet/Environment/Impacts/", " Medium Impact");
                break;
            default:
                pathParts = new PlanetMaterialPathParts("Planet/Environment/Impacts/", " Hard Impact");
                break;
        }
    }

    private class PlanetMaterialPathParts
    {
        public string prefix, suffix;

        public PlanetMaterialPathParts(string prefix, string suffix)
        {
            this.prefix = prefix;
            this.suffix = suffix;
        }
    }

    public static void SetMaterialTypeBasedOnName(string materialName, GameObject objectWithMaterial)
    {
        PlanetMaterialType materialType = GetMaterialTypeBasedOnName(materialName);

        if (materialType == PlanetMaterialType.Rock)
            return;

        PlanetMaterial planetMaterialComponent = objectWithMaterial.AddComponent<PlanetMaterial>();
        planetMaterialComponent.planetMaterialType = materialType;
    }

    private static PlanetMaterialType GetMaterialTypeBasedOnName(string materialName)
    {
        PlanetMaterialType[] materialsToLookFor = new PlanetMaterialType[] { PlanetMaterialType.Wood, PlanetMaterialType.HollowMetal, PlanetMaterialType.Metal };

        for(int x = 0; x < materialsToLookFor.Length; x++)
        {
            if (materialName.Contains(GetMaterialName(materialsToLookFor[x])))
                return materialsToLookFor[x];
        }

        return PlanetMaterialType.Rock;
    }

    public static PlanetMaterialType GetMaterialFromTransform(Transform transformWithMaterial, Vector3 atPosition)
    {
        if (transformWithMaterial.CompareTag("Terrain")) //Walking on terrain
        {
            int newTerrainTextureIndex = PlanetTerrain.planetTerrain.GetTextureIndexAtPoint(atPosition);

            if (newTerrainTextureIndex == 0) //Ground (so grass, snow, sand, mud, etc)
                return Planet.planet.groundMaterial;
            else if (newTerrainTextureIndex == 1) //Cliff (rock)
                return PlanetMaterialType.Rock;
            else //Seabed
                return Planet.planet.seabedMaterial;
        }
        else if (GroundIsPartOfHorizon(transformWithMaterial)) //Walking on horizon
        {
            if (Planet.planet.GetComponent<PlanetTerrain>().customization.lowBoundaries)
                return Planet.planet.seabedMaterial;
            else
                return Planet.planet.groundMaterial;
        }
        else //Walking on something else
        {
            PlanetMaterial customMaterialTag = transformWithMaterial.GetComponent<PlanetMaterial>();
            return customMaterialTag ? customMaterialTag.planetMaterialType : PlanetMaterialType.Rock;
        }
    }

    private static bool GroundIsPartOfHorizon(Transform ground)
    {
        if (!ground)
            return false;
        else if (ground.name.Equals("Planet Horizon"))
            return true;
        else
            return GroundIsPartOfHorizon(ground.parent);
    }
}
