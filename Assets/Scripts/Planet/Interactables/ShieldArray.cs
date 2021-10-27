using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldArray : Interactable
{
    //Status variables
    private bool transitioning = false, shouldBeOn = true;
    private int workerCode = 0;
    private Dictionary<Transform, AudioSource> shieldSoundSources;

    //Customization
    public AudioClip onSound, offSound;
    public Transform[] shields;

    public override void Interact(Pill interacting, bool turnOn)
    {
        //Already got the memo
        if (shouldBeOn == turnOn)
            return;

        //Indicate new desired state
        shouldBeOn = turnOn;

        //Start new worker if needed
        if(!transitioning)
        {
            transitioning = true;
            StartCoroutine(RefreshShields());
        }
    }

    private IEnumerator RefreshShields()
    {
        int workerKey = ++workerCode;

        List<Transform> shieldsLeftToProcess = new List<Transform>(shields);
        bool tryingToTurnThemOn = shouldBeOn;

        //Iterate through all shields and refresh each
        while(shieldsLeftToProcess.Count != 0)
        {
            //Pop next shield off
            Transform nextShield = shieldsLeftToProcess[Random.Range(0, shieldsLeftToProcess.Count)];
            shieldsLeftToProcess.Remove(nextShield);

            //Process it
            SetShieldState(nextShield, tryingToTurnThemOn);

            //Wait
            yield return new WaitForSeconds(Random.Range(0.1f, 0.15f));

            //Start from the beginning again if the level has been flicked before we finished
            if(tryingToTurnThemOn != shouldBeOn)
            {
                tryingToTurnThemOn = shouldBeOn;
                shieldsLeftToProcess = new List<Transform>(shields);
            }
        }

        transitioning = false;

        //Wait for all sounds to stop playing
        yield return new WaitForSeconds(5);

        //Clean up at the end if nothing has happened in the meantime. Else, let the worker that is replacing us do the clean up since we don't want to jam his flow.
        if(workerKey == workerCode)
            CleanUpAllAudioSources();
    }

    private void SetShieldState(Transform shield, bool setToOn)
    {
        if (!shield)
            return;

        shield.gameObject.SetActive(setToOn);

        if(Vector3.Distance(Player.player.transform.position, shield.position) < 50)
        {
            if (setToOn)
                PlaySoundForShield(shield, onSound);
            else
                PlaySoundForShield(shield, offSound);
        }
    }

    private void PlaySoundForShield(Transform shield, AudioClip sound)
    {
        if (shieldSoundSources == null)
            shieldSoundSources = new Dictionary<Transform, AudioSource>();

        //See sfx source already exists
        shieldSoundSources.TryGetValue(shield, out AudioSource sfxSource);

        //Else, create a new one
        if (!sfxSource)
            sfxSource = CreateAudioSourceFor(shield);

        //Play sound from sfx source
        sfxSource.Stop();
        sfxSource.clip = sound;
        sfxSource.Play();
    }

    private AudioSource CreateAudioSourceFor(Transform shield)
    {
        AudioSource sfxSource = new GameObject().AddComponent<AudioSource>();

        //Edit transform
        sfxSource.transform.position = shield.position;
        sfxSource.transform.parent = transform.parent;

        //Edit sfx source
        sfxSource.spatialBlend = 1.0f;
        sfxSource.volume = 1.0f;

        //Register sfx source (so we can manage and get rid of it later)
        God.god.ManageAudioSource(sfxSource);
        shieldSoundSources.Add(shield, sfxSource);

        return sfxSource;
    }

    private void CleanUpAllAudioSources()
    {
        if (shieldSoundSources == null)
            return;

        foreach(AudioSource sfxSource in shieldSoundSources.Values)
        {
            sfxSource.Stop();
            God.god.UnmanageAudioSource(sfxSource);
            Destroy(sfxSource.gameObject);
        }

        shieldSoundSources.Clear();
    }
}
