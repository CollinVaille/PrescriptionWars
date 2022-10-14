using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSphere : MonoBehaviour
{
    [SerializeField] private Sprite sprite = null;

    public bool isFullyWithinImage
    {
        get
        {
            Vector2Int[] coordinatesToCheck = new Vector2Int[4];
            coordinatesToCheck[0] = new Vector2Int(((int)transform.localPosition.x + 1920) - ((int)transform.localScale.x / 2), 1080 - (int)transform.localPosition.z);
            coordinatesToCheck[1] = new Vector2Int(((int)transform.localPosition.x + 1920), (1080 - (int)transform.localPosition.z) - ((int)transform.localScale.z / 2));
            coordinatesToCheck[2] = new Vector2Int(((int)transform.localPosition.x + 1920) + ((int)transform.localScale.x / 2), 1080 - (int)transform.localPosition.z);
            coordinatesToCheck[3] = new Vector2Int(((int)transform.localPosition.x + 1920), (1080 - (int)transform.localPosition.z) + ((int)transform.localScale.z / 2));
            for(int index = 0; index < coordinatesToCheck.Length; index++)
            {
                if (coordinatesToCheck[index].x < 0 || coordinatesToCheck[index].x >= 3840 || coordinatesToCheck[index].y < 0 || coordinatesToCheck[index].y >= 2160 || sprite.texture.GetPixel(coordinatesToCheck[index].x, coordinatesToCheck[index].y).a == 0)
                    return false;
            }
            return true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(isFullyWithinImage);
    }
}
