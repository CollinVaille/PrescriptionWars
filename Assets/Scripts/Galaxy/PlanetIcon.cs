using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIcon : MonoBehaviour
{
    public Text nameLabel;

    public Planet.Biome biome;

    public void InitializePlanet (string planetName)
    {
        AddNameLabel(planetName);
    }

    private void AddNameLabel (string planetName)
    {
        //Create label
        nameLabel = new GameObject(name + " Label").AddComponent<Text>();

        //Make it a child of canvas
        nameLabel.transform.SetParent(GameObject.Find("Canvas").transform, false);

        //Add it to UI layer
        nameLabel.gameObject.layer = 5;

        //Set gameobject name
        name = planetName;

        //Set text
        nameLabel.text = planetName;

        //Set font
        nameLabel.font = GalaxyMenu.galaxyMenu.planetNameFont;

        //Set font size
        nameLabel.fontSize = 20;

        //Long names will be invisible without this
        nameLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        nameLabel.verticalOverflow = VerticalWrapMode.Overflow;

        //Center text below planet
        nameLabel.alignment = TextAnchor.LowerCenter;
    }

    private void Update ()
    {
        //Update position of name label
        if(nameLabel)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            nameLabel.transform.position = screenPosition;
        }
    }

    private void OnMouseUp ()
    {
        Debug.Log("Clicked on " + name);
    }
}
