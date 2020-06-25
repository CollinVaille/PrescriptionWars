using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedSFX : MonoBehaviour
{
    private Item item;
    private AudioSource audioSource;

    private void Start ()
    {
        item = GetComponent<Item>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1;
    }

    private void Update ()
    {
        //Time to pack up the bags
        if(item.BeingHeld())
        {
            audioSource.Stop();
            Destroy(audioSource);
            Destroy(this);
        }
    }

    private void OnCollisionEnter (Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 2)
            return;

        if (collision.gameObject.CompareTag("Terrain"))
        {
            if(PlanetTerrain.planetTerrain.GetTextureIndexAtPoint(transform.position) == 1)
                audioSource.PlayOneShot(God.god.hardItemImpact);
            else
                audioSource.PlayOneShot(God.god.softItemImpact);
        }
        else
            audioSource.PlayOneShot(God.god.hardItemImpact);
    }
}
