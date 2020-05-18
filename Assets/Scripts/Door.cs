using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorType { Single, Double }
    public enum DoorMotion { SlideX, SlideY, SlideZ }

    //Customization
    public AudioClip openSound, closeSound;
    public float openPosition, closePosition;
    public DoorType doorType = DoorType.Single;
    public DoorMotion doorMotion = DoorMotion.SlideX;
    public bool automatic = false;

    public Building building;

    //Status variables
    private bool open = false, transitioning = false;

    private void Start ()
    {
        if (automatic)
            StartCoroutine(AutomaticController());
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

    private IEnumerator OpenDoor (AudioSource audioSource, Pill botCulprit)
    {
        if (transitioning || open)
            yield break;

        transitioning = true;

        if (audioSource)
            audioSource.PlayOneShot(openSound);
        else
            AudioSource.PlayClipAtPoint(openSound, transform.position);

        if (building)
            building.DoorOpened();

        if(doorType == DoorType.Single)
        {
            Vector3 doorPosition = transform.localPosition;
            float duration = 0.75f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (doorMotion == DoorMotion.SlideX)
                    doorPosition.x = Mathf.Lerp(closePosition, openPosition, t / duration);
                else if (doorMotion == DoorMotion.SlideY)
                    doorPosition.y = Mathf.Lerp(closePosition, openPosition, t / duration);
                else
                    doorPosition.z = Mathf.Lerp(closePosition, openPosition, t / duration);

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

            float duration = 0.75f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if(doorMotion == DoorMotion.SlideX)
                {
                    leftDoorPosition.x = Mathf.Lerp(closePosition, openPosition, t / duration);
                    rightDoorPosition.x = -Mathf.Lerp(closePosition, openPosition, t / duration);
                }
                else if (doorMotion == DoorMotion.SlideY)
                {
                    leftDoorPosition.y = Mathf.Lerp(closePosition, openPosition, t / duration);
                    rightDoorPosition.y = -Mathf.Lerp(closePosition, openPosition, t / duration);
                }
                else
                {
                    leftDoorPosition.z = Mathf.Lerp(closePosition, openPosition, t / duration);
                    rightDoorPosition.z = -Mathf.Lerp(closePosition, openPosition, t / duration);
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

            if (!transitioning && open && !botCulprit.IsDead()
                && Vector3.Distance(botCulprit.transform.position, transform.position) < 12)
            {
                botCulprit.transform.LookAt(transform);
                StartCoroutine(CloseDoor(audioSource));
            }
        }
    }

    private IEnumerator CloseDoor (AudioSource audioSource)
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
            float duration = 0.75f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (doorMotion == DoorMotion.SlideX)
                    doorPosition.x = Mathf.Lerp(openPosition, closePosition, t / duration);
                else if (doorMotion == DoorMotion.SlideY)
                    doorPosition.y = Mathf.Lerp(openPosition, closePosition, t / duration);
                else
                    doorPosition.z = Mathf.Lerp(openPosition, closePosition, t / duration);

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

            float duration = 0.75f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if(doorMotion == DoorMotion.SlideX)
                {
                    leftDoorPosition.x = Mathf.Lerp(openPosition, closePosition, t / duration);
                    rightDoorPosition.x = -Mathf.Lerp(openPosition, closePosition, t / duration);
                }
                else if (doorMotion == DoorMotion.SlideY)
                {
                    leftDoorPosition.y = Mathf.Lerp(openPosition, closePosition, t / duration);
                    rightDoorPosition.y = -Mathf.Lerp(openPosition, closePosition, t / duration);
                }
                else
                {
                    leftDoorPosition.z = Mathf.Lerp(openPosition, closePosition, t / duration);
                    rightDoorPosition.z = -Mathf.Lerp(openPosition, closePosition, t / duration);
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

        if (building)
            building.DoorClosed();

        transitioning = false;
        open = false;
    }

    private void OnCollisionEnter (Collision collision)
    {
        if (open || transitioning)
            return;

        Pill tracking = collision.gameObject.GetComponent<Pill>();

        if (tracking && !collision.gameObject.GetComponent<Player>())
            StartCoroutine(OpenDoor(tracking.GetAudioSource(), tracking));
    }

    private IEnumerator AutomaticController ()
    {
        //Player or good bot or bad bot, respectively = any pill
        int pillLayers = (1 << 11) | (1 << 14) | (1 << 15);

        //Detection radius
        float radius = transform.lossyScale.magnitude * 3;

        //Use audio source of detected pill for opening/closing sounds
        Collider[] detected = new Collider[1];
        detected[0] = null;

        //Controller loop
        while (true)
        {
            //Update sensor 5 times a second
            yield return new WaitForSeconds(0.2f);

            if (transitioning)
                continue;

            if(open)
            {
                //If there is no one there, close door
                if (!Physics.CheckSphere(transform.position, radius, pillLayers))
                {
                    //Use audio source of guy who triggered door to open if he's still around
                    if(detected[0] &&
                       detected[0].gameObject.activeInHierarchy &&
                       Vector3.Distance(detected[0].transform.position, transform.position) < 20)
                        yield return StartCoroutine(CloseDoor(detected[0].GetComponent<AudioSource>()));
                    else
                        yield return StartCoroutine(CloseDoor(null));
                }
            }
            else
            {
                //If there is someone there, open door
                if (Physics.OverlapSphereNonAlloc(transform.position,
                                                  radius,
                                                  detected,
                                                  pillLayers) > 0)
                    yield return StartCoroutine(OpenDoor(detected[0].GetComponent<AudioSource>(), null));
            }
        }
    }
}
