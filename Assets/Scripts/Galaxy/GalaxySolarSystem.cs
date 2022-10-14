using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySolarSystem : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, LabelOverride("Star"), Tooltip("The script component of the star that is at the center of the solar system. Specified through the inspector.")] private GalaxyStar starVar = null;
    [SerializeField] private Sprite sprite = null;

    //Non-inspector variables and properties.

    /// <summary>
    /// Public property that should be used to access the script of the star that is at the center of the solar system.
    /// </summary>
    public GalaxyStar star { get => starVar; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(IsFullyWithinImage());
    }

    public bool IsFullyWithinImage()
    {
        Vector2Int[] coordinatesToCheck = new Vector2Int[4];
        coordinatesToCheck[0] = new Vector2Int(((int)transform.localPosition.x + 1920) - (20 / 2), 1080 - (int)transform.localPosition.z);
        coordinatesToCheck[1] = new Vector2Int(((int)transform.localPosition.x + 1920), (1080 - (int)transform.localPosition.z) - (20 / 2));
        coordinatesToCheck[2] = new Vector2Int(((int)transform.localPosition.x + 1920) + (20 / 2), 1080 - (int)transform.localPosition.z);
        coordinatesToCheck[3] = new Vector2Int(((int)transform.localPosition.x + 1920), (1080 - (int)transform.localPosition.z) + (20 / 2));
        for (int index = 0; index < coordinatesToCheck.Length; index++)
        {
            if (coordinatesToCheck[index].x < 0 || coordinatesToCheck[index].x >= 3840 || coordinatesToCheck[index].y < 0 || coordinatesToCheck[index].y >= 2160 || sprite.texture.GetPixel(coordinatesToCheck[index].x, coordinatesToCheck[index].y).a == 0)
                return false;
        }
        return true;
    }
}
