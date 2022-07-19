using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;

    private List<GameObject> gameObjects = null;

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
        this.gameObjects = gameObjects;

        if (gameObjects == null || gameObjects.Count < 2)
            return;
        foreach (GameObject g in gameObjects)
            if (g == null)
                return;

        //Add a Line Renderer to the GameObject.
        LineRenderer line = gameObject.AddComponent<LineRenderer>();

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