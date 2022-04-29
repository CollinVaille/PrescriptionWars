using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : VehiclePart
{
    //Customization
    [SerializeField] private Vector2 meterLimits;
    public Material normalSpeedMaterial, redLiningMaterial;

    //References
    private Transform gearHead, speedBar;

    //Status variables
    private float maxSpeed, oneGearLessMaxSpeed;
    private Vector3 previousLocation;

    public void UpdateGear(float newGearsMaxSpeed)
    {
        UpdateHeadPosition(gearHead, newGearsMaxSpeed / maxSpeed);
    }

    protected override void Start()
    {
        base.Start();

        SetReferencesAndVariables();
        VehicleSetUp();

        StartCoroutine(UpdateLoop());
    }

    private void SetReferencesAndVariables()
    {
        gearHead = transform.Find("Gear Head");
        speedBar = transform.Find("Speed Bar");
        previousLocation = transform.position;
    }

    private void VehicleSetUp()
    {
        Vehicle vehicle = transform.GetComponentInParent<Vehicle>();
        vehicle.CoupleSpeedometerToVehicle(this);
        SetRedLine(vehicle);
    }

    private void SetRedLine(Vehicle vehicle)
    {
        maxSpeed = vehicle.gears[vehicle.gears.Length - 1];
        oneGearLessMaxSpeed = vehicle.gears[vehicle.gears.Length - 2];
        Transform redLine = transform.Find("Red Line");

        UpdateHeadPosition(redLine, oneGearLessMaxSpeed / maxSpeed);
    }

    private IEnumerator UpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateSpeedometer();
        }
    }

    private void UpdateSpeedometer()
    {
        float speed = UpdateSpeed();
        float percentage = speed / maxSpeed;

        UpdateHeadPosition(speedBar, percentage * 0.5f);
        UpdateHeadScale(speedBar, percentage);
        UpdateSpeedBarMaterial(speed);
    }

    private float UpdateSpeed()
    {
        float speed = Vector3.Distance(previousLocation, transform.position) * 2.0f;
        previousLocation = transform.position;
        return speed;
    }

    private void UpdateHeadPosition(Transform head, float percentage)
    {
        Vector3 headPos = head.localPosition;
        headPos.z = Mathf.Lerp(meterLimits.x, meterLimits.y, percentage);
        head.localPosition = headPos;
    }

    private void UpdateHeadScale(Transform head, float percentage)
    {
        Vector3 headScale = head.localScale;
        headScale.z = (meterLimits.y - meterLimits.x) * percentage;
        head.localScale = headScale;
    }

    private void UpdateSpeedBarMaterial(float speed)
    {
        if (speed < oneGearLessMaxSpeed + 1)
            SetMaterialOnTransform(speedBar, normalSpeedMaterial);
        else
            SetMaterialOnTransform(speedBar, redLiningMaterial);
    }

    private void SetMaterialOnTransform(Transform trans, Material mat)
    {
        Renderer rend = trans.GetComponent<Renderer>();

        if (rend.sharedMaterial != mat)
            rend.sharedMaterial = mat;
    }
}
