using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperspaceLanesManager : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab = null;

    private List<GameObject> hyperspaceLanes = null;

    public enum HyperspaceLaneColoringMode
    {
        EmpireColorGradient,
        Static
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the coloring mode of each empire's interior hyperspace lanes.
    /// </summary>
    public HyperspaceLaneColoringMode interiorHyperspaceLaneColoringMode
    {
        get => GalaxyGameSettings.interiorHyperspaceLaneColoringMode;
        set
        {
            if(GalaxyGameSettings.interiorHyperspaceLaneColoringMode != value)
            {
                GalaxyGameSettings.interiorHyperspaceLaneColoringMode = value;
                return;
            }

            List<GameObject> interiorHyperspaceLanes = this.interiorHyperspaceLanes;
            foreach (GameObject interiorHyperspaceLane in interiorHyperspaceLanes)
            {
                if(value == HyperspaceLaneColoringMode.EmpireColorGradient)
                {
                    interiorHyperspaceLane.GetComponent<Line>().startColor = interiorHyperspaceLane.GetComponent<Line>().gameObjects[0].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
                    interiorHyperspaceLane.GetComponent<Line>().endColor = interiorHyperspaceLane.GetComponent<Line>().gameObjects[1].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
                }
                else if (value == HyperspaceLaneColoringMode.Static)
                {
                    interiorHyperspaceLane.GetComponent<Line>().startColor = interiorHyperspaceLaneStaticColor;
                    interiorHyperspaceLane.GetComponent<Line>().endColor = interiorHyperspaceLaneStaticColor;
                }
            }
        }
    }
    /// <summary>
    /// Public property that should be used both to access and mutate the static color of the empire's interior hyperspace lanes (note: this color may not be visible depending on the coloring mode).
    /// </summary>
    public Color interiorHyperspaceLaneStaticColor
    {
        get => GalaxyGameSettings.interiorHyperspaceLaneStaticColor;
        set
        {
            if (!GalaxyGameSettings.interiorHyperspaceLaneStaticColor.Equals(value))
            {
                GalaxyGameSettings.interiorHyperspaceLaneStaticColor = value;
                return;
            }

            if(interiorHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.Static)
            {
                List<GameObject> interiorHyperspaceLanes = this.interiorHyperspaceLanes;
                foreach (GameObject interiorHyperspaceLane in interiorHyperspaceLanes)
                {
                    interiorHyperspaceLane.GetComponent<Line>().startColor = value;
                    interiorHyperspaceLane.GetComponent<Line>().endColor = value;
                }
            }
        }
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the coloring mode of each empire's border hyperspace lanes.
    /// </summary>
    public HyperspaceLaneColoringMode borderHyperspaceLaneColoringMode
    {
        get => GalaxyGameSettings.borderHyperspaceLaneColoringMode;
        set
        {
            if(GalaxyGameSettings.borderHyperspaceLaneColoringMode != value)
            {
                GalaxyGameSettings.borderHyperspaceLaneColoringMode = value;
                return;
            }

            List<GameObject> borderHyperspaceLanes = this.borderHyperspaceLanes;
            foreach (GameObject borderHyperspaceLane in borderHyperspaceLanes)
            {
                if (value == HyperspaceLaneColoringMode.EmpireColorGradient)
                {
                    borderHyperspaceLane.GetComponent<Line>().startColor = borderHyperspaceLane.GetComponent<Line>().gameObjects[0].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
                    borderHyperspaceLane.GetComponent<Line>().endColor = borderHyperspaceLane.GetComponent<Line>().gameObjects[1].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
                }
                else if (value == HyperspaceLaneColoringMode.Static)
                {
                    borderHyperspaceLane.GetComponent<Line>().startColor = borderHyperspaceLaneStaticColor;
                    borderHyperspaceLane.GetComponent<Line>().endColor = borderHyperspaceLaneStaticColor;
                }
            }
        }
    }
    /// <summary>
    /// Public property that should be used both to access and mutate the static color of the empire's border hyperspace lanes (note: this color may not be visible depending on the coloring mode).
    /// </summary>
    public Color borderHyperspaceLaneStaticColor
    {
        get => GalaxyGameSettings.borderHyperspaceLaneStaticColor;
        set
        {
            if (!GalaxyGameSettings.borderHyperspaceLaneStaticColor.Equals(value))
            {
                GalaxyGameSettings.borderHyperspaceLaneStaticColor = value;
                return;
            }

            if (borderHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.Static)
            {
                List<GameObject> borderHyperspaceLanes = this.borderHyperspaceLanes;
                foreach (GameObject borderHyperspaceLane in borderHyperspaceLanes)
                {
                    borderHyperspaceLane.GetComponent<Line>().startColor = value;
                    borderHyperspaceLane.GetComponent<Line>().endColor = value;
                }
            }
        }
    }

    public static HyperspaceLanesManager hyperspaceLanesManager = null;

    private List<GameObject> interiorHyperspaceLanes
    {
        get
        {
            List<GameObject> interiorHyperspaceLanesList = new List<GameObject>();
            if (hyperspaceLanes == null || hyperspaceLanes.Count == 0)
                return interiorHyperspaceLanesList;
            foreach(GameObject hyperspaceLane in hyperspaceLanes)
            {
                if (hyperspaceLane.GetComponent<Line>().gameObjects.Count == 2 && hyperspaceLane.GetComponent<Line>().gameObjects[0].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner == hyperspaceLane.GetComponent<Line>().gameObjects[1].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner)
                    interiorHyperspaceLanesList.Add(hyperspaceLane);
            }
            return interiorHyperspaceLanesList;
        }
    }

    private List<GameObject> borderHyperspaceLanes
    {
        get
        {
            List<GameObject> borderHyperspaceLanesList = new List<GameObject>();
            if (hyperspaceLanes == null || hyperspaceLanes.Count == 0)
                return borderHyperspaceLanesList;
            foreach (GameObject hyperspaceLane in hyperspaceLanes)
            {
                if (hyperspaceLane.GetComponent<Line>().gameObjects.Count == 2 && hyperspaceLane.GetComponent<Line>().gameObjects[0].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner != hyperspaceLane.GetComponent<Line>().gameObjects[1].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner)
                    borderHyperspaceLanesList.Add(hyperspaceLane);
            }
            return borderHyperspaceLanesList;
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
    /// Public method that should be called in order to add a brand new hyperspace lane to the galaxy (interior or border).
    /// </summary>
    /// <param name="planet1"></param>
    /// <param name="planet2"></param>
    /// <param name="daddy"></param>
    public void AddHyperspaceLane(GameObject planet1, GameObject planet2, Transform daddy)
    {
        if (hyperspaceLanes == null)
            hyperspaceLanes = new List<GameObject>();

        List<string> planetNames = new List<string>();
        planetNames.Add(planet1.name);
        planetNames.Add(planet2.name);
        planetNames.Sort();
        string lineName = planetNames[0] + " - " + planetNames[1];

        bool goodLine = true;
        for (int x = 0; x < hyperspaceLanes.Count; x++)
        {
            if (hyperspaceLanes[x].name.Equals(lineName))
            {
                goodLine = false;
                break;
            }
        }

        if (goodLine)
        {
            //Adds the connected planet to the list of neighbors for both planets.
            planet1.transform.GetChild(0).GetComponent<GalaxyPlanet>().neighborPlanets.Add(planet2.transform.GetChild(0).GetComponent<GalaxyPlanet>().planetID);
            planet2.transform.GetChild(0).GetComponent<GalaxyPlanet>().neighborPlanets.Add(planet1.transform.GetChild(0).GetComponent<GalaxyPlanet>().planetID);

            //Create the hyperspace lane.
            GameObject line = Instantiate(linePrefab);
            line.name = lineName;

            //Interior hyperspace lane coloring.
            if(planet1.transform.GetChild(0).GetComponent<GalaxyPlanet>().owner == planet2.transform.GetChild(0).GetComponent<GalaxyPlanet>().owner)
            {
                if(interiorHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.EmpireColorGradient)
                    line.GetComponent<Line>().Initialize(new List<GameObject>() { planet1, planet2 }, planet1.transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor, planet2.transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor);
                else if (interiorHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.Static)
                    line.GetComponent<Line>().Initialize(new List<GameObject>() { planet1, planet2 }, interiorHyperspaceLaneStaticColor, interiorHyperspaceLaneStaticColor);
            }
            //Border hyperspace lane coloring.
            else
            {
                if(borderHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.EmpireColorGradient)
                    line.GetComponent<Line>().Initialize(new List<GameObject>() { planet1, planet2 }, planet1.transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor, planet2.transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor);
                else if (borderHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.Static)
                    line.GetComponent<Line>().Initialize(new List<GameObject>() { planet1, planet2 }, borderHyperspaceLaneStaticColor, borderHyperspaceLaneStaticColor);
            }

            line.transform.parent = daddy;

            hyperspaceLanes.Add(line);
        }
    }

    /// <summary>
    /// This public method should be called in order to update the coloring of all hyperspace lanes in the galaxy (interior and border hyperspace lanes).
    /// </summary>
    public void UpdateHyperspaceLaneColoring()
    {
        //Interior hyperspace lanes.
        List<GameObject> interiorHyperspaceLanes = this.interiorHyperspaceLanes;
        foreach(GameObject interiorHyperspaceLane in interiorHyperspaceLanes)
        {
            if(interiorHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.EmpireColorGradient)
            {
                interiorHyperspaceLane.GetComponent<Line>().startColor = interiorHyperspaceLane.GetComponent<Line>().gameObjects[0].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
                interiorHyperspaceLane.GetComponent<Line>().endColor = interiorHyperspaceLane.GetComponent<Line>().gameObjects[1].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
            }
            else if (interiorHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.Static)
            {
                interiorHyperspaceLane.GetComponent<Line>().startColor = interiorHyperspaceLaneStaticColor;
                interiorHyperspaceLane.GetComponent<Line>().endColor = interiorHyperspaceLaneStaticColor;
            }
        }

        //Border hyperspace lanes.
        List<GameObject> borderHyperspaceLanes = this.borderHyperspaceLanes;
        foreach (GameObject borderHyperspaceLane in borderHyperspaceLanes)
        {
            if (borderHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.EmpireColorGradient)
            {
                borderHyperspaceLane.GetComponent<Line>().startColor = borderHyperspaceLane.GetComponent<Line>().gameObjects[0].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
                borderHyperspaceLane.GetComponent<Line>().endColor = borderHyperspaceLane.GetComponent<Line>().gameObjects[1].transform.GetChild(0).GetComponent<GalaxyPlanet>().owner.LabelColor;
            }
            else if (borderHyperspaceLaneColoringMode == HyperspaceLaneColoringMode.Static)
            {
                borderHyperspaceLane.GetComponent<Line>().startColor = borderHyperspaceLaneStaticColor;
                borderHyperspaceLane.GetComponent<Line>().endColor = borderHyperspaceLaneStaticColor;
            }
        }
    }
}
