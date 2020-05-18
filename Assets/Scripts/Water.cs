using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public AudioClip[] minorSplashes, majorSplashes, exits;

    private void OnTriggerEnter (Collider entering)
    {
        Pill pill = entering.GetComponent<Pill>();

        if (pill)
            pill.Submerge(GetComponent<Collider>());

        Rigidbody rBody = entering.GetComponent<Rigidbody>();

        if(rBody)
        {
            rBody.drag += 4;

            if(rBody.velocity.magnitude > 10)
                PlaySound(majorSplashes[Random.Range(0, majorSplashes.Length)], entering);
            else if(rBody.velocity.magnitude > 4)
                PlaySound(minorSplashes[Random.Range(0, minorSplashes.Length)], entering);             
        }
    }

    private void OnTriggerExit (Collider exiting)
    {
        Pill pill = exiting.GetComponent<Pill>();

        if (pill)
            pill.Emerge(GetComponent<Collider>());

        Rigidbody rBody = exiting.GetComponent<Rigidbody>();

        if (rBody)
        {
            rBody.drag -= 4;

            PlaySound(exits[Random.Range(0, exits.Length)], exiting);
        }
    }

    private void PlaySound (AudioClip sound, Collider source)
    {
        if (source.GetComponent<AudioSource>())
            source.GetComponent<AudioSource>().PlayOneShot(sound);
        else
            AudioSource.PlayClipAtPoint(sound, source.transform.position);
    }
}
