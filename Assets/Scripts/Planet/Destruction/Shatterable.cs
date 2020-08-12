using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shatterable : MonoBehaviour, Damageable
{
    public AudioClip dent, shatter;
    public GameObject dentPrefab, shardPrefab;

    private AudioSource sfxSource = null;
    private Renderer dentRenderer;

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

        Transform newDent = Instantiate(dentPrefab, transform).transform;
        newDent.localPosition = Vector3.zero;
        newDent.localEulerAngles = Vector3.zero;
        newDent.localScale = Vector3.one * 1.01f;
        dentRenderer = newDent.GetComponent<Renderer>();

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

        int columns = 3, rows = 3;

        Vector3 shardScale = new Vector3(1.0f / columns, 1.0f / rows, 0.25f);
        float startX = -0.5f + shardScale.x / 2.0f;
        float startY = -0.5f + shardScale.y / 2.0f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Transform shard = Instantiate(shardPrefab, transform).transform;

                shard.localPosition = new Vector3(startX + (x * 1.0f) / columns, startY + (y * 1.0f) / rows, 0.0f);

                shard.localScale = shardScale;
                shard.localEulerAngles = Vector3.zero;
                shard.transform.parent = null;

                shard.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().sharedMaterial;

                shard.GetComponent<Rigidbody>().AddExplosionForce(knockback, from, 1000);
            }
        }

        Destroy(gameObject);
    }

    private void PlaySound(AudioClip sound, float volume, bool finalSound)
    {
        if(!sfxSource)
        {
            if (finalSound)
            {
                AudioSource.PlayClipAtPoint(sound, transform.position, volume);
                return;
            }

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.spatialBlend = 1.0f;
            sfxSource.maxDistance = 150.0f;
        }

        sfxSource.PlayOneShot(sound, volume);
    }
}
