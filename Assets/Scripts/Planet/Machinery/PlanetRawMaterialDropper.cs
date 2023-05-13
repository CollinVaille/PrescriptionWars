using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRawMaterialDropper : PlanetFactoryMachine
{
    public GameObject payloadToInstantiate;
    public Transform placeToInstantiateAt;
    public AudioClip instantiationSound;

    protected override void OnStartOfStep(MachineStep step)
    {
        if (step == MachineStep.Outtake)
        {
            processingSlot = Instantiate(payloadToInstantiate).transform;
            processingSlot.position = placeToInstantiateAt.position;
            processingSlot.rotation = transform.rotation;

            if (!processingSlot.GetComponent<Rigidbody>())
                processingSlot.gameObject.AddComponent<Rigidbody>();

            AudioSource factoryProtoPillAudioSource = processingSlot.GetComponent<AudioSource>();
            God.god.ManageAudioSource(factoryProtoPillAudioSource);
            if (factoryProtoPillAudioSource)
                factoryProtoPillAudioSource.PlayOneShot(instantiationSound);
        }
    }

    protected override void OnEndOfStep(MachineStep step)
    {
        if(step == MachineStep.Outtake)
        {
            if (processingSlot && processingSlot.GetComponent<Rigidbody>())
                Destroy(processingSlot.GetComponent<Rigidbody>());
        }
    }

    protected override bool IsStartingMachine() { return true; }
}
