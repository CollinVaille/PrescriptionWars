using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTransmission : RadioTransmission
{
    public string customMessage;

    public CustomTransmission(Squad squad, string customMessage) : base(squad, TransmissionType.CustomMessage)
    {
        this.customMessage = customMessage;
    }
}
