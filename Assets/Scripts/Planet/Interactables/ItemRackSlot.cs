using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRackSlot : Interactable
{
    public AudioClip[] swapSounds;
    
    private Item stowedItem = null;
    private string emptySlotName;
    private List<IItemRackSlotMonitor> swapEventListeners;

    private void Start()
    {
        emptySlotName = name;

        Item initiallyStowedItem = GetComponentInChildren<Item>();

        if (initiallyStowedItem)
            StowSpawnItem(initiallyStowedItem);
    }

    public override void Interact(PlanetPill interacting)
    {
        base.Interact(interacting);

        SwapItems(interacting);
    }

    private void SwapItems(PlanetPill interacting)
    {
        interacting.GetAudioSource().PlayOneShot(swapSounds[Random.Range(0, swapSounds.Length)]);

        Item toStow = interacting.GetItemInHand();
        Item previouslyStowed = stowedItem;

        //Pill unequips current item and equips stowed item
        interacting.Equip(stowedItem, false);

        //Rack forgets about stowed item and stows new item
        StowItem(toStow);

        SendSwapEventToListeners(previouslyStowed, toStow, false);
    }

    private void StowItem(Item toStow)
    {
        if (toStow)
        {
            //Stow new item...

            //Set status and name
            stowedItem = toStow;
            name = toStow.name;

            //Set parent
            toStow.transform.parent = transform;

            //Set orientation
            toStow.transform.localEulerAngles = toStow.GetRotationInItemRack();
            toStow.transform.localPosition = toStow.GetPlaceInItemRack();
        }
        else
        {
            //No new item to stow, so make slot empty again...

            //Only need to update status and name
            stowedItem = null;
            name = emptySlotName;
        }
    }

    private void StowSpawnItem(Item toStow)
    {
        //Spawn items need extra work to transition to rack...

        //Remove rigidbody
        if (toStow.GetComponent<Rigidbody>())
            Destroy(toStow.GetComponent<Rigidbody>());

        //Can no longer grab it off ground
        toStow.gameObject.layer = 0;

        //Disabling trigger collider for performance (collider used for identification when on ground)
        if (toStow.GetComponent<Collider>())
            toStow.GetComponent<Collider>().enabled = false;

        //Then perform rest of normal rack placement work...
        StowItem(toStow);

        SendSwapEventToListeners(null, toStow, true);
    }

    protected override string GetInteractionVerb() { return stowedItem ? "Retrieve" : "Stow"; }

    private void SendSwapEventToListeners(Item previouslyStowed, Item newlyStowed, bool initialSpawnEvent)
    {
        if (swapEventListeners != null)
        {
            foreach (IItemRackSlotMonitor eventListener in swapEventListeners)
            {
                if (eventListener != null)
                    eventListener.OnItemRackChange(previouslyStowed, newlyStowed, this, initialSpawnEvent);
            }
        }
    }

    public void AddSwapEventListener(IItemRackSlotMonitor swapEventListener)
    {
        if (swapEventListeners == null)
            swapEventListeners = new List<IItemRackSlotMonitor>();

        swapEventListeners.Add(swapEventListener);
    }

    public Item GetStowedItem() { return stowedItem; }
}
