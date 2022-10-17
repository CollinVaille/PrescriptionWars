using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyStar : MonoBehaviour
{
    public enum StarType
    {
        RedDwarf,
        RedGiant,
        BlueGiant,
        YellowDwarf
    }

    /// <summary>
    /// Private variable that holds the enum value that indicates what type of star this star is.
    /// </summary>
    private StarType typeVar = 0;
    /// <summary>
    /// Public property that should be used in order to access the type of star this star is.
    /// </summary>
    public StarType type { get => typeVar; }

    /// <summary>
    /// Public property that should be used in order to access the local scale of the star.
    /// </summary>
    public Vector3 localScale { get => transform.localScale; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method should be called in the GenerateSolarSystems method in the galaxy generator and initializes all needed variables in the star.
    /// </summary>
    public void InitializeFromGalaxyGenerator(StarType starType)
    {
        typeVar = starType;
    }
}

[System.Serializable]
public class GalaxyStarData
{
    public float[] localScale = new float[3];
    public GalaxyStar.StarType starType = 0;

    public GalaxyStarData(GalaxyStar star)
    {
        localScale[0] = star.localScale.x;
        localScale[1] = star.localScale.y;
        localScale[2] = star.localScale.z;

        starType = star.type;
    }
}