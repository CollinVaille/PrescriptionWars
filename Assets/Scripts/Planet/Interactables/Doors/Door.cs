using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    public enum DoorType { Single, Double }
    public enum DoorMotion { SlideX, SlideY, SlideZ }

    //Customization
    public AudioClip openSound, closeSound;
    public float openPosition, closePosition;
    public DoorType doorType = DoorType.Single;
    public DoorMotion doorMotion = DoorMotion.SlideX;
    public float openDuration = 0.75f, closeDuration = 0.75f;

    //References
    private IndoorZoneGrouping indoorZoneGrouping;

    //Status variables
    protected bool open = false, transitioning = false;

    private void Start()
    {
        indoorZoneGrouping = GetComponentInParent<IndoorZoneGrouping>();
    }

    public override void Interact(PlanetPill interacting)
    {
        base.Interact(interacting);

        ToggleDoorState(interacting.GetAudioSource());
    }

    public void ToggleDoorState (AudioSource audioSource)
    {
        if(!transitioning)
        {
            if (open)
                StartCoroutine(CloseDoor(audioSource));
            else
                StartCoroutine(OpenDoor(audioSource, null));
        }
    }

    protected IEnumerator OpenDoor (AudioSource audioSource, PlanetPill botCulprit)
    {
        if (transitioning || open)
            yield break;

        transitioning = true;

        if (audioSource)
            audioSource.PlayOneShot(openSound);
        else
            AudioSource.PlayClipAtPoint(openSound, transform.position);

        if (indoorZoneGrouping)
            indoorZoneGrouping.AddNewOpening();

        if(doorType == DoorType.Single)
        {
            Vector3 doorPosition = transform.localPosition;
            for (float t = 0; t < openDuration; t += Time.deltaTime)
            {
                if (doorMotion == DoorMotion.SlideX)
                    doorPosition.x = Mathf.Lerp(closePosition, openPosition, t / openDuration);
                else if (doorMotion == DoorMotion.SlideY)
                    doorPosition.y = Mathf.Lerp(closePosition, openPosition, t / openDuration);
                else
                    doorPosition.z = Mathf.Lerp(closePosition, openPosition, t / openDuration);

                transform.localPosition = doorPosition;

                yield return null;
            }
        }
        else //Double doors
        {
            Transform leftDoor = transform.Find("Left Door");
            Transform rightDoor = transform.Find("Right Door");

            Vector3 leftDoorPosition = leftDoor.localPosition;
            Vector3 rightDoorPosition = rightDoor.localPosition;

            for (float t = 0; t < openDuration; t += Time.deltaTime)
            {
                if(doorMotion == DoorMotion.SlideX)
                {
                    leftDoorPosition.x = Mathf.Lerp(closePosition, openPosition, t / openDuration);
                    rightDoorPosition.x = -Mathf.Lerp(closePosition, openPosition, t / openDuration);
                }
                else if (doorMotion == DoorMotion.SlideY)
                {
                    leftDoorPosition.y = Mathf.Lerp(closePosition, openPosition, t / openDuration);
                    rightDoorPosition.y = -Mathf.Lerp(closePosition, openPosition, t / openDuration);
                }
                else
                {
                    leftDoorPosition.z = Mathf.Lerp(closePosition, openPosition, t / openDuration);
                    rightDoorPosition.z = -Mathf.Lerp(closePosition, openPosition, t / openDuration);
                }

                leftDoor.localPosition = leftDoorPosition;
                rightDoor.localPosition = rightDoorPosition;

                yield return null;
            }

            if (doorMotion == DoorMotion.SlideX)
            {
                leftDoorPosition.x = openPosition;
                rightDoorPosition.x = -openPosition;
            }
            else if (doorMotion == DoorMotion.SlideY)
            {
                leftDoorPosition.y = openPosition;
                rightDoorPosition.y = -openPosition;
            }
            else
            {
                leftDoorPosition.z = openPosition;
                rightDoorPosition.z = -openPosition;
            }

            leftDoor.localPosition = leftDoorPosition;
            rightDoor.localPosition = rightDoorPosition;
        }

        transitioning = false;
        open = true;

        //If a bot opened the door, then have the bot automatically close the door after a second or so
        if(botCulprit)
        {
            yield return new WaitForSeconds(Random.Range(0.75f, 1.25f));

            if (!transitioning && open && !botCulprit.IsDead
                && Vector3.Distance(botCulprit.transform.position, transform.position) < 12)
            {
                botCulprit.transform.LookAt(transform);
                StartCoroutine(CloseDoor(audioSource));
            }
        }
    }

    protected IEnumerator CloseDoor (AudioSource audioSource)
    {
        if (transitioning || !open)
            yield break;

        transitioning = true;

        if(audioSource)
            audioSource.PlayOneShot(closeSound);
        else
            AudioSource.PlayClipAtPoint(closeSound, transform.position);

        if (doorType == DoorType.Single)
        {
            Vector3 doorPosition = transform.localPosition;
            for (float t = 0; t < closeDuration; t += Time.deltaTime)
            {
                if (doorMotion == DoorMotion.SlideX)
                    doorPosition.x = Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                else if (doorMotion == DoorMotion.SlideY)
                    doorPosition.y = Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                else
                    doorPosition.z = Mathf.Lerp(openPosition, closePosition, t / closeDuration);

                transform.localPosition = doorPosition;

                yield return null;
            }
        }
        else //Double doors
        {
            Transform leftDoor = transform.Find("Left Door");
            Transform rightDoor = transform.Find("Right Door");

            Vector3 leftDoorPosition = leftDoor.localPosition;
            Vector3 rightDoorPosition = rightDoor.localPosition;

            for (float t = 0; t < closeDuration; t += Time.deltaTime)
            {
                if(doorMotion == DoorMotion.SlideX)
                {
                    leftDoorPosition.x = Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                    rightDoorPosition.x = -Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                }
                else if (doorMotion == DoorMotion.SlideY)
                {
                    leftDoorPosition.y = Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                    rightDoorPosition.y = -Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                }
                else
                {
                    leftDoorPosition.z = Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                    rightDoorPosition.z = -Mathf.Lerp(openPosition, closePosition, t / closeDuration);
                }

                leftDoor.localPosition = leftDoorPosition;
                rightDoor.localPosition = rightDoorPosition;

                yield return null;
            }

            leftDoorPosition.x = closePosition;
            rightDoorPosition.x = -closePosition;

            leftDoor.localPosition = leftDoorPosition;
            rightDoor.localPosition = rightDoorPosition;
        }

        if (indoorZoneGrouping)
            indoorZoneGrouping.CloseExistingOpening();

        transitioning = false;
        open = false;
    }

    private void OnCollisionEnter (Collision collision)
    {
        if (open || transitioning)
            return;

        PlanetPill tracking = collision.gameObject.GetComponent<PlanetPill>();

        if (tracking && !tracking.IsPlayer)
            StartCoroutine(OpenDoor(tracking.GetAudioSource(), tracking));
    }

    protected override string GetInteractionVerb()
    {
        if (transitioning)
            return "";
        else
            return open ? "Close" : "Open";
    }
}
