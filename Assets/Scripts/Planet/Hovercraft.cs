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
        if (Hovercast(forcePosition, out RaycastHit hit))
        {
            distanceToGround = Mathf.Max(Mathf.Sqrt(hit.distance), airCushion);
            //hitName = " " + hit.collider.name;
        }

        //gearIndicator.text = distanceToGround.ToString("F2") + hitName;

        //Apply hover force
        rBody.AddForce(Time.fixedDeltaTime * power * Vector3.up / distanceToGround, ForceMode.Force);

        //Get back fan ground point
        Vector3 backPoint = fans[fans.Length - 1].position;
        if (Hovercast(backPoint, out hit))
            backPoint = hit.point;

        //Get front fan ground point
        Vector3 frontPoint = fans[0].position;
        if (Hovercast(frontPoint, out hit))
            frontPoint = hit.point;

        //Determine rotation of speeder based on back/front ground points
        transform.rotation = Quaternion.LookRotation(frontPoint - backPoint);
    }

    //Casts ray downwards, detecting anything that should be "hovered over" (including water!)
    //Returns true if anything was hit
    private bool Hovercast(Vector3 origin, out RaycastHit hit)
    {
        if (Physics.Raycast(origin, Vector3.down, out hit, 99999, surfaceLayerMask, QueryTriggerInteraction.Ignore))
        {
            //Any hit below sea level in planets with sea gets replaced with equivalent sea level point
            //(How floating on water is achieved)
            if (Planet.planet.hasOcean)
            {
                //The exception is that hovercraft submerged can't use fans to push off water
                float seaLevel = Planet.planet.oceanTransform.position.y;
                if (hit.point.y < seaLevel && transform.position.y >= seaLevel)
                {
                    //Replace hit point
                    Vector3 newHitPoint = origin;
                    newHitPoint.y = seaLevel;
                    hit.point = newHitPoint;

                    //Replace hit distance
                    hit.distance = origin.y - seaLevel;

                    //Nothing else is used so no need to replace it...
                }
            }

            return true;
        }
        else
            return false;
    }
}
