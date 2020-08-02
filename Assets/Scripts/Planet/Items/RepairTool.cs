using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTool : Item
{
    //Customization
    public float cooldown = 0.1f, repairPoints = 2.5f;

    //Status
    private float lastRepair = 0;
    private bool repairing = false;

    //Repair spark
    public Transform repairSpark;
    private Light sparkLight;
    private ParticleSystem sparkParticles;

    //SFX
    private AudioSource sfxSource;

    private void Start()
    {
        //SFX initialization
        sfxSource = GetComponent<AudioSource>();
        God.god.ManageAudioSource(sfxSource);

        //Spark initialization
        sparkLight = repairSpark.GetComponent<Light>();
        sparkParticles = repairSpark.GetComponent<ParticleSystem>();

        //Begin monitoring repairs
        StartCoroutine(RepairMonitoring());
    }

    public override void PrimaryAction()
    {
        Repair();
    }

    public override void PutInHand(Pill newHolder)
    {
        base.PutInHand(newHolder);

        if (holderIsPlayer)
            holder.GetComponent<Player>().SetContinuousPrimaryAction(cooldown > 0);
    }

    public void Repair()
    {
        if (!holder || Time.timeSinceLevelLoad - lastRepair <= cooldown)
            return;

        lastRepair = Time.timeSinceLevelLoad;
        SetRepairing(true);

        if (holder.RaycastShoot(transform, 5, out RaycastHit hit))
        {
            Pill pill = hit.collider.GetComponent<Pill>();

            if (pill)
                pill.Heal(repairPoints / 2.0f);
            else
            {
                Vehicle owningVehicle = GetOwningVehicle(hit.collider.transform);

                if (owningVehicle)
                    owningVehicle.FixPart(hit.collider.transform, repairPoints);
            }

            //Move spark to hit location
            repairSpark.position = hit.point;
        }
    }

    private IEnumerator RepairMonitoring()
    {
        while(true)
        {
            //Wait a jiffy
            yield return new WaitForSeconds(0.05f);

            //This is when things get interesting
            if (repairing)
            {
                //Stop repairing?
                if (!holder || Time.timeSinceLevelLoad - lastRepair > cooldown + 0.1f)
                    SetRepairing(false);
                else //If not, then let's make a light show!!!
                    sparkLight.intensity = Random.Range(2.0f, 4.0f);
            }
        }
    }

    private void SetRepairing(bool repairing)
    {
        if (this.repairing == repairing)
            return;

        this.repairing = repairing;

        if(repairing)
        {
            sfxSource.Play();

            sparkParticles.Play(true);
        }
        else
        {
            sfxSource.Pause();

            sparkParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        sparkLight.enabled = repairing;
    }

    private Vehicle GetOwningVehicle(Transform t)
    {
        Vehicle vehicle = t.GetComponent<Vehicle>();

        if (!vehicle && t.parent)
            vehicle = GetOwningVehicle(t.parent);

        return vehicle;
    }
}
