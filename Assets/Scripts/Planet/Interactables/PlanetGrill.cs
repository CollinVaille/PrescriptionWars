using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGrill : Interactable, IItemRackSlotMonitor
{
    public Transform rotatingPart;
    public ParticleSystem embers;
    public AudioClip openGrill, closeGrill, charSound;
    public AudioSource grillAudioSource;
    public ItemRackSlot grillSlot;
    public Material charredMaterial;

    private bool on = false, transitioning = false;
    private int onStateInstance = 0;

    private void Awake()
    {
        grillSlot.AddSwapEventListener(this);
    }

    public override void Interact(Pill interacting)
    {
        if (transitioning)
            return;

        StartCoroutine(ChangeOnState());
    }

    private IEnumerator ChangeOnState()
    {
        //Starting
        transitioning = true;
        bool turningOn = !on;

        //Turn off grill before we start swinging it shut
        if (turningOn)
            onStateInstance++;
        else
        {
            embers.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            grillAudioSource.Stop();
            God.god.UnmanageAudioSource(grillAudioSource);
        }

        //Play starting sound
        grillAudioSource.PlayOneShot(turningOn ? openGrill : closeGrill);

        //Transition period
        for(float transitionElapsedTime = 0.0f, transitionTotalTime = 2.5f; transitionElapsedTime < transitionTotalTime; transitionElapsedTime += Time.deltaTime)
        {
            float lerpValue = transitionElapsedTime / transitionTotalTime;
            if (!turningOn)
                lerpValue = 1.0f - lerpValue;

            SetPartsLocalXRotation(rotatingPart, Mathf.Lerp(0.0f, -90.0f, lerpValue));

            //Wait a frame
            yield return null;
        }

        //Finalize transition
        SetPartsLocalXRotation(rotatingPart, turningOn ? -90.0f : 0.0f);
        if (turningOn)
        {
            embers.Play(true);

            God.god.ManageAudioSource(grillAudioSource);
            grillAudioSource.Play();

            //Grill any item currently on the grill
            GrillItem(grillSlot.GetStowedItem());
        }

        //Done transitioning
        on = turningOn;
        transitioning = false;

        //Things to do after we're done transitioning
        if (turningOn)
            StartCoroutine(OnStateLoop());
    }

    private IEnumerator OnStateLoop()
    {
        int onStateLoopInstance = onStateInstance;

        Ray ray = new Ray(transform.position, Vector3.up);
        RaycastHit[] hits = new RaycastHit[5];

        while (onStateLoopInstance == onStateInstance && on)
        {
            //Check above the grill to see if we should set anything on fire
            int hitCount = Physics.RaycastNonAlloc(ray, hits, 2.5f, ~0, QueryTriggerInteraction.Ignore);
            if (hitCount > 0)
            {
                for(int x = 0; x < hitCount && x < hits.Length; x++)
                {
                    if (Fire.IsFlammable(hits[x].transform))
                        Fire.SetOnFire(hits[x].transform, Vector3.zero, Random.Range(0.5f, 2.0f));
                }
            }

            //Wait a fraction of a second
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SetPartsLocalXRotation(Transform part, float localXRotation)
    {
        part.localEulerAngles = GeneralHelperMethods.GetModifiedVector3(part.localEulerAngles, localXRotation, GeneralHelperMethods.WhichVector.X);
    }

    public override bool OverrideTriggerDescription() { return true; }

    protected override string GetInteractionVerb() { return on ? "Turn Off" : "Turn On"; }

    public void OnItemRackChange(Item previouslyStowed, Item newlyStowed, ItemRackSlot itemRackSlot, bool initialSpawnEvent)
    {
        if (on)
            GrillItem(newlyStowed);
    }

    public void GrillItem(Item itemToGrill)
    {
        if (!itemToGrill)
            return;

        MeshRenderer[] renderersToGrill = itemToGrill.GetComponentsInChildren<MeshRenderer>();
        if(renderersToGrill != null)
        {
            foreach (MeshRenderer rendererToGrill in renderersToGrill)
                rendererToGrill.sharedMaterial = charredMaterial;
        }

        grillAudioSource.PlayOneShot(charSound);
    }
}
