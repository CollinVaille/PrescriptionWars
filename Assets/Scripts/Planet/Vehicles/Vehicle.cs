using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vehicle : MonoBehaviour
{
    public static bool setUp = false;
    public static Text speedometerText, gearIndicatorText;
    public static int speedometerReading = 0;

    public static List<AudioClip> lightHits, mediumHits, hardHits;

    private static void InitializeStaticVariables()
    {
        if (setUp)
            return;

        speedometerText = PlanetPauseMenu.pauseMenu.HUD.Find("Speedometer").GetComponent<Text>();
        gearIndicatorText = PlanetPauseMenu.pauseMenu.HUD.Find("Gear Indicator").GetComponent<Text>();
        speedometerReading = 0;

        lightHits = new List<AudioClip>();
        God.InitializeAudioList(lightHits, "Planet/Vehicles/Damage SFX/Light Hit ");

        mediumHits = new List<AudioClip>();
        God.InitializeAudioList(mediumHits, "Planet/Vehicles/Damage SFX/Medium Hit ");

        hardHits = new List<AudioClip>();
        God.InitializeAudioList(hardHits, "Planet/Vehicles/Damage SFX/Hard Hit ");

        setUp = true;
    }

    //Power
    protected bool on = false, hasDriverCurrently = false;
    public AudioClip powerOn, powerOff;

    //References
    protected AudioSource generalAudio;
    protected Rigidbody rBody;
    private Speedometer speedometer;
    private List<Collider> vehicleColliders;
    private List<Transform> parts;
    private List<Vector3> originalPartPositions;
    private List<Quaternion> originalPartRotations;

    //Thrusting, braking, and steering
    [HideInInspector] protected float gasPedal = 0.0f; //0.0f = not pressed, 1.0f = full forward, -1.0f = full backward
    [HideInInspector] protected float steeringWheel = 0.0f; //0.0f = even/no rotation, 1.0f = full right, -1.0f = full left
    [SerializeField] private float thrustPower = 2000;
    public float brakePower = 1500, turnStrength = 90;
    public float floorPosition = -0.1f;

    //Gears
    public int[] gears;
    public AudioClip gearShift, gearStuck, slowDown;
    private int gearNumber = 0;
    protected int absoluteMaxSpeed = 1, currentMaxSpeed = 0, currentSpeed = 0;
    private bool lastGearIsThrust = false, thrusting = false;

    //Traction
    private bool tractionControl = false, cruiseControl = false;
    public int traction = 50;

    //Damage
    private float lastImpactTime = 0.0f;
    private List<Engine> forwardEngines;
    private float thrustPerEngine = 0.0f, brakingPerEngine = 0.0f;

    protected virtual void Awake()
    {
        forwardEngines = new List<Engine>();
    }

    protected virtual void Start()
    {
        InitializeStaticVariables();

        //Get references
        generalAudio = GetComponent<AudioSource>();
        rBody = GetComponent<Rigidbody>();

        //Initialize gear system
        absoluteMaxSpeed = gears[gears.Length - 1];
        RefreshGear(false, true);

        //Initialize vehicle colliders list
        vehicleColliders = new List<Collider>();
        AddCollidersRecursive(transform);

        //Initialize parts arrays
        parts = new List<Transform>(GetComponentsInChildren<Transform>());
        originalPartPositions = new List<Vector3>(parts.Count - 1);
        originalPartRotations = new List<Quaternion>(parts.Count - 1);
        for (int x = 0; x < parts.Count; x++)
        {
            //Don't allow root of vehicle in parts list
            if(parts[x] == transform)
            {
                parts.RemoveAt(x);
                x--;
                continue;
            }

            //Create parallel arrays for part
            originalPartPositions.Add(parts[x].localPosition);
            originalPartRotations.Add(parts[x].localRotation);
        }

        //Initialize traction
        tractionControl = traction > 0;
    }

    protected virtual void FixedUpdate()
    {
        if (!on)
            return;
        
        //Update speed reading
        int newSpeed = (int)rBody.velocity.magnitude;
        if (newSpeed + 10 < currentSpeed)
            generalAudio.PlayOneShot(slowDown, (currentSpeed - newSpeed) / 50.0f);
        currentSpeed = newSpeed;

        //Update traction
        UpdateTraction();
    }

    public virtual void SetPower(bool turnOn)
    {
        if (on == turnOn)
            return;

        on = turnOn;

        if (turnOn) //Turn on
            generalAudio.PlayOneShot(powerOn);
        else //Turn off
        {
            generalAudio.PlayOneShot(powerOff);
            gasPedal = 0.0f;
        }

        foreach (Engine engine in forwardEngines)
            engine.SetPower(turnOn);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        //Don't apply damage and sound effects to janky collisions with passengers and whatnot
        if (ShouldIgnoreCollision(collision))
            return;

        //Prevent redundant damage calls on single impact
        if (Time.timeSinceLevelLoad < lastImpactTime + 0.5f)
            return;
        lastImpactTime = Time.timeSinceLevelLoad;

        //Get impact speed
        float impactSpeed = collision.relativeVelocity.magnitude;

        //Based on impact speed, apply vehicle damage
        if (impactSpeed < 12 || transform.InverseTransformPoint(collision.GetContact(0).point).y < 0) //Harmless bump
            generalAudio.PlayOneShot(God.RandomClip(lightHits), impactSpeed / 12.0f);
        else if(impactSpeed < 40) //Light damage
        {
            generalAudio.PlayOneShot(God.RandomClip(mediumHits), impactSpeed / 40.0f);

            DamageParts(collision.GetContact(0).point, impactSpeed / 10.0f, impactSpeed, true);
        }
        else //Heavy damage
        {
            generalAudio.PlayOneShot(God.RandomClip(hardHits), impactSpeed / 60.0f);

            DamageParts(collision.GetContact(0).point, impactSpeed / 10.0f, impactSpeed, true);
        }
    }

    private bool ShouldIgnoreCollision(Collision collision)
    {
        return collision.transform.GetComponentInParent<Pill>();
    }

    public void DamageParts(Vector3 contactPoint, float radius, float impactSpeed, bool recursive)
    {
        foreach (Transform part in parts)
            DamagePart(part, contactPoint, radius, impactSpeed, recursive);
    }

    public void DamagePart(Transform part, Vector3 contactPoint, float radius, float impactSpeed, bool recursive)
    {
        //Determine if part is allowed to be damaged
        if (!part || !part.CompareTag("Untagged") || Vector3.Distance(part.position, contactPoint) > radius)
            return;

        //Remember original position
        Vector3 originalLocalPosition = part.localPosition;

        //Determine where in local space (i.e. relative to car part) impact occured
        Vector3 localContactPoint = part.InverseTransformPoint(contactPoint);

        //Rotate and translate part
        part.Rotate(-localContactPoint.y * (impactSpeed / 12.0f), localContactPoint.x * (impactSpeed / 12.0f), 0.0f, Space.Self);
        part.Translate(0.0f, 0.0f, Mathf.Clamp(-localContactPoint.z * impactSpeed * 0.005f, -0.4f, 0.4f), Space.Self);

        //This means the part moved away from the center of the vehicle and thus could now be floating in midair so undo translation to prevent that
        if (part.localPosition.magnitude > originalLocalPosition.magnitude)
            part.localPosition = originalLocalPosition;

        //"Recursively" apply damage
        //I know this recursive system is confusing but it came about from a long effort to make
        //a part-specific damage system that can take damage both from the root transform detecting crash collisions
        //and the children detecting point damage from things like bullets, punches, explosions, fire, etc.
        if (recursive && part.GetComponent<IDamageable>() != null)
            part.GetComponent<IDamageable>().Damage(impactSpeed, 0, contactPoint, DamageType.Melee, -53); //Don't change -53
    }

    public void PlayDamageSound(float damageAmount)
    {
        if (damageAmount <= 12) //Harmless bump
            generalAudio.PlayOneShot(God.RandomClip(lightHits), damageAmount / 12.0f);
        else if (damageAmount < 40) //Light damage
            generalAudio.PlayOneShot(God.RandomClip(mediumHits), damageAmount / 40.0f);
        else //Heavy damage
            generalAudio.PlayOneShot(God.RandomClip(hardHits), damageAmount / 60.0f);
    }

    public void UpdateSpeedometer()
    {
        if (currentSpeed != speedometerReading)
        {
            speedometerReading = currentSpeed;
            speedometerText.text = speedometerReading + " mph";
        }
    }

    public void UpdateGearIndicator()
    {
        if(gearNumber == 0)
            gearIndicatorText.text = "Park";
        else if(thrusting && gearNumber == gears.Length - 1)
            gearIndicatorText.text = "Thrusting";
        else
            gearIndicatorText.text = "Gear " + gearNumber;
    }

    public bool Grounded(float errorMargin)
    {
        return Physics.Raycast(transform.position + Vector3.one * floorPosition,
            Vector3.down, errorMargin);
    }

    public void ChangeGear(bool goUpOne, bool updateIndicator)
    {
        //Can't change gears while thrusting
        if(thrusting)
        {
            generalAudio.PlayOneShot(gearStuck);
            return;
        }

        //Change gear number
        if (goUpOne)
        {
            if (lastGearIsThrust)
            {
                if (++gearNumber >= gears.Length - 1)
                {
                    gearNumber = gears.Length - 2;
                    generalAudio.PlayOneShot(gearStuck);
                    return;
                }
            }
            else
            {
                if (++gearNumber >= gears.Length)
                {
                    gearNumber = gears.Length - 1;
                    generalAudio.PlayOneShot(gearStuck);
                    return;
                }
            }
        }
        else if(--gearNumber < 0)
        {
            gearNumber = 0;
            generalAudio.PlayOneShot(gearStuck);
            return;
        }

        RefreshGear(updateIndicator, false);
    }

    protected virtual void RefreshGear(bool updateIndicator, bool skipSoundEffect)
    {
        //Update effects
        currentMaxSpeed = gears[gearNumber];

        if(!skipSoundEffect)
            generalAudio.PlayOneShot(gearShift);

        if (updateIndicator)
            UpdateGearIndicator();

        if (speedometer)
            speedometer.UpdateGear(currentMaxSpeed);
    }

    private void AddCollidersRecursive(Transform t)
    {
        foreach(Collider c in t.gameObject.GetComponents<Collider>())
            vehicleColliders.Add(c);

        foreach (Transform child in t)
            AddCollidersRecursive(child);
    }

    public void SetPassengerCollisionRecursive(Transform passengerTransform, bool ignoreCollision)
    {
        Collider passengerCollider = passengerTransform.gameObject.GetComponent<Collider>();
        if (passengerCollider)
        {
            foreach (Collider vehicleCollider in vehicleColliders)
            {
                if(vehicleCollider)
                    Physics.IgnoreCollision(passengerCollider, vehicleCollider, ignoreCollision);
            }
        }

        foreach (Transform child in passengerTransform)
            SetPassengerCollisionRecursive(child, ignoreCollision);
    }

    private void UpdateTraction ()
    {
        if (tractionControl && currentSpeed > 0)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rBody.velocity);
            if (localVelocity.x < 0) //Eliminate negative velocity
            {
                localVelocity.x += Time.fixedDeltaTime * traction;

                if (localVelocity.x > 0) //Boundary check
                    localVelocity.x = 0;
                //else if (!drifting && localVelocity.x < -50)
                //    StartCoroutine(DriftingSFX());
            }
            else //Eliminate positive velocity
            {
                localVelocity.x -= Time.fixedDeltaTime * traction;

                if (localVelocity.x < 0) //Boundary check
                    localVelocity.x = 0;
                //else if (!drifting && localVelocity.x > 50)
                //    StartCoroutine(DriftingSFX());
            }
            rBody.velocity = transform.TransformDirection(localVelocity);
        }
    }

    public void AddForwardEngine (Engine engine, bool onStartUp)
    {
        forwardEngines.Add(engine);

        if (onStartUp)
        {
            thrustPerEngine = thrustPower / forwardEngines.Count;

            if (engine.supportReverseThrusting)
                brakingPerEngine = brakePower / forwardEngines.Count;
        }
        else
        {
            thrustPower += thrustPerEngine;

            if (engine.supportReverseThrusting)
                brakePower += brakingPerEngine;
        }
    }

    public void RemoveForwardEngine (Engine engine)
    {
        forwardEngines.Remove(engine);

        thrustPower -= thrustPerEngine;

        if (engine.supportReverseThrusting)
            brakePower -= brakingPerEngine;
    }

    public List<Engine> GetForwardEngines() { return forwardEngines; }

    public bool PoweredOn () { return on; }

    public AudioSource GetGeneralAudio () { return generalAudio; }

    public void FixPart (Transform part, float repairPoints)
    {
        int partIndex = parts.IndexOf(part);

        //Part has to be preexisting in parts list to be repaired
        if (partIndex < 0 || partIndex >= parts.Count)
            return;

        part.localPosition = Vector3.Lerp(part.localPosition, originalPartPositions[partIndex], 0.5f);
        part.localRotation = Quaternion.Lerp(part.localRotation, originalPartRotations[partIndex], 0.5f);

        VehiclePart vehiclePart = part.GetComponent<VehiclePart>();
        if (vehiclePart)
            vehiclePart.Repair(repairPoints);

        //Recursively fix ancestry
        //So if knob on wing is being repaired and wing is parent of knob, wing gets repaired too
        if (part.parent)
            FixPart(part.parent, repairPoints);
    }

    protected bool MovingBackward() { return transform.InverseTransformDirection(rBody.velocity).z < -0.1f; }

    protected virtual void UpdateEngineEffects()
    {
        bool backwardThrusting = MovingBackward();

        foreach (Engine engine in forwardEngines)
            engine.UpdateEngineEffects(backwardThrusting, currentSpeed, absoluteMaxSpeed, ForwardEngineAudioCoefficient());
    }

    private bool CruiseControlActivated() { return cruiseControl; }

    public virtual void SetCruiseControl(bool turnCruiseControlOn) { cruiseControl = turnCruiseControlOn; }

    public void SetGasPedal(float gasPedal)
    {
        if (CruiseControlActivated() || thrusting)
            return;

        this.gasPedal = gasPedal;
    }

    public void SetThrusting(bool thrusting)
    {
        this.thrusting = thrusting;

        if(thrusting)
        {
            gasPedal = 1.0f;

            gearNumber = gears.Length - 1;
            RefreshGear(true, true);
        }
        else
        {
            gearNumber = gears.Length - 2;
            RefreshGear(true, true);
        }
    }

    protected float GetThrustPower() { return thrusting ? thrustPower * 2.0f : thrustPower; }

    public void SetSteeringWheel(float steeringWheel)
    {
        if (!CruiseControlActivated())
            this.steeringWheel = steeringWheel;
    }

    public int GetCurrentGear() { return gearNumber; }

    public void CoupleThrusterToVehicle() { lastGearIsThrust = true; }

    public void CoupleSpeedometerToVehicle(Speedometer speedometer) { this.speedometer = speedometer; }

    public float ForwardEngineAudioCoefficient() { return 1.0f + ((1.0f * currentSpeed) / absoluteMaxSpeed); }

    public void UpdateDriverStatus(bool hasDriverNow)
    {
        if (hasDriverCurrently == hasDriverNow)
            return;

        hasDriverCurrently = hasDriverNow;

        if (hasDriverNow)
            SetPower(true);
        else if (!CruiseControlActivated())
            SetPower(false);
    }

    public Rigidbody GetRBody() { return rBody; }
}
