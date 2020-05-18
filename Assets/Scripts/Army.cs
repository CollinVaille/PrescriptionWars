using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour
{
    private static List<Army> armies;

    //Army composition
    public List<Squad> squads;
    public Pill fieldGeneral;
    
    //Faction allegience
    public int team = 0;

    //Comms channel
    private Queue<RadioTranmission> commsChannel;
    private List<AudioSource> channelReceivers;

    public void Start ()
    {
        //Initialize comms channel
        StartCoroutine(CommsChannelManager());

        //When done with set up, make army available for referencing
        if (armies == null)
            armies = new List<Army>();
        armies.Add(this);
    }

    public static Army GetArmy (int team)
    {
        Army army = null;

        for(int x = 0; x < armies.Count; x++)
        {
            if(armies[x].team == team)
            {
                army = armies[x];
                break;
            }
        }

        return army;
    }

    public void AddSquad ()
    {

    }

    private IEnumerator CommsChannelManager ()
    {
        //Initialization
        commsChannel = new Queue<RadioTranmission>();
        channelReceivers = new List<AudioSource>();

        //Main loop: always up, waiting to process new incoming transmissions
        while (true)
        {
            //Wait for a new message to appear
            yield return new WaitWhile(() => commsChannel.Count == 0);

            //Get the next message
            RadioTranmission rt = commsChannel.Dequeue();

            //Broadcast message
            foreach(AudioSource receiver in channelReceivers)
            {
                if (receiver.isPlaying)
                    receiver.Stop();

                //receiver.clip = 
                receiver.Play();
            }

            //Process message
            Debug.Log(rt.transmissionType);
        }
    }

    public void AddChannelReceiver (AudioSource newReceiver) { channelReceivers.Add(newReceiver); }

    public void SendTransmission (RadioTranmission rt) { commsChannel.Enqueue(rt); }
}

public class RadioTranmission
{
    public enum TransmissionType { ReportingIn }

    public TransmissionType transmissionType = TransmissionType.ReportingIn;
    public int squadCode = -1;

    public RadioTranmission (TransmissionType transmissionType, int squadCode)
    {
        this.transmissionType = transmissionType;
        this.squadCode = squadCode;
    }
}