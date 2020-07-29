using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    public GameObject gameObject1;          // Reference to the first GameObject
    public GameObject gameObject2;          // Reference to the second GameObject

    private LineRenderer line;                           // Line Renderer

    public Material lineMaterial;

    // Use this for initialization
    void Start()
    {
        // Add a Line Renderer to the GameObject
        line = gameObject.AddComponent<LineRenderer>();

        // Set the width of the Line Renderer
        line.startWidth = 0.20f;
        line.endWidth = 0.20f;
        //line.SetWidth(0.10F, 0.10F); //Deprecated

        // Set the number of vertex fo the Line Renderer
        line.positionCount = 2;
        //line.SetVertexCount(2); //Deprecated

        line.material = lineMaterial;

        line.startColor = Color.yellow;
        line.endColor = Color.yellow;

        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePosition()
    {
        // Check if the GameObjects are not null
        if (gameObject1 != null && gameObject2 != null)
        {
            // Update position of the two vertex of the Line Renderer
            line.SetPosition(0, gameObject1.transform.position);
            line.SetPosition(1, gameObject2.transform.position);
        }
    }
}