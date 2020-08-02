using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vehicle : MonoBehaviour
{
    public static bool setUp = false;
    public static Text speedometer, gearIndicator;
    public static int speedometerReading = 0;

    public static List<AudioClip> lightHits, mediumHits, hardHits;

    private static void InitializeStaticVariables()
    {
        if (setUp)
            return;

        speedometer = God.god.HUD.Find("Speedometer").GetComponent<Text>();
        gearIndicator = God.god.HUD.Find("Gear Indicator").GetComponent<Text>();
        speedometerReading = 0;

        lightHits = new List<AudioClip>();
        God.InitializeAudioList(lightHits, "Vehicles/Damage SFX/Light Hit ");

        mediumHits = new List<AudioClip>();
        God.InitializeAudioList(mediumHits, "Vehicles/Damage SFX/Medium Hit ");

        hardHits = new List<AudioClip>();
        God.InitializeAudioList(hardHits, "Vehicles/Damage SFX/Hard Hit ");

        setUp = true;
    }

    //Power
    protected bool on = false;
    public AudioClip powerOn, powerOff;

    //References
    protected AudioSource generalAudio, engineAudio;
    protected Rigidbody rBody;
    private List<Collider> vehicleColliders;
    private List<Transform> parts;
    private List<Vector3> originalPartPositions;
    private List<Quaternion> originalPartRotations;

    //Thrusting, braking, and steering
    [HideInInspector] public float gasPedal = 0.0f; //0.0f = not pressed, 1.0f = full forward, -1.0f = full backward
    [HideInInspector] public float steeringWheel = 0.0f; //0.0f = even/no rotation, 1.0f = full right, -1.0f = full left
    public float thrustPower = 2000, brakePower = 1500, turnStrength = 90;
    public float floorPosition = -0.1f;

    //Gears
    public int[] gears;
    public AudioClip gearShift, gearStuck, slowDown;
    private int gearNumber = 0;
    protected int maxSpeed = 0, currentSpeed = 0;

    //Traction
    private bool tractionControl = false;
    public int traction = 50;

    //Damage
    private float lastImpactTime = 0.0f;
    protected List<Engine> engines;
    private float thrustPerEngine = 0.0f, brakingPerEngine = 0.0f;

    private void Awake()
    {
        engines = new List<Engine>();
    }

    protected virtual void Start()
    {
        InitializeStaticVariables();

        //Get references
        generalAudio = GetComponent<AudioSource>();
        engineAudio = GetComponents<AudioSource>()[1];
        rBody = GetComponent<Rigidbody>();

        //Make engine noise stop on pause
        God.god.ManageAudioSource(engineAudio);

        //Initialize gear system
        maxSpeed = gears[gearNumber];

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

        //Update engine pitch
        engineAudio.pitch = Mathf.Max(1, currentSpeed / 25.0f);

        //Update movement
        if (currentSpeed > maxSpeed) //Slow down if going over speed limit
            rBody.AddForce(-rBody.velocity * Time.fixedDeltaTime,  ForceMode.VelocityChange);
        else //Under speed limit; normal control
        {
            if (gasPedal > 0)
                rBody.AddForce(transform.forward * Time.fixedDeltaTime * thrustPower);
            else if (gasPedal < 0)
                rBody.AddForce(-transform.forward * Time.fixedDeltaTime * brakePower);

            //Stabilize vehicle to zero mph if operator doesn't resist
            if (currentSpeed < 3)
                rBody.AddForce(-rBody.velocity * Time.fixedDeltaTime / 5.0f, ForceMode.VelocityChange);
        }

        //Update traction
        UpdateTraction();
    }

    public virtual void SetPower(bool turnOn)
    {
        if (on == turnOn)
            return;

        if(turnOn) //Turn on
        {
            generalAudio.PlayOneShot(powerOn);

            engineAudio.Play();
        }
        else //Turn off
        {
            generalAudio.PlayOneShot(powerOff);

            engineAudio.pitch = 1.0f;
            engineAudio.Pause();

            gasPedal = 0.0f;
        }

        on = turnOn;
    }

    protected void OnCollisionEnter(Collision collision)
    {
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

    public void DamageParts(Vector3 contactPoint, float radius, float impactSpeed, bool recursive)
    {
        foreach (Transform part in parts)
            DamagePart(part, contactPoint, radius, impactSpeed, recursive);
    }

    public void DamagePart(Transform part, Vector3 contactPoint, float radius, float impactSpeed, bool recursive)
    {
        //Determine if part is allowed to be damaged
        if (!part.CompareTag("Untagged") || Vector3.Distance(part.position, contactPoint) > radius)
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
        if (recursive && part.GetComponent<Damageable>() != null)
            part.GetComponent<Damageable>().Damage(impactSpeed, 0, contactPoint, DamageType.Melee, -53); //Don't change -53
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
            speedometer.text = speedometerReading + " mph";
        }
    }

    public void UpdateGearIndicator()
    {
        if(gearNumber == 0)
            gearIndicator.text = "Park";
        else
            gearIndicator.text = "Gear " + gearNumber;
    }

    public bool Grounded(float errorMargin)
    {
        return Physics.Raycast(transform.position + Vector3.one * floorPosition,
            Vector3.down, errorMargin);
    }

    public void ChangeGear(bool goUpOne, bool updateIndicator)
    {
        //Change gear number
        if (goUpOne)
        {
            if(++gearNumber >= gears.Length)
            {
                gearNumber = gears.Length - 1;
                generalAudio.PlayOneShot(gearStuck);
            }
        }
        else if(--gearNumber < 0)
        {
            gearNumber = 0;
            generalAudio.PlayOneShot(gearStuck);
        }

        //Update effects
        maxSpeed = gears[gearNumber];

        generalAudio.PlayOneShot(gearShift);
        if (updateIndicator)
            UpdateGearIndicator();
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
                Physics.IgnoreCollision(passengerCollider, vehicleCollider, ignoreCollision);
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

    public void AddEngine (Engine engine, bool onStartUp)
    {
        engines.Add(engine);

        if(onStartUp)
        {
            thrustPerEngine = thrustPower / engines.Count;
            brakingPerEngine = brakePower / engines.Count;
        }
        else
        {
            thrustPower += thrustPerEngine;
            brakePower += brakingPerEngine;
        }
    }

    public void RemoveEngine (Engine engine)
    {
        engines.Remove(engine);

        thrustPower -= thrustPerEngine;
        brakePower -= brakingPerEngine;
    }

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
}
