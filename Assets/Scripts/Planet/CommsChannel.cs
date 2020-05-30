using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommsChannel : MonoBehaviour
{
    //References
    private Army army;

    //Management variables
    private Queue<RadioTranmission> commsChannel;
    private List<AudioSource> channelReceivers;
    private float pitch = 1.0f;

    //Common audio lines
    private static bool commonAudioInitialized = false;
    private static AudioClip startSound, endSound, thisIs, squad;
    private static List<AudioClip> reportingIn;

    public void InitializeCommsChannel(Army army)
    {
        //Set references
        this.army = army;

        //Initialization
        commsChannel = new Queue<RadioTranmission>();
        channelReceivers = new List<AudioSource>();

        InitializeCommonAudio();

        //Start management
        StartCoroutine(CommsChannelManager());

        //TEMPORARY
        AddChannelReceiver(GetComponent<AudioSource>());
    }

    private static void InitializeCommonAudio()
    {
        if (commonAudioInitialized)
            return;

        commonAudioInitialized = true;

        startSound = Resources.Load<AudioClip>("Radio/Common/Start Sound");
        endSound = Resources.Load<AudioClip>("Radio/Common/End Sound");
        thisIs = Resources.Load<AudioClip>("Radio/Common/This Is");
        squad = Resources.Load<AudioClip>("Radio/Common/Squad");

        reportingIn = new List<AudioClip>();
        God.InitializeAudioList(reportingIn, "Radio/Common/Reporting In ");
    }

    public void AddChannelReceiver(AudioSource newReceiver) { channelReceivers.Add(newReceiver); }

    public void Send(RadioTranmission tranmission) { commsChannel.Enqueue(tranmission); }

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
            RadioTranmission rt = commsChannel.Dequeue();

            Debug.Log("Beginning of transmission: " + rt.transmissionType);

            //Play entire audio transmission on all receivers
            yield return StartCoroutine(PlayTransmission(rt));

            Debug.Log("End of transmission: " + rt.transmissionType);
        }
    }

    private IEnumerator PlayTransmission(RadioTranmission rt)
    {
        //Start sound
        pitch = 1.0f;
        yield return new WaitForSeconds(PlayClip(startSound));

        //Set the voice pitch
        if (rt.sender.leader)
            pitch = rt.sender.leader.GetComponent<AudioSource>().pitch;

        //Play series of clips based on the type of transmission
        switch (rt.transmissionType)
        {
            case TransmissionType.ReportingIn:
                if(Random.Range(0, 2) == 0)
                    yield return new WaitForSeconds(PlayClip(thisIs));

                yield return new WaitForSeconds(PlayClip(rt.sender.Pronounce()));

                if (Random.Range(0, 2) == 0)
                    yield return new WaitForSeconds(PlayClip(squad));

                yield return new WaitForSeconds(PlayClip(God.RandomClip(reportingIn)));

                break;
        }

        //End sound
        pitch = 1.0f;
        yield return new WaitForSeconds(PlayClip(endSound));
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
}

public enum TransmissionType { ReportingIn }

public class RadioTranmission
{
    public TransmissionType transmissionType = TransmissionType.ReportingIn;
    public Squad sender = null;

    public RadioTranmission(Squad sender, TransmissionType transmissionType)
    {
        this.sender = sender;
        this.transmissionType = transmissionType;
    }
}
