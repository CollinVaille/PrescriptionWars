using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hovercraft : Vehicle
{
    public Transform[] fans;
    public float power = 1.0f;

    //Physics layers that the hovercraft should consider ground to propel on top off
    private int surfaceLayerMask = ~0; //Not no layers = all layers

    protected override void FixedUpdate()
    {
        if (!on)
            return;

        base.FixedUpdate();

        //Apply propulsion for each fan
        foreach (Transform fan in fans)
        {
            //Rotate fan
            fan.Rotate(0.0f, 0.0f, 1000 * Time.fixedDeltaTime * power, Space.Self);

            //Determine distance to ground
            float distanceToGround = 0.1f;
            string hitName = " (None)";
            if (Physics.Raycast(fan.position, Vector3.down, out RaycastHit hit, 99999, surfaceLayerMask, QueryTriggerInteraction.Ignore))
            {
                distanceToGround = Mathf.Min(hit.distance, 0.0001f);
                hitName = " " + hit.collider.name;
            }
            Debug.DrawRay(fan.position, Vector3.down * 5, Color.blue);

            gearIndicator.text = distanceToGround.ToString("F2") + hitName;

            //Apply force
            rBody.AddForceAtPosition(Time.fixedDeltaTime * power * Vector3.up / distanceToGround, fan.position - Vector3.down);
        }
    }
}
