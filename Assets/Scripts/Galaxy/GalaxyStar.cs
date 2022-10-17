using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyStar : MonoBehaviour
{
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
}

[System.Serializable]
public class GalaxyStarData
{
    public float[] localScale = new float[3];

    public GalaxyStarData(GalaxyStar star)
    {
        localScale[0] = star.localScale.x;
        localScale[1] = star.localScale.y;
        localScale[2] = star.localScale.z;
    }
}