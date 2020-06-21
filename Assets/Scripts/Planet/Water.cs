using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public AudioClip[] minorSplashes, majorSplashes, exits;
    private List<Rigidbody> submerged;

    private void Awake()
    {
        submerged = new List<Rigidbody>();
    }

    private void OnTriggerEnter (Collider entering)
    {
        //Get pill
        Pill pill = entering.GetComponent<Pill>();

        //Submerge pill
        if (pill)
            pill.Submerge(GetComponent<Collider>());

        //Get rigidbody
        Rigidbody rBody = entering.GetComponent<Rigidbody>();
        if (!rBody)
            rBody = entering.transform.root.GetComponent<Rigidbody>();
        
        //Submerge rigidbody
        if(rBody)
        {
            if(!submerged.Contains(rBody))
            {
                submerged.Add(rBody);

                rBody.drag += 4;

                if (rBody.velocity.magnitude > 10)
                    PlaySound(majorSplashes[Random.Range(0, majorSplashes.Length)], entering);
                else if (rBody.velocity.magnitude > 4)
                    PlaySound(minorSplashes[Random.Range(0, minorSplashes.Length)], entering);
            }

            if (entering.CompareTag("Buoyant"))
                StartCoroutine(FloatToSurface(rBody, entering));
        }
    }

    private void OnTriggerExit (Collider exiting)
    {
        //Get pill
        Pill pill = exiting.GetComponent<Pill>();

        //Emerge pill
        if (pill)
            pill.Emerge(GetComponent<Collider>());

        //Get rigidbody
        Rigidbody rBody = exiting.GetComponent<Rigidbody>();
        if (!rBody)
            rBody = exiting.transform.root.GetComponent<Rigidbody>();

        //Emerge rigidbody
        if (rBody && submerged.Contains(rBody))
        {
            submerged.Remove(rBody);

            rBody.drag -= 4;

            PlaySound(exits[Random.Range(0, exits.Length)], exiting);
        }
    }

    private IEnumerator FloatToSurface(Rigidbody buoyantBody, Collider buoyantCollider)
    {
        float surfaceLevel = transform.GetComponent<Collider>().bounds.max.y;

        //Debug.Log("Floating " + buoyantBody.name + " to surface. (" + name + ")");

        buoyantBody.useGravity = false;

        while(buoyantCollider.bounds.min.y <= surfaceLevel)
        {
            buoyantBody.MovePosition(buoyantBody.position + Vector3.up * Time.deltaTime);
            yield return null;
        }

        buoyantBody.useGravity = true;

        //Debug.Log("Done.");
    }

    private void PlaySound (AudioClip sound, Collider source)
    {
        if (source.GetComponent<AudioSource>())
            source.GetComponent<AudioSource>().PlayOneShot(sound);
        else
            AudioSource.PlayClipAtPoint(sound, source.transform.position);
    }
}
