using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;

    private LineRenderer line = null;

    private List<GameObject> gameObjectsVar = null;
    /// <summary>
    /// Publicly accessible duplicate list of game objects that the line is attached to. The add and remove functions on this list will not work as intended.
    /// </summary>
    public List<GameObject> gameObjects
    {
        get
        {
            if (gameObjectsVar == null)
                return null;

            List<GameObject> list = new List<GameObject>();
            for(int index = 0; index < gameObjectsVar.Count; index++)
            {
                list.Add(gameObjectsVar[index]);
            }
            return list;
        }
    }

    public Color startColor
    {
        get => line == null ? Color.clear : line.startColor;
        set
        {
            if (line != null)
                line.startColor = value;
        }
    }
    public Color endColor
    {
        get => line == null ? Color.clear : line.endColor;
        set
        {
            if (line != null)
                line.endColor = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize(List<GameObject> gameObjects, Color startColor, Color endColor)
    {
        if(line != null)
        {
            Destroy(gameObject.GetComponent<LineRenderer>());
            line = null;
        }

        gameObjectsVar = gameObjects;

        if (gameObjects == null || gameObjects.Count < 2)
            return;
        foreach (GameObject g in gameObjects)
            if (g == null)
                return;

        //Add a Line Renderer to the GameObject.
        line = gameObject.AddComponent<LineRenderer>();

        //Set the width of the Line Renderer.
        line.startWidth = 0.20f;
        line.endWidth = 0.20f;

        //Set the number of vertices of the Line Renderer.
        line.positionCount = gameObjects.Count;

        line.material = lineMaterial;

        line.startColor = startColor;
        line.endColor = endColor;

        for(int gameObjectIndex = 0; gameObjectIndex < gameObjects.Count; gameObjectIndex++)
        {
            line.SetPosition(gameObjectIndex, gameObjects[gameObjectIndex].transform.position);
        }
    }
}