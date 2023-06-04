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
    CommsPersonality commsPersonality;

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

            //Play entire audio transmission on all receivers
            yield return StartCoroutine(PlayTransmission(rt));
        }
    }

    private IEnumerator PlayTransmission(RadioTransmission rt)
    {
        commsPersonality = rt.squad.GetCommsPersonality();

        //Start sound
        yield return new WaitForSeconds(PlayClip(RadioCommonAudio.startSound, true));

        List<RadioClip> radioClips = RadioTransmissionLogic.DecodeTransmission(rt);
        foreach(RadioClip radioClip in radioClips)
        {
            //Play the radio clip
            Pill broadcaster = rt.squad.leader;
            if(!InterruptionDetected(broadcaster))
            {
                //Play clip
                SetSubtitleText(rt.squad.name + ": " + rt.subtitle);
                float duration = PlayClip(radioClip.audioClip);

                //Wait for clip to end or broadcaster to die
                for (float t = 0.0f; t < duration; t += Time.deltaTime)
                {
                    if (InterruptionDetected(broadcaster))
                        break;

                    yield return null;
                }
            }

            //Special case handling for when radio clip is interrupted
            if (InterruptionDetected(broadcaster))
            {
                //If broadcaster dies while on air, air his death thrawls, play the flatline exit sound, followed by a courtious automated voice message
                if (broadcaster.IsDead)
                    yield return StartCoroutine(PlayDeathInterruption(rt, broadcaster));

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
        yield return new WaitForSeconds(PlayClip(RadioCommonAudio.endSound, true));
    }

    private bool InterruptionDetected(Pill broadcaster)
    {
        return broadcaster.IsDead;
    }

    private IEnumerator PlayDeathInterruption(RadioTransmission rt, Pill broadcaster)
    {
        AudioSource dyingBroadcaster = broadcaster.GetAudioSource();
        float duration;

        //Death throws
        if (dyingBroadcaster.isPlaying && dyingBroadcaster.clip)
        {
            SetSubtitleText(rt.squad.name + ": *Dies*");
            duration = PlayClip(dyingBroadcaster.clip);
            yield return new WaitForSeconds(duration);
        }

        //Flatline
        SetSubtitleText("*Connection Lost*");
        duration = PlayClip(Resources.Load<AudioClip>("Planet/Radio/Mechanical/Flatline"), true);
        yield return new WaitForSeconds(duration);

        //Name of corresponder
        SetSubtitleText("Voicemail: " + rt.squad.Pronounce() + " is not available to complete the call. This transmission will now end. Thank you for your patience.");
        List<RadioClip> radioClips = RadioWordPronounciation.PronounceWords(rt.squad.name, out _);
        foreach(RadioClip radioClip in radioClips)
        {
            duration = PlayClip(radioClip.audioClip);
            yield return new WaitForSeconds(duration);
        }

        //Voicemail
        duration = PlayClip(Resources.Load<AudioClip>("Planet/Radio/Mechanical/Death Voicemail"), true);
        yield return new WaitForSeconds(duration);
    }

    private float PlayClip(AudioClip clip, bool overridePersonalityWithGeneric = false)
    {
        float pitch = overridePersonalityWithGeneric ? 1.0f : commsPersonality.pitch;

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