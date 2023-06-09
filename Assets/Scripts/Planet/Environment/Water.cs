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
        PlanetPill pill = entering.GetComponent<PlanetPill>();

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
                    PlaySound(majorSplashes[Random.Range(0, majorSplashes.Length)], rBody.gameObject);
                else if (rBody.velocity.magnitude > 4)
                    PlaySound(minorSplashes[Random.Range(0, minorSplashes.Length)], rBody.gameObject);
            }

            if (entering.CompareTag("Buoyant"))
                StartCoroutine(FloatToSurface(rBody, entering));
        }
    }

    private void OnTriggerExit (Collider exiting)
    {
        //Get pill
        PlanetPill pill = exiting.GetComponent<PlanetPill>();

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

            PlaySound(exits[Random.Range(0, exits.Length)], rBody.gameObject);
        }
    }

    private IEnumerator FloatToSurface(Rigidbody buoyantBody, Collider buoyantCollider)
    {
        float surfaceLevel = transform.GetComponent<Collider>().bounds.max.y;

        //Debug.Log("Floating " + buoyantBody.name + " to surface. (" + name + ")");

        buoyantBody.useGravity = false;

        //Float loop (go until vehicle emerges from water)
        while(buoyantCollider.bounds.min.y <= surfaceLevel)
        {
            //Raise to just below surface
            while (buoyantCollider.bounds.min.y <= surfaceLevel - 0.35f)
            {
                buoyantBody.MovePosition(buoyantBody.position + Vector3.up * Time.deltaTime);
                yield return null;
            }

            //Steady vertical velocity to near zero unless being slightshot or something crazy
            Vector3 bodyVelocity = buoyantBody.velocity;
            bodyVelocity.y /= 3.0f;
            buoyantBody.velocity = bodyVelocity;

            //Maintain height just below surface so we don't have to keep bobbing up/down
            while (ValueWithin(buoyantCollider.bounds.min.y, surfaceLevel - 0.5f, surfaceLevel))
            {
                //Simulate slight/natural bobbing
                buoyantBody.AddForce(Vector3.up * Random.Range(-0.1f, 0.1f));

                yield return new WaitForSeconds(0.2f);
            }
        }

        buoyantBody.useGravity = true;

        //Debug.Log("Done.");
    }

    private void PlaySound (AudioClip sound, GameObject source)
    {
        if (source.GetComponent<AudioSource>())
            source.GetComponent<AudioSource>().PlayOneShot(sound);
        else
            AudioSource.PlayClipAtPoint(sound, source.transform.position);
    }

    private bool ValueWithin(float value, float min, float max) { return value >= min && value <= max; }
}
