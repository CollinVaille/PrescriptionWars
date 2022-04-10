using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommsChannel : MonoBehaviour
{
    //References
    private Army army;

    //Management variables
    private Queue<RadioTransmission> commsChannel;
    private List<AudioSource> channelReceivers;
    private Text subtitle;
    private float pitch = 1.0f;

    public void InitializeCommsChannel(Army army)
    {
        //Set references
        this.army = army;

        //Initialization
        commsChannel = new Queue<RadioTransmission>();
        channelReceivers = new List<AudioSource>();
        subtitle = PlanetPauseMenu.pauseMenu.HUD.Find("Radio Subtitles").GetComponent<Text>();

        RadioCommonAudio.InitializeCommonAudio();

        //Start management
        StartCoroutine(CommsChannelManager());

        //TEMPORARY
        AddChannelReceiver(GetComponent<AudioSource>());
    }

    public void AddChannelReceiver(AudioSource newReceiver) { channelReceivers.Add(newReceiver); }

    public void Send(RadioTransmission tranmission) { commsChannel.Enqueue(tranmission); }

    private IEnumerator CommsChannelManager()
    {
        //Wait for things to settle down
        yield return new WaitForSeconds(Random.Range(3.0f, 5.0f));

        //Main loop: always up, waiting to process new incoming transmissions
        while (true)
        {
            //Random delay between transmissions
            yield return new WaitForSeconds(Random.Range(0.25f, 1.0f));

            //Wait for a new message to appear
            yield return new WaitWhile(() => commsChannel.Count == 0);

            //Get the next message
            RadioTransmission rt = commsChannel.Dequeue();

            Debug.Log("Beginning of transmission: " + rt.transmissionType);

            //Play entire audio transmission on all receivers
            yield return StartCoroutine(PlayTransmission(rt));

            Debug.Log("End of transmission: " + rt.transmissionType);
        }
    }

    private IEnumerator PlayTransmission(RadioTransmission rt)
    {
        CommsPersonality commsPersonality = rt.squad.GetCommsPersonality();

        //Start sound
        pitch = 1.0f;
        yield return new WaitForSeconds(PlayClip(RadioCommonAudio.startSound));

        //Set the voice pitch
        if (rt.squad.leader)
            pitch = commsPersonality.pitch;

        List<RadioClip> radioClips = RadioTransmissionLogic.DecodeTransmission(rt);
        foreach(RadioClip radioClip in radioClips)
        {
            Pill broadcaster = rt.squad.leader;
            if(broadcaster && !broadcaster.IsDead())
            {
                //Play clip
                SetSubtitleText(rt.squad.name + ": " + rt.subtitle);
                float duration = PlayClip(radioClip.audioClip);

                //Wait for clip to end or broadcaster to die
                for (float t = 0.0f; t < duration; t += Time.deltaTime)
                {
                    if (!broadcaster || broadcaster.IsDead())
                        break;

                    yield return null;
                }
            }

            //If broadcaster died during radio transmission, air his death thrawls on air and then play the flatline exit sound
            if (!broadcaster || broadcaster.IsDead())
            {
                if(broadcaster)
                {
                    AudioSource dyingBroadcaster = broadcaster.GetAudioSource();
                    if(dyingBroadcaster.isPlaying && dyingBroadcaster.clip)
                    {
                        //Death throws
                        float duration = PlayClip(dyingBroadcaster.clip);
                        yield return new WaitForSeconds(duration);

                        //Flatline
                        duration = PlayClip(RadioCommonAudio.flatline);
                        yield return new WaitForSeconds(duration);
                    }
                }

                //Prematurely done with transmission
                break;
            }

            //Silence between clips
            radioClip.pauseDurationAfter *= Random.Range(commsPersonality.pauseLengthMultiplier.x, commsPersonality.pauseLengthMultiplier.y);
            yield return new WaitForSeconds(radioClip.pauseDurationAfter);
        }

        //Clear subtitles
        SetSubtitleText("");

        //End sound
        pitch = 1.0f;
        yield return new WaitForSeconds(PlayClip(RadioCommonAudio.endSound));
    }

    private float PlayClip(AudioClip clip)
    {
        foreach (AudioSource receiver in channelReceivers)
        {
            if (receiver.isPlaying)
                receiver.Stop();

            receiver.pitch = pitch;
            receiver.clip = clip;
            receiver.Play();
        }

        return clip.length / pitch;
    }

    private void SetSubtitleText(string subtitleText)
    {
        if(subtitleText != null && !subtitleText.Equals(""))
        {
            subtitle.text = subtitleText;
            subtitle.enabled = true;
        }
        else
        {
            subtitle.text = "";
            subtitle.enabled = false;
        }
    }
}