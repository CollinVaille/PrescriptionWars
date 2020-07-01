﻿using System.Collections;
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
            GameObject line = Instantiate(linePrefab);
            line.name = lineName;
            line.GetComponent<Line>().gameObject1 = planet1;
            line.GetComponent<Line>().gameObject2 = planet2;
            line.transform.parent = daddy;
            hyperspaceLanes.Add(line);
        }
    }
}