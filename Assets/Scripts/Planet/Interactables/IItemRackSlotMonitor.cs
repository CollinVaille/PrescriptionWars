using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemRackSlotMonitor
{
    public void OnItemRackChange(Item previouslyStowed, Item newlyStowed, ItemRackSlot itemRackSlot, bool initialSpawnEvent);
}
