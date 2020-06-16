using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hovercraft : Vehicle
{
    public Transform[] fans;
    public float power = 1.0f, airCushion = 0.5f;

    //Physics layers that the hovercraft should consider ground to propel on top off
    private int surfaceLayerMask = ~0; //Not no layers = all layers

    protected override void FixedUpdate()
    {
        if (!on)
            return;

        base.FixedUpdate();

        //Steering wheel
        if (steeringWheel != 0)
            transform.Rotate(0.0f, steeringWheel * Time.fixedDeltaTime * turnStrength, 0.0f);

        UpdateFans();
    }

    private void UpdateFans()
    {
        //Rotate fans
        foreach (Transform fan in fans)
            fan.Rotate(0.0f, 0.0f, 1000 * Time.fixedDeltaTime * power, Space.Self);

        //Determine distance to ground
        float distanceToGround = airCushion;
        //string hitName = " (None)";
        Vector3 forcePosition = transform.position;
        forcePosition.y += floorPosition;
        if (Physics.Raycast(forcePosition, Vector3.down, out RaycastHit hit, 99999, surfaceLayerMask, QueryTriggerInteraction.Ignore))
        {
            distanceToGround = Mathf.Max(Mathf.Sqrt(hit.distance), airCushion);
            //hitName = " " + hit.collider.name;
        }

        //gearIndicator.text = distanceToGround.ToString("F2") + hitName;

        //Apply hover force
        rBody.AddForce(Time.fixedDeltaTime * power * Vector3.up / distanceToGround, ForceMode.Force);


        //Get back fan ground point
        Vector3 backPoint = fans[fans.Length - 1].position;
        if (Physics.Raycast(backPoint, Vector3.down, out hit, 99999, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            backPoint = hit.point;

        //Get front fan ground point
        Vector3 frontPoint = fans[0].position;
        if (Physics.Raycast(frontPoint, Vector3.down, out hit, 99999, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            frontPoint = hit.point;

        //Determine rotation of speeder based on back/front ground points
        transform.rotation = Quaternion.LookRotation(frontPoint - backPoint);
    }
}
