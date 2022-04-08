using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioTransmission
{
    public TransmissionType transmissionType = TransmissionType.ReportingIn;
    public Squad squad = null;
    public string subtitle = "";

    public RadioTransmission(Squad squad, TransmissionType transmissionType)
    {
        this.squad = squad;
        this.transmissionType = transmissionType;
    }
}