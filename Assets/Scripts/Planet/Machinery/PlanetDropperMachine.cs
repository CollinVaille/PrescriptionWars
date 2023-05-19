using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDropperMachine : PlanetFactoryMachine
{
    public GameObject payloadToInstantiate;
    public Transform placeToInstantiateAt;
    public AudioClip instantiationSound;
    public Material subjectMaterial;
    public Mesh cylinderMesh;

    private Queue<Transform> bufferedSubjects;

    protected override void OnStartOfStep(MachineStep step)
    {
        if (step == MachineStep.Outtake)
        {
            processingSlot = GetSubjectToDrop();
            PrepareSubjectForDropping(processingSlot);
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

    private void EnsureSubjectHasConstrainedRigidbody(Transform subject)
    {
        Rigidbody subjectsRigidbody = subject.GetComponent<Rigidbody>();
        if (!subjectsRigidbody)
            subjectsRigidbody = subject.gameObject.AddComponent<Rigidbody>();

        subjectsRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private Transform GetSubjectToDrop()
    {
        if (bufferedSubjects != null && bufferedSubjects.Count > 0)
        {
            Transform subject = bufferedSubjects.Dequeue();
            subject.gameObject.SetActive(true);
            return subject;
        }
        else
            return Instantiate(payloadToInstantiate).transform;
    }

    private void PrepareSubjectForDropping(Transform subject)
    {
        //Set material type
        PlanetMaterial.SetMaterialType(PlanetMaterialType.Swamp, subject.gameObject);

        //Set visual material
        subject.GetComponent<MeshRenderer>().sharedMaterial = subjectMaterial;

        //Set mesh
        subject.GetComponent<MeshFilter>().mesh = cylinderMesh;

        //Set position and rotation
        subject.position = placeToInstantiateAt.position;
        subject.rotation = transform.rotation;

        //Set audio
        AudioSource factoryProtoPillAudioSource = subject.GetComponent<AudioSource>();
        God.god.ManageAudioSource(factoryProtoPillAudioSource);
        if (factoryProtoPillAudioSource)
            factoryProtoPillAudioSource.PlayOneShot(instantiationSound);

        //Add drop physics
        EnsureSubjectHasConstrainedRigidbody(subject);
    }

    public void AddSubjectToBuffer(Transform subject)
    {
        if (bufferedSubjects == null)
            bufferedSubjects = new Queue<Transform>();
        bufferedSubjects.Enqueue(subject);
    }
}
