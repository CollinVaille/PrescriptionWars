using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewResourceBar : MonoBehaviour
{
    [Header("Image Components")]

    [SerializeField] private Image flagImage = null;

    // Start is called before the first frame update
    void Start()
    {
        flagImage.sprite = NewGalaxyManager.empires[NewGalaxyManager.playerID].flag.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
