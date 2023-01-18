using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoundationJSON
{
    //Non-serializable (ignored by serializer)
    public Transform transform;

    //Serializable
    public string prefab;
    public Vector3 localPosition;
    public Vector3 localScale;

    //Uses the values in the field variables to instantiate and customize a foundation.
    public void GenerateFoundation(City city)
    {
        //Instantiate and parent the foundation
        transform = GameObject.Instantiate(Resources.Load<GameObject>("Planet/City/Foundations/" + prefab)).transform;
        transform.parent = city.transform;

        //Apply customization to foundation
        transform.localPosition = localPosition;
        SetScale(transform.Find("Slab"), transform.Find("Ground"));
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        //Add foundation to saving system
        if (city.foundationManager.foundations == null)
            city.foundationManager.foundations = new List<FoundationJSON>();
        city.foundationManager.foundations.Add(this);

        //Add foundation to collider system
        if (city.foundationManager.foundationColliders == null)
            city.foundationManager.foundationColliders = new List<Collider>();
        city.foundationManager.foundationColliders.AddRange(transform.GetComponentsInChildren<Collider>());
    }

    private void SetScale(Transform slab, Transform ground)
    {
        //Scale the x, y, z of the slab
        slab.localScale = localScale - Vector3.up * 0.025f;

        //Scale the x, z of the ground
        Vector3 groundScale = ground.localScale.x * localScale;
        groundScale.y = ground.localScale.y;
        ground.localScale = groundScale;

        //Adjust the y position of the ground
        Vector3 groundPosition = ground.localPosition;
        groundPosition.y = groundPosition.y * localScale.y - 0.005f;
        ground.localPosition = groundPosition;
    }
}
