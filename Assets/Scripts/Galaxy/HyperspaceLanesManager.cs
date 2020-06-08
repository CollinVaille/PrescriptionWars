using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperspaceLanesManager : MonoBehaviour
{
    public List<GameObject> hyperspaceLanes;

    public GameObject linePrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddHyperspaceLane(GameObject planet1, GameObject planet2, Transform daddy)
    {
        GameObject line = Instantiate(linePrefab);

        List<string> planetNames = new List<string>();
        planetNames.Add(planet1.name);
        planetNames.Add(planet2.name);
        planetNames.Sort();
        line.name = planetNames[0] + " - " + planetNames[1];

        bool goodLine = true;
        for (int x = 0; x < hyperspaceLanes.Count; x++)
        {
            string name = hyperspaceLanes[x].name;
            if (hyperspaceLanes[x].name.Equals(line.name))
            {
                goodLine = false;
            }
        }
        if (goodLine)
        {
            line.GetComponent<Line>().gameObject1 = planet1;
            line.GetComponent<Line>().gameObject2 = planet2;
            line.transform.parent = daddy;
            hyperspaceLanes.Add(line);
        }
        else
        {
            Destroy(line);
        }
    }

    public bool CheckIfPlanetHasHyperspaceLanes(GameObject planet)
    {
        for(int x = 0; x < hyperspaceLanes.Count; x++)
        {
            if (planet.name.Equals(hyperspaceLanes[x].GetComponent<Line>().gameObject1.name) || planet.name.Equals(hyperspaceLanes[x].GetComponent<Line>().gameObject2.name))
                return true;
        }
        return false;
    }
}
