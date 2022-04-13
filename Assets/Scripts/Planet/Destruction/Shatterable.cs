using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shatterable : MonoBehaviour, Damageable
{
    public AudioClip dent, shatter;
    public Material dentMaterial;
    public GameObject shardPrefab;

    private AudioSource sfxSource = null;
    private Renderer dentRenderer;

    [Tooltip("Shards created = rows (X) * columns (Y).")] public Vector2Int fractures = Vector2Int.one * 3;
    public float health = 350;
    private float maxHealth;
    private bool pristine = true, shattered = false;

    private void Start()
    {
        maxHealth = health;
    }

    public void Damage(float damage, float knockback, Vector3 from, DamageType damageType, int team)
    {
        if (shattered)
            return;

        health -= damage;

        if (health <= 0)
            Shatter(knockback, from);
        else if (pristine)
            CreateDent(damage);
        else
            UpdateDent(damage);
    }

    private void CreateDent(float damage)
    {
        pristine = false;

        //Create dent
        Transform newDent = (new GameObject()).transform;
        newDent.name = name + " Dent";

        //Set transform of dent
        newDent.parent = transform;
        newDent.localPosition = Vector3.zero;
        newDent.localEulerAngles = Vector3.zero;
        newDent.localScale = Vector3.one * 1.01f;

        //Set mesh and renderer of dent
        newDent.gameObject.AddComponent<MeshFilter>().sharedMesh = transform.GetComponent<MeshFilter>().sharedMesh;
        dentRenderer = newDent.gameObject.AddComponent<MeshRenderer>();
        dentRenderer.material = dentMaterial;

        UpdateDent(damage);
    }

    private void UpdateDent(float damage)
    {
        PlaySound(dent, damage / 50.0f, false);

        Color dentColor = dentRenderer.material.color;
        dentColor.a = 1.0f - health / maxHealth;
        dentRenderer.material.color = dentColor;
    }

    private void Shatter(float knockback, Vector3 from)
    {
        shattered = true;

        PlaySound(shatter, 1.0f, true);

        int rows = Mathf.Max(fractures.x, 1);
        int columns = Mathf.Max(fractures.y, 1);

        Vector3 shardScale = new Vector3(1.0f / columns, 1.0f / rows, 0.25f);
        float startX = -0.5f + shardScale.x / 2.0f;
        float startY = -0.5f + shardScale.y / 2.0f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Transform shard = Instantiate(shardPrefab, transform).transform;

                //Calculate position
                float xPos = startX + (x * 1.0f) / columns;
                float yPos = startY + (y * 1.0f) / rows;

                //Apply position and rotation
                if(transform.localScale.x > transform.localScale.z) //Normal case where x-axis is used as width
                {
                    shard.localPosition = new Vector3(xPos, yPos, 0.0f);
                    shard.localEulerAngles = Vector3.zero;
                }
                else //Case where z-axis is used as width
                {
                    shard.localPosition = new Vector3(0.0f, yPos, xPos);
                    shard.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
                }   

                shard.localScale = shardScale;
                shard.transform.parent = null;

                shard.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().sharedMaterial;

                shard.GetComponent<Rigidbody>().AddExplosionForce(knockback, from, 1000);
            }
        }

        //If there's a larger body this shatterable is a part of that would be comprised if this was shattered, then report "Hull Compromised" to it so it can handle its own damage
        //For instance if this was a glass case for a stasis pod, then shatter the case and report "Hull Compromised" to the stasis pod so it can handle the cascading destruction, i.e....
        //...water spilling out, rest of parts falling to ground, destruction of stasis pod object
        Damageable compromisedOnShatter = transform.parent.GetComponentInParent<Damageable>();
        if (compromisedOnShatter != null)
        {
            //So the parent object doesn't try to mess with this object which is about to be destroyed
            if (transform.parent)
                transform.parent = null;

            compromisedOnShatter.Damage(1, 1, transform.position, DamageType.HullCompromised, -9000);
        }

        Destroy(gameObject);
    }

    private void PlaySound(AudioClip sound, float volume, bool finalSound)
    {
        if(finalSound)
        {
            AudioSource.PlayClipAtPoint(sound, transform.position, volume);
            return;
        }

        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.spatialBlend = 1.0f;
            sfxSource.maxDistance = 150.0f;
        }

        sfxSource.PlayOneShot(sound, volume);
    }
}
