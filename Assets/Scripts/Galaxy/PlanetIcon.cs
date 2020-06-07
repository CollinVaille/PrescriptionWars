using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIcon : MonoBehaviour
{
    public Text nameLabel;

    public Planet.Biome biome;

    public GameObject linePrefab;

    public List<GameObject> hyperspaceLanes;

    Vector3 rotation;

    public void InitializePlanet (string planetName)
    {
        AddNameLabel(planetName);

        //Amount the planet will rotate.
        rotation = new Vector3(0, 0, Random.Range(5, 21));
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

        //Rotates the planet.
        transform.localEulerAngles += rotation * Time.deltaTime;
    }

    public void AddHyperspaceLane(GameObject planet1, GameObject planet2, Transform daddy, int planet1Index, int planet2Index)
    {
        GameObject line = Instantiate(linePrefab);

        line.GetComponent<Line>().gameObject1 = planet1;
        line.GetComponent<Line>().gameObject2 = planet2;

        List<string> planetNames = new List<string>();
        planetNames.Add(planet1.name);
        planetNames.Add(planet2.name);
        planetNames.Sort();
        line.name = planetNames[0] + " - " + planetNames[1];

        bool goodLine = true;
        for (int x = 0; x < hyperspaceLanes.Count; x++)
        {
            string name = hyperspaceLanes[x].name;
            if (name.Equals("" + planetNames[0] + " - " + planetNames[1]))
            {
                Debug.Log("Works");
            }
        }
        if (goodLine)
        {
            line.transform.parent = daddy;
            hyperspaceLanes.Add(line);
            //Debug.Log("Added");
        }
    }

    private void OnMouseUp ()
    {
        Debug.Log("Clicked on " + name);
    }
}
