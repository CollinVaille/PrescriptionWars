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

            AudioSource processingSlotAudioSource = processingSlot.GetComponent<AudioSource>();
            if (processingSlotAudioSource)
                processingSlotAudioSource.PlayOneShot(instantiationSound);
        }
    }

    protected override void BeforePushingToNextMachine()
    {
        if (processingSlot && processingSlot.GetComponent<Rigidbody>())
            Destroy(processingSlot.GetComponent<Rigidbody>());
    }

    protected override bool IsStartingMachine() { return true; }
}
