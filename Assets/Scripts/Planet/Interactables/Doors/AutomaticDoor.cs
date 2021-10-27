using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDoor : Door
{
    public AutomaticDoorTriggerZone[] triggerZones;

    private int pillLayers;
    private float radius;
    private Collider[] detected;

    private void Start()
    {
        //Player or good bot or bad bot, respectively = any pill
        pillLayers = (1 << 11) | (1 << 14) | (1 << 15);

        //Detection radius
        radius = 5.0f;
        //float radius = transform.lossyScale.magnitude * 3;

        //Use audio source of detected pill for opening/closing sounds
        detected = new Collider[1];
        detected[0] = null;

        StartCoroutine(AutomaticController());
    }

    private IEnumerator AutomaticController()
    {
        //Controller loop
        while (true)
        {
            //Update sensor 5 times a second
            yield return new WaitForSeconds(0.2f);

            if (transitioning)
                continue;

            if (open)
            {
                //If there is no one there, close door
                if (!DoorShouldBeOpen(false))
                {
                    //Use audio source of guy who triggered door to open if he's still around
                    if (detected[0] &&
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
                if (DoorShouldBeOpen(true))
                    yield return StartCoroutine(OpenDoor(detected[0]?.GetComponent<AudioSource>(), null));
            }
        }
    }

    private bool DoorShouldBeOpen(bool updateDetectedArray)
    {
        if (triggerZones != null)
        {
            foreach(AutomaticDoorTriggerZone triggerZone in triggerZones)
            {
                if (triggerZone != null && triggerZone.HasOccupants())
                    return true;
            }

            return false;
        }
        else if (updateDetectedArray)
            return Physics.OverlapSphereNonAlloc(transform.position, radius, detected, pillLayers) > 0;
        else
            return Physics.CheckSphere(transform.position, radius, pillLayers);
    }

}
