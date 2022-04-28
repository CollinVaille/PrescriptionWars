using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altimeter : VehiclePart
{
    //Customization
    [SerializeField] private Vector2 meterLimits;
    [SerializeField] private Vector3 raycastStartOffset;

    //References
    private Vehicle vehicle;
    private Transform selfHead, trackingHead;

    protected override void Start()
    {
        base.Start();
        SetReferences();
        StartCoroutine(UpdateLoop());
    }

    private void SetReferences()
    {
        vehicle = transform.GetComponentInParent<Vehicle>();
        selfHead = transform.Find("Self Head");
        trackingHead = transform.Find("Closest Obstable Head");
    }

    private IEnumerator UpdateLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateAltimeter();
        }
    }

    private void UpdateAltimeter()
    {
        UpdateHead(selfHead, vehicle.transform.position.y);
        UpdateHead(trackingHead, GetTrackingAltitude());
    }

    private void UpdateHead(Transform head, float altitude)
    {
        //0 = lowest allowed altitude, 0.5 = halfway up, 1 = highest allowed altitude
        float altitudePercentage = (altitude - God.god.altitudeLimits.x) / (God.god.altitudeLimits.y - God.god.altitudeLimits.x);

        Vector3 headPos = head.localPosition;
        headPos.z = Mathf.Lerp(meterLimits.x, meterLimits.y, altitudePercentage);
        head.localPosition = headPos;
    }

    private float GetTrackingAltitude()
    {
        if (Physics.Raycast(vehicle.transform.TransformPoint(raycastStartOffset), Vector3.down, out RaycastHit hitInfo))
            return hitInfo.point.y;
        else
            return God.god.altitudeLimits.x;
    }
}
