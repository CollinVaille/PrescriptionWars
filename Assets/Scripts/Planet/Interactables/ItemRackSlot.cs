using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRackSlot : Interactable
{
    public AudioClip[] swapSounds;

    private Item stowedItem = null;

    private void Start()
    {
        Item initiallyStowedItem = GetComponentInChildren<Item>();

        if (initiallyStowedItem)
            StowSpawnItem(initiallyStowedItem);
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        SwapItems(interacting);
    }

    private void SwapItems(Pill interacting)
    {
        interacting.GetAudioSource().PlayOneShot(swapSounds[Random.Range(0, swapSounds.Length)]);

        Item toStow = interacting.GetItemInHand();

        //Pill unequips current item and equips stowed item
        interacting.Equip(stowedItem, false);

        //Rack forgets about stowed item and stows new item
        StowItem(toStow);
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
            name = "Empty Rack Slot";
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
    }

    protected override string GetInteractionVerb() { return stowedItem ? "Retrieve" : "Stow"; }
}
