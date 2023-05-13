using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryProtoPill : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < 5.0f)
            return;

        PlanetMaterialType impactMaterial = PlanetMaterial.GetMaterialFromTransform(collision.collider.transform, collision.contacts[0].point);
        AudioClip impactSound = PlanetMaterial.GetMaterialAudio(impactMaterial, PlanetMaterialInteractionType.MediumImpact);
        GetComponent<AudioSource>().PlayOneShot(impactSound);
    }

    //YOYOYO homie swings
    //something something something? maybe
}
