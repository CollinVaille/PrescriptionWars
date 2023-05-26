using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Pill
{
    private enum POV { FirstPerson, ThirdPerson, ThirdPerson360 }
    private enum Dir { Left, Center, Right }

    public static Player player;
    public static int playerTeam = 0;

    //References
    private Transform head, cameraTransform;
    private GameObject interactOption;
    private Image healthBar, reticle, underwaterVisual;
    private Text itemInfo;
    private AudioSource feet, underwaterAmbience;
    private AudioReverbZone underwaterReverb;
    private Item droppedOnDeath = null;
    private AudioReverbPreset outdoorReverbPreset;
    public GameObject mapMarkerPrefab;

    //Customization
    public float rotationSpeed = 90;
    public float healthRegen = 5;
    private GameObject headGear = null, bodyGear = null;
    public AudioClip ricochet, gearRicochet, hitMarker, hitArmorMarker, flying;

    //Status variables
    private POV pov, povBefore3rdPerson;
    private Dir thirdPersonHorCamPos;
    private Item sidearm = null;
    private bool soaring = false, continuousPrimaryAction = false, sprinting = false;
    private int indoorZoneCount = 0;

    //Coroutine keys
    private int itemInfoFlashCode = 0;

    //Item bob variables
    private float bobInput = 0;

    //Feet sound stuff
    private PlanetMaterialType lastMaterialSteppedOn = PlanetMaterialType.NoMaterial;
    private AudioClip walking, running;
    private Collider submersedIn = null;

    //Commands
    private Squad.Orders order1, order2, order3;

    //Initialization
    protected override void Start ()
    {
        base.Start();
        
        //Set references
        head = transform.Find("Head");
        cameraTransform = head.Find("Camera");
        healthBar = PlanetPauseMenu.pauseMenu.HUD.Find("Health Bar").GetComponent<Image>();
        reticle = PlanetPauseMenu.pauseMenu.HUD.Find("Reticle").GetComponent<Image>();
        underwaterVisual = PlanetPauseMenu.pauseMenu.HUD.Find("Underwater").GetComponent<Image>();
        itemInfo = PlanetPauseMenu.pauseMenu.HUD.Find("Item Info").GetComponent<Text>();
        feet = GetComponents<AudioSource>()[1];
        underwaterAmbience = GetComponents<AudioSource>()[2];
        underwaterReverb = GetComponent<AudioReverbZone>();

        //Initialize POV
        thirdPersonHorCamPos = Dir.Center;
        povBefore3rdPerson = POV.FirstPerson;
        SetPOV(POV.FirstPerson);

        //Initialize other player settings
        mainAudioSource.spatialBlend = 0;
        feet.clip = walking;

        ApplyDisplaySettings();

        //Create marker for player on the planet map
        MapMarker mapMarker = Instantiate(mapMarkerPrefab).GetComponent<MapMarker>();
        mapMarker.InitializeMarker(transform);

        //Load in later
        order1 = Squad.Orders.Follow;
        order2 = Squad.Orders.FormSquare;
        order3 = Squad.Orders.Roam;

        //Make audio sources able to be paused by pause menu
        God.god.ManageAudioSource(feet);
        God.god.ManageAudioSource(underwaterAmbience);

        //Start coroutines
        StartCoroutine(ManageInteractOption());
        StartCoroutine(CameraOrbit());
        StartCoroutine(ManageFootstepsMaterial());
    }

    //Update player controls and effects
    private void Update ()
    {
        if (Time.timeScale == 0 || controlOverride)
            return;

        //Flying sfx
        if (!touchingWater && !IsGrounded(5.0f) && !soaring && rBody.velocity.magnitude > 35)
            StartCoroutine(FlyingSFX());

        if (dead)
        {
            //Still have limit controls when dead
            POVInputUpdate();
            DeadMovementInputUpdate();

            return;
        }

        RotationInputUpdate();
        TranslationInputUpdate();
        MovementEffectsUpdate();

        //Remainder of controls only respond on initial frame of being pressed down
        if (!Input.anyKeyDown && !continuousPrimaryAction)
            return;

        ItemInputUpdate();
        POVInputUpdate();
        SquadInputUpdate();
    }

    //Rotate player based on input
    private void RotationInputUpdate ()
    {
        //Rotation... can't rotate when in 360 view because mouse movement is being used by 360 camera
        if(pov != POV.ThirdPerson360)
        {
            //Rotate body left/right
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime);

            //Rotate head down/up
            head.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime, Space.Self);
            
            //Put limits on head rotation
            if(head.localEulerAngles.x > 180)
            {
                if (head.localEulerAngles.x < 300) //Lower limit
                    head.localEulerAngles = new Vector3(300, 0, 0);
            }
            else if (head.localEulerAngles.x > 60) //Lower limit
                head.localEulerAngles = new Vector3(60, 0, 0);
        }
    }

    //Move player based on input
    private void TranslationInputUpdate()
    {
        //Sprinting
        if (Input.GetButtonDown("Sprint"))
            SetSprinting(true);
        else if (Input.GetButtonUp("Sprint"))
            SetSprinting(false);

        //Left/right movement
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed * 0.5f * Time.deltaTime, Space.Self);

        //Backward/forward movement
        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime, Space.Self);

        //Jump
        if (Input.GetButtonDown("Jump"))
            Jump();
    }

    //Make item look like it is slightly moving in hand and update feet sound
    private void MovementEffectsUpdate ()
    {
        Vector3 holdingPlace = Vector3.zero;
        if(holding)
            holdingPlace = holding.transform.localPosition;

        if (IsGrounded(0.2f) || touchingWater) //Normal item bob
        {
            if (Input.GetAxis("Vertical") != 0) //Must be moving forward/backward to item bob
            {
                if(holding)
                {
                    //Calculate item bob
                    bobInput += Time.deltaTime * moveSpeed;
                    holdingPlace.y = -0.25f + 0.05f * Mathf.Sin(bobInput);

                    //Apply item bob
                    holding.transform.localPosition = holdingPlace;
                }

                if (!feet.isPlaying)
                    feet.Play();
            }
            else //Otherwise, just reset item position
            {
                if(holding)
                {
                    holdingPlace.y = -0.25f;
                    holding.transform.localPosition = holdingPlace;
                }

                if (feet.isPlaying)
                    feet.Pause();
            }
        }
        else //Lift weapon all the way up when in air
        {
            //Raise
            if(holding)
            {
                holdingPlace.y += 0.5f * Time.deltaTime;

                //Enforce limit
                if (holdingPlace.y > -0.1f)
                    holdingPlace.y = -0.1f;

                //Apply
                holding.transform.localPosition = holdingPlace;
            }

            if (feet.isPlaying)
                feet.Pause();
        }
    }

    //Item controls
    private void ItemInputUpdate ()
    {
        //Manage adding/removing items from hand
        if (Input.GetButtonDown("Equip")) //Hand-to-(ground or item rack) swapping
        {
            if (interactOption && interactOption.GetComponent<Item>()) //Pick up item from ground
            {
                Equip(interactOption.GetComponent<Item>());
                EraseInteractOption(); //"Used up" the option
            }
            else if(interactOption && interactOption.GetComponent<ItemRackSlot>()) //Pick up item from item rack
                interactOption.GetComponent<Interactable>().Interact(GetPill());
            else if(!controlOverride) //Just unequip current item (but not when we're in some special control state because often times its an acident then)
                Equip(null);
        }
        else if(Input.GetButtonDown("Interact")) //Button used for general interaction with the world
        {
            if(interactOption && interactOption.GetComponent<Interactable>())
                    interactOption.GetComponent<Interactable>().Interact(GetPill());
        }
        else if (Input.GetButtonDown("Sidearm")) //Hand/sidearm swapping
            SwapToSidearm();

        //Rest of function is for using the item in hand...
        if (!holding)
            return;

        //Item actions
        if (Input.GetButtonDown("Stab"))
            StartCoroutine(holding.ExpensiveStab(0.35f, Vector3.zero, holding.swoosh));
        else if (Input.GetButtonDown("Tertiary Action"))
            holding.TertiaryAction();
        else if (Input.GetButtonDown("Secondary Action"))
            holding.SecondaryAction();
        else if (Input.GetButton("Primary Action"))
        {
            if (continuousPrimaryAction || Input.GetButtonDown("Primary Action"))
                holding.PrimaryAction();
        }
    }

    //POV controls
    private void POVInputUpdate ()
    {
        if (Input.GetButtonDown("Third Person")) //Toggle 3rd person (center)
        {
            if (pov == POV.ThirdPerson)
                SetPOV(POV.FirstPerson);
            else
                SetPOV(POV.ThirdPerson);
        }
        else if (Input.GetButtonDown("360 View")) //Toggle 360 view
        {
            if (pov == POV.ThirdPerson360)
                SetPOV(POV.FirstPerson);
            else
                SetPOV(POV.ThirdPerson360);
        }
        else if (Input.GetButtonDown("Left Third Person")) //Toggle 3rd person left
        {
            if (pov == POV.ThirdPerson && thirdPersonHorCamPos == Dir.Left)
                SetPOV(povBefore3rdPerson);
            else
                GoToThirdPerson(Dir.Left);
        }
        else if (Input.GetButtonDown("Right Third Person")) //Toggle 3rd person right
        {
            if (pov == POV.ThirdPerson && thirdPersonHorCamPos == Dir.Right)
                SetPOV(povBefore3rdPerson);
            else
                GoToThirdPerson(Dir.Right);
        }
    }

    //Squad controls
    private void SquadInputUpdate ()
    {
        if (squad == null)
            return;

        if(squad.leader == GetPill()) //Leader commands
        {
            if (Input.GetButtonDown("Order 1"))
                squad.SetOrders(order1);
            else if (Input.GetButtonDown("Order 2"))
                squad.SetOrders(order2);
            else if (Input.GetButtonDown("Order 3"))
                squad.SetOrders(order3);
        }
    }

    //Limited movement available when dead
    private void DeadMovementInputUpdate ()
    {
        //Rotation... can't rotate when in 360 view because mouse movement is being used by 360 camera
        if (pov != POV.ThirdPerson360)
        {
            //Rotate body left/right
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime);

            //Rotate head down/up
            head.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime, Space.Self);

            //Put limits on head rotation
            if (head.localEulerAngles.x > 180)
            {
                if (head.localEulerAngles.x < 300) //Lower limit
                    head.localEulerAngles = new Vector3(300, 0, 0);
            }
            else if (head.localEulerAngles.x > 60) //Lower limit
                head.localEulerAngles = new Vector3(60, 0, 0);
        }
    }

    public override void Damage (float damage, float knockback, Vector3 from, DamageType damageType, int team)
    {
        base.Damage(damage, knockback, from, damageType, team);

        //Display player's health after damage
        Vector3 healthBarScale = healthBar.GetComponent<RectTransform>().localScale;
        healthBarScale.x = health / maxHealth;
        healthBar.GetComponent<RectTransform>().localScale = healthBarScale;
    }

    public override void Heal (float amount, bool fromExternalSource)
    {
        base.Heal(amount, fromExternalSource);

        //Display player's health after heal
        Vector3 healthBarScale = healthBar.GetComponent<RectTransform>().localScale;
        healthBarScale.x = health / maxHealth;
        healthBar.GetComponent<RectTransform>().localScale = healthBarScale;
    }

    //Should only be called by Damage function
    protected override void Die ()
    {
        base.Die();

        //Drop held item
        Equip(null);

        //Fall down
        rBody.constraints = RigidbodyConstraints.None;

        //Not on feet anymore so no footsteps
        feet.Stop();
    }

    public override void Equip (Item item, bool dropOldItem = true)
    {
        Item oldHolding = holding;

        //Do the item switching
        base.Equip(item, dropOldItem);

        //Set up new item
        if (holding)
        {
            //Positioning
            holding.transform.parent = head;
            holding.transform.localPosition = holding.GetPlaceInPlayerHand();
            holding.transform.localRotation = Quaternion.Euler(0, 0, 0);

            FlashItemInfo();
        }
        else
            itemInfo.text = "";

        //Finish discarding old item
        if (oldHolding && dropOldItem)
        {
            oldHolding.gameObject.AddComponent<DroppedSFX>();

            if (dead) //Make sure no one can take our item right now, we'll be back for it on respawn
            {
                droppedOnDeath = oldHolding;
                oldHolding.gameObject.layer = 0;
            }
        }
    }

    public override void Equip (Item primary, Item secondary)
    {
        Equip(secondary);
        SwapToSidearm();
        Equip(primary);
    }

    private IEnumerator ManageInteractOption ()
    {
        Text interactText = PlanetPauseMenu.pauseMenu.HUD.Find("Interactable Text").GetComponent<Text>();
        int interactMask = (1 << 10) | (1 << 14); //Collide with interactable (10) or good bot (14) layers

        //Continually update the interact option
        while(true)
        {
            //Check in front of us for objects in interactable layer (layer 10)
            Vector3 from = head.TransformPoint(Vector3.back * 0.25f);
            if(Physics.SphereCast(from, 0.25f, head.forward, out RaycastHit hit, 2.25f, interactMask, QueryTriggerInteraction.Collide))
            {
                //Found an interactable object so add/display it as the option
                interactOption = hit.collider.gameObject;
                Interactable interactable = interactOption.GetComponent<Interactable>();
                interactText.text = interactable ? interactable.GetInteractionDescription() : "Grab " + interactOption.name;
            }
            else if(interactOption) //Didn't find anything so erase any previous interact option
            {
                interactOption = null;
                interactText.text = "";
            }

            //Wait
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EraseInteractOption ()
    {
        interactOption = null;
        PlanetPauseMenu.pauseMenu.HUD.Find("Interactable Text").GetComponent<Text>().text = "";
    }

    private void SwapToSidearm ()
    {
        //Reference swap
        Item newSidearm = holding;
        holding = sidearm;
        sidearm = newSidearm;

        //Adjust new sidearm
        if(newSidearm)
        {
            newSidearm.transform.parent = transform;

            //New position and rotation
            newSidearm.transform.localPosition = newSidearm.GetPlaceOnBack();
            newSidearm.transform.localEulerAngles = newSidearm.GetRotationOnBack();

            newSidearm.RetireFromHand();
        }

        //Adjust new holding
        if (holding)
        {
            holding.transform.parent = head;
            holding.transform.localPosition = holding.GetPlaceInPlayerHand();
            holding.transform.localRotation = Quaternion.Euler(0, 0, 0);

            holding.PutInHand(GetPill());

            FlashItemInfo();
        }
        else
            itemInfo.text = "";
    }

    private void SetPOV (POV newPOV)
    {
        if (newPOV == POV.FirstPerson)
        {
            pov = POV.FirstPerson;

            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.Euler(0, 0, 0);

            //Hide gear so it doesn't block view
            UpdateGearVisibility();
        }
        else if (newPOV == POV.ThirdPerson)
            GoToThirdPerson(Dir.Center);
        else //Third person 360
        {
            pov = POV.ThirdPerson360;

            cameraTransform.localPosition = new Vector3(0.0f, 0.0f, -3.5f);
            cameraTransform.localRotation = Quaternion.Euler(0, 0, 0);

            //Make sure we can see that glorious gear
            UpdateGearVisibility();
        }
    }

    private void GoToThirdPerson (Dir horizontalCameraPosition)
    {
        povBefore3rdPerson = pov;
        pov = POV.ThirdPerson;

        thirdPersonHorCamPos = horizontalCameraPosition;

        //Positioning
        if(horizontalCameraPosition == Dir.Left)
            cameraTransform.localPosition = new Vector3(-1.5f, 1.5f, -3.5f);
        else if (horizontalCameraPosition == Dir.Center)
            cameraTransform.localPosition = new Vector3(0, 1.5f, -3.5f);
        else //Right
            cameraTransform.localPosition = new Vector3(1.5f, 1.5f, -3.5f);

        //Rotation
        cameraTransform.localRotation = Quaternion.Euler(5, 0, 0);

        //Make sure we can see that glorious gear
        UpdateGearVisibility();
    }

    private IEnumerator CameraOrbit ()
    {
        Vector3 cameraMovement = Vector3.zero;

        float localZPosition = Mathf.Abs(cameraTransform.localPosition.z);

        float minZ = 2, maxZ = 10;

        //Update camera position and rotation every frame
        while (true)
        {
            //Until next frame
            yield return null;

            //Only move camera when playing in 360 view
            if (pov != POV.ThirdPerson360 || Time.timeScale == 0)
                continue;

            //Update horizontal, vertical positioning and zoom
            cameraMovement.x = Input.GetAxis("Mouse X");
            cameraMovement.y = Input.GetAxis("Mouse Y");
            cameraMovement.z = 0; //Start with no zoom movement (zoom handled later on)

            //Apply positioning
            cameraTransform.Translate(cameraMovement * Time.deltaTime * 30);

            //Look at player
            cameraTransform.LookAt(transform);

            //Calculate zoom
            localZPosition -= Input.GetAxis("Mouse ScrollWheel") * 200 * Time.deltaTime;
            if (localZPosition < minZ)
                localZPosition = minZ;
            else if (localZPosition > maxZ)
                localZPosition = maxZ;

            //Adjust zoom
            if (cameraTransform.localPosition.magnitude != localZPosition)
                cameraTransform.Translate(Vector3.forward * (cameraTransform.localPosition.magnitude - localZPosition), Space.Self);
        }
    }

    private IEnumerator ManageHealth ()
    {
        Image healthBarBackground = PlanetPauseMenu.pauseMenu.HUD.Find("Health Bar Background").GetComponent<Image>();

        //Every frame, manage visibility of health bar until death
        int originalLifeNumber = GetLifeNumber();
        while (originalLifeNumber == GetLifeNumber())
        {
            //Hide health bar
            SetImageTransparency(healthBar, 0);
            SetImageTransparency(healthBarBackground, 0);

            //Full health; do nothing
            while (health >= maxHealth)
                yield return null;

            //Show health bar
            SetImageTransparency(healthBar, 1);
            SetImageTransparency(healthBarBackground, 1);

            //Wait until reach full health again (and regen in meantime)
            while (health < maxHealth && !dead)
            {
                if(healthRegen > 0)
                    Heal(healthRegen / 5, false);

                yield return new WaitForSeconds(0.2f);
            }

            //Don't manage health bar anymore if dead
            if (dead)
                break;

            //Fade out health bar (break if get below max health again)
            for(float t = 1; t > 0 && health >= maxHealth; t -= Time.deltaTime)
            {
                SetImageTransparency(healthBar, t);
                SetImageTransparency(healthBarBackground, t);

                yield return null;
            }
        }

        //Only get here on death, so hide health bar since we won't be needing that anymore
        SetImageTransparency(healthBar, 0);
        SetImageTransparency(healthBarBackground, 0);
    }

    public void UpdateHealthBarColor() { SetImageColorKeepTransparency(healthBar, squad.GetArmy().color); }

    private void SetImageTransparency (Image image, float alpha)
    {
        Color imageColor = image.color;
        imageColor.a = alpha;
        image.color = imageColor;
    }

    private void SetImageColorKeepTransparency(Image image, Color color)
    {
        Color imageColor = color;
        imageColor.a = image.color.a;
        image.color = imageColor;
    }

    public override void AimDownSights () { StartCoroutine(AimDownSightsImplement()); }

    private IEnumerator AimDownSightsImplement ()
    {
        if (performingAction || !holding || holding.IsStabbing())
            yield break;

        performingAction = true;
        holding.GetComponent<Gun>().aiming = true;

        Item aimingDown = holding;

        //Transition to aiming down sights
        float duration = 0.2f;
        Vector3 itemPosition;
        for(float t = 0.0f; t < duration && Input.GetButton("Secondary Action"); t += Time.deltaTime)
        {
            //If we drop our weapon then zooming is off
            if (aimingDown != holding)
                break;

            //Set new x position
            itemPosition = holding.transform.localPosition;
            itemPosition.x = Mathf.Lerp(0.5f, 0.0f, t / duration);
            itemPosition.z = Mathf.Lerp(0.0f, 0.5f, t / duration);
            holding.transform.localPosition = itemPosition;

            //Zoom in FOV
            cameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(60, 40, t / duration);

            //Wait a frame
            yield return null;
        }

        reticle.enabled = true;

        //Hold until aim button is released
        while (aimingDown == holding && Input.GetButton("Secondary Action"))
            yield return null;

        reticle.enabled = false;

        //Transition out of it
        duration = 0.2f;
        for (float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            //If we drop our weapon then zooming is off
            if (aimingDown != holding)
                break;

            //Set new x position
            itemPosition = holding.transform.localPosition;
            itemPosition.x = Mathf.Lerp(0.0f, 0.5f, t / duration);
            itemPosition.z = Mathf.Lerp(0.5f, 0.0f, t / duration);
            holding.transform.localPosition = itemPosition;

            //Zoom out FOV
            cameraTransform.GetComponent<Camera>().fieldOfView = Mathf.Lerp(40, 60, t / duration);

            //Wait a frame
            yield return null;
        }

        //Now that we're done aiming, completely reset position and fov to original values...
        
        if (aimingDown == holding)
        {
            //Reset positioning
            itemPosition = holding.transform.localPosition;
            itemPosition.x = 0.5f;
            itemPosition.z = 0.0f;
            holding.transform.localPosition = itemPosition;

            holding.GetComponent<Gun>().aiming = false;
        }

        //Reset FOV
        cameraTransform.GetComponent<Camera>().fieldOfView = 60;

        performingAction = false;
    }

    public override void RaiseShield () { StartCoroutine(RaiseShieldImplement()); }

    private IEnumerator RaiseShieldImplement ()
    {
        if (performingAction || !holding || holding.GetComponent<Shield>().IsShielding() || holding.IsStabbing())
            yield break;

        holding.GetComponent<Shield>().SetShielding(true);

        Item hidingBehind = holding;

        //Transition to shield raised
        float duration = 0.2f;
        Vector3 itemPosition;
        for (float t = 0.0f; t < duration && Input.GetButton("Secondary Action"); t += Time.deltaTime)
        {
            //If we drop our shield
            if (hidingBehind != holding)
                break;

            //Set new x position
            itemPosition = holding.transform.localPosition;
            itemPosition.x = Mathf.Lerp(0.5f, 0.0f, t / duration);
            itemPosition.z = Mathf.Lerp(0.0f, 0.1f, t / duration);
            holding.transform.localPosition = itemPosition;

            //Wait a frame
            yield return null;
        }

        //Hold until raise shield button is released
        while (hidingBehind == holding && Input.GetButton("Secondary Action"))
            yield return null;

        //Transition out of it
        duration = 0.2f;
        for (float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            //If we drop our shield
            if (hidingBehind != holding)
                break;

            //Set new x position
            itemPosition = holding.transform.localPosition;
            itemPosition.x = Mathf.Lerp(0.0f, 0.5f, t / duration);
            itemPosition.z = Mathf.Lerp(0.1f, 0.0f, t / duration);
            holding.transform.localPosition = itemPosition;

            //Wait a frame
            yield return null;
        }

        //Now that we're done cowering, completely reset position to original value...

        if (hidingBehind == holding)
        {
            //Reset positioning
            itemPosition = holding.transform.localPosition;
            itemPosition.x = 0.5f;
            itemPosition.z = 0.0f;
            holding.transform.localPosition = itemPosition;

            holding.GetComponent<Shield>().SetShielding(false);
        }
    }

    public void FlashItemInfo () { StartCoroutine(FlashItemInfoImplement()); }

    private IEnumerator FlashItemInfoImplement ()
    {
        if (!holding)
            yield break;

        int key = ++itemInfoFlashCode;

        //Update text
        itemInfo.text = holding.GetItemInfo();
        if (itemInfo.text == null || itemInfo.text.Equals("")) //If text is blank, then just blank everything and we're done
        {
            itemInfo.text = "";
            itemInfo.enabled = false;
            yield break;
        }
        else //Else, there's real text, so let's display it, wait, and then fade it out
            itemInfo.enabled = true;

        //Make text fully opaque
        Color textColor;
        textColor = itemInfo.color;
        textColor.a = 1.0f;
        itemInfo.color = textColor;

        //Align text based on what other UI is currently on the screen
        itemInfo.alignment = DurabilityTextManager.ShowingDurabilityTextCurrently() ? TextAnchor.LowerRight : TextAnchor.LowerLeft;

        //Wait to fade
        float duration = 3.0f;
        for (float t = 0.0f; t < duration && key == itemInfoFlashCode; t += Time.deltaTime)
            yield return null;

        //Fade period
        duration = 3.0f;
        for (float t = 0.0f; t < duration && key == itemInfoFlashCode; t += Time.deltaTime)
        {
            //Update fading alpha of text color
            textColor = itemInfo.color;
            textColor.a = Mathf.Lerp(1.0f, 0.0f, t / duration);
            itemInfo.color = textColor;

            //Wait a frame
            yield return null;
        }

        if(key == itemInfoFlashCode)
        {
            //Make text fully transparent
            textColor = itemInfo.color;
            textColor.a = 0.0f;
            itemInfo.color = textColor;
        }
    }

    public void IncrementItemInfoFlashCode () { itemInfoFlashCode++; }

    public void BlankItemInfo () { itemInfo.text = ""; }

    public override void EquipGear (GameObject gear, bool forHead)
    {
        if(forHead) //Head gear
        {
            //Get rid of previous gear first
            if (headGear)
                Destroy(headGear);

            headGear = gear;

            if (!gear)
                return;

            //Position head gear
            gear.transform.parent = head;
            gear.transform.localPosition = Vector3.zero;
            gear.transform.localRotation = Quaternion.Euler(0, 0, 0);

            //Visibility
            UpdateGearVisibility();
        }
        else //Body gear
        {
            //Get rid of previous gear first
            if (bodyGear)
                Destroy(bodyGear);

            bodyGear = gear;

            if (!gear)
                return;

            //Position body gear
            gear.transform.parent = transform;
            gear.transform.localPosition = Vector3.zero;
            gear.transform.localRotation = Quaternion.Euler(0, 0, 0);

            //Visibility
            UpdateGearVisibility();
        }
    }

    public void UpdateGearVisibility (bool forceShow = false)
    {
        if (headGear)
            headGear.SetActive(pov != POV.FirstPerson || forceShow);

        if (bodyGear)
            bodyGear.SetActive(pov != POV.FirstPerson || forceShow);
    }

    public void BulletRicochet (RaycastHit hit)
    {
        if (hit.transform.root == transform) //Sound of bullet hitting gear/items on player
        {
            GetAudioSource().PlayOneShot(gearRicochet);

            if (holding && holding.GetComponent<Shield>())
                holding.GetComponent<Shield>().ImpactRecoil();
        }
        else if (Vector3.Distance(transform.position, hit.point) < 15) //Sound of bullet ricochet nearby
            AudioSource.PlayClipAtPoint(ricochet, hit.point);
    }

    private IEnumerator ManageFootstepsMaterial ()
    {
        RaycastHit raycastHit;

        //Every 5th of a second, update the kind of sound feet make when walking (just material not speed)
        while (true)
        {
            if(touchingWater) //Water walking
            {
                if (head.position.y < submersedIn.bounds.max.y + 0.15f) //Fully submersed
                {
                    if (lastMaterialSteppedOn != PlanetMaterialType.Swimming)
                    {
                        SetMaterialUnderFeet(PlanetMaterialType.Swimming);
                        SetHeadSubmergence(true);
                    }
                }
                else //Knee deep
                {
                    if (lastMaterialSteppedOn != PlanetMaterialType.Water)
                    {
                        SetMaterialUnderFeet(PlanetMaterialType.Water);
                        SetHeadSubmergence(false);
                    }
                }
            }
            else
            {
                SetHeadSubmergence(false);

                Physics.Raycast(transform.position, Vector3.down, out raycastHit);
                if (raycastHit.transform)
                {
                    PlanetMaterialType newMaterialType = PlanetMaterial.GetMaterialFromTransform(raycastHit.collider.transform, transform.position);
                    SetMaterialUnderFeetIfNeeded(newMaterialType);
                }
            }

            //Wait
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SetMaterialUnderFeetIfNeeded(PlanetMaterialType planetMaterialType)
    {
        if (lastMaterialSteppedOn != planetMaterialType)
            SetMaterialUnderFeet(planetMaterialType);
    }

    private void SetMaterialUnderFeet (PlanetMaterialType planetMaterialType)
    {
        //Update tracker
        lastMaterialSteppedOn = planetMaterialType;

        //Set footstep material
        walking = PlanetMaterial.GetMaterialAudio(planetMaterialType, PlanetMaterialInteractionType.Walking);
        running = PlanetMaterial.GetMaterialAudio(planetMaterialType, PlanetMaterialInteractionType.Running);

        //Update sound currently playing
        if (sprinting)
            feet.clip = running;
        else
            feet.clip = walking;
    }

    public override void Submerge (Collider water)
    {
        base.Submerge(water);

        submersedIn = water;
    }

    public override void Emerge (Collider water)
    {
        base.Emerge(water);

        submersedIn = null;
    }

    private void SetHeadSubmergence (bool submerged)
    {
        if(submerged)
        {
            if(!underwaterReverb.enabled) //Submerge
            {
                underwaterReverb.enabled = true;
                underwaterVisual.enabled = true;
                underwaterAmbience.Play();

                Planet.planet.ambientVolume *= 0.25f;
                PlanetTerrain.planetTerrain.SetTreeVisibility(false);
            }
        }
        else
        {
            if (underwaterReverb.enabled) //Emerge
            {
                underwaterReverb.enabled = false;
                underwaterVisual.enabled = false;
                underwaterAmbience.Pause();

                Planet.planet.ambientVolume *= 4;
                PlanetTerrain.planetTerrain.SetTreeVisibility(true);
            }
        }
    }

    public override bool RaycastShoot (Transform from, int range, out RaycastHit hit)
    {
        if(pov != POV.ThirdPerson360)
        {
            Ray ray = cameraTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

            return Physics.Raycast(ray, out hit, range);
        }
        else
            return base.RaycastShoot(from, range, out hit);
    }

    private IEnumerator FlyingSFX ()
    {
        if (soaring || IsGrounded(0.1f) || touchingWater || rBody.velocity.magnitude <= 35)
            yield break;

        soaring = true;

        //Create audio source
        AudioSource flyingSFX = gameObject.AddComponent<AudioSource>();

        //Get it going
        flyingSFX.spatialBlend = 0.0f;
        flyingSFX.loop = true;
        flyingSFX.clip = flying;
        flyingSFX.Play();

        //Make it pause when game pauses
        God.god.ManageAudioSource(flyingSFX);

        //Wait until we are no longer supposed to be flying/soaring
        while (!IsGrounded(0.1f) && !touchingWater && rBody.velocity.magnitude > 35)
            yield return new WaitForSeconds(0.1f);
        
        //Stop flying
        flyingSFX.Stop();
        God.god.UnmanageAudioSource(flyingSFX);
        Destroy(flyingSFX);

        soaring = false;
    }

    public override void BringToLife ()
    {
        base.BringToLife();

        //Start managing our health again
        StartCoroutine(ManageHealth());

        //Re-equip item we had before death
        if (droppedOnDeath)
        {
            Equip(droppedOnDeath);
            droppedOnDeath = null;
        }

        //Back on your feet
        rBody.constraints = RigidbodyConstraints.FreezeRotation;

        //There's a rare glitch where there is a left over interact option from spawning otherwise
        EraseInteractOption();
    }

    public override void OnCreationFromSpawner(Spawner spawner)
    {
        base.OnCreationFromSpawner(spawner);

        PlanetPauseMenu.pauseMenu.UpdateFactionColor();
    }

    public void SetContinuousPrimaryAction (bool continuous) { continuousPrimaryAction = continuous; }

    public void IncrementIndoorZoneCount (AudioReverbPreset indoorReverbPreset, IndoorZoneGrouping indoorZoneGrouping)
    {
        if(indoorZoneCount == 0) //Switch from outdoors to indoors
        {
            Planet.planet.ambientVolume *= 0.25f;
            outdoorReverbPreset = God.god.GetComponent<AudioReverbZone>().reverbPreset;
            God.god.GetComponent<AudioReverbZone>().reverbPreset = indoorReverbPreset;

            //moveSpeed *= 0.5f;

            IndoorZoneGrouping.playerInside = indoorZoneGrouping;

            //Entered building without using any doors (possible on respawn) so go to indoor ambience
            if (indoorZoneGrouping.AirTight())
                Planet.planet.ambientVolume *= 0.25f;
        }

        indoorZoneCount++;
    }

    public void DecrementIndoorZoneCount ()
    {
        if(indoorZoneCount == 1) //Switch from indoors to outdoors
        {
            Planet.planet.ambientVolume *= 4;
            God.god.GetComponent<AudioReverbZone>().reverbPreset = outdoorReverbPreset;

            //moveSpeed *= 2.0f;

            if (IndoorZoneGrouping.playerInside)
            {
                //Left building without using any doors (possible on respawn) so go to outdoor ambience
                if (IndoorZoneGrouping.playerInside.AirTight())
                    Planet.planet.ambientVolume *= 4;

                IndoorZoneGrouping.playerInside = null;
            }
        }

        indoorZoneCount--;
    }

    public override void OverrideControl (Interactable overrider)
    {
        base.OverrideControl(overrider);

        feet.Stop();
        SetSprinting(false);

        if (overrider)
        {
            if (overrider is Seat)
                StartCoroutine(PlayerSitOverride((Seat)overrider));
            else if (overrider is Ladder)
                StartCoroutine(PlayerLadderOverride(overrider));
            else if (overrider is Turret)
                StartCoroutine(PlayerTurretOverride((Turret)overrider));
            else
                StartCoroutine(PlayerSleepOverride(overrider));

            //Ladder override is only for player and is handled within Ladder.cs
        }
    }

    //Dying or pressing any key besides 360 POV key will break this override
    private IEnumerator PlayerSleepOverride (Interactable overrider)
    {
        do
        {
            if(Input.GetButtonDown("360 View"))
            {
                if (pov == POV.ThirdPerson360)
                    SetPOV(POV.FirstPerson);
                else
                    SetPOV(POV.ThirdPerson360);
            }

            yield return null;
        }
        while (!dead && (!Input.anyKeyDown || Input.GetButtonDown("360 View")));

        overrider.ReleaseControl(!dead);
    }

    //Only death or outside interference will break this override
    //Updates all but movement as well
    private IEnumerator PlayerLadderOverride (Interactable overrider)
    {
        //Wait a frame so item update doesn't get called twice in first frame
        yield return null;

        while (!dead && controlOverride)
        {
            RotationInputUpdate();
            ItemInputUpdate();
            POVInputUpdate();
            SquadInputUpdate();

            yield return null;
        }

        overrider.ReleaseControl(!dead);
    }

    private IEnumerator PlayerSitOverride (Seat seat)
    {
        Vehicle vehicleUnderControl = seat.controls;

        //Player set up
        head.localEulerAngles = Vector3.zero;

        if (vehicleUnderControl)
        {
            vehicleUnderControl.UpdateGearIndicator();
            Vehicle.speedometerText.enabled = true;
            Vehicle.gearIndicatorText.enabled = true;
        }

        while (!dead && !seat.EjectingOccupant())
        {
            //Wait a frame
            yield return null;

            //Pause
            if (Time.timeScale == 0)
                continue;

            //Update inputs
            RotationInputUpdate();
            ItemInputUpdate();
            POVInputUpdate();
            SquadInputUpdate();

            //Update vehicle GUI
            if (vehicleUnderControl)
            {
                vehicleUnderControl.SetGasPedal(Input.GetAxis("Vertical"));
                vehicleUnderControl.SetSteeringWheel(Input.GetAxis("Horizontal"));

                if (Input.GetButtonDown("Up One Gear"))
                    vehicleUnderControl.ChangeGear(true, true);
                else if (Input.GetButtonDown("Down One Gear"))
                    vehicleUnderControl.ChangeGear(false, true);

                vehicleUnderControl.UpdateSpeedometer();
            }
        }

        //Player clean up
        if(vehicleUnderControl)
        {
            Vehicle.speedometerText.enabled = false;
            Vehicle.gearIndicatorText.enabled = false;
        }

        seat.ReleaseControl(!dead);
    }

    private IEnumerator PlayerTurretOverride(Turret turret)
    {
        //Wait an initial frame to avoid redundant logic checks
        yield return null;

        while (!dead && controlOverride && !Input.GetButtonDown("Interact"))
        {
            //Pause
            if (Time.timeScale == 0)
            {
                yield return null;
                continue;
            }

            //Update inputs
            POVInputUpdate();
            SquadInputUpdate();

            //Update turret inputs
            if(pov != POV.ThirdPerson360)
            {
                turret.RotateTurret(Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime);
                turret.RotateTurret(Vector3.right, -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime);
            }
            turret.SetTriggerPressed(Input.GetButton("Primary Action"));

            //Wait a frame
            yield return null;
        }

        turret.ReleaseControl(!dead);
    }

    private void SetSprinting (bool sprinting)
    {
        if(sprinting)
        {
            if(!this.sprinting) //Start sprinting
            {
                this.sprinting = true;
                moveSpeed *= 1.3333f;
                feet.clip = running;
            }
        }
        else if(this.sprinting) //Stop sprinting
        {
            this.sprinting = false;
            moveSpeed *= 0.75f;
            feet.clip = walking;
        }
    }

    public override bool StabbingWithIntentToExecute (float durationIntoExecution) { return durationIntoExecution < 5 && (Input.GetButton("Primary Action") || PlanetPauseMenu.pauseMenu.IsPaused()); }

    public void PlayHitMarkerSound (bool hitArmor)
    {
        if (hitArmor)
            GetAudioSource().PlayOneShot(hitArmorMarker);
        else
            GetAudioSource().PlayOneShot(hitMarker);
    }

    public AudioSource GetFeetAudioSource() { return feet; }

    public Camera GetCamera () { return cameraTransform.GetComponent<Camera>(); }

    public void ResetHeadRotation() { head.localEulerAngles = Vector3.zero; }

    public void ApplyDisplaySettings ()
    {
        rotationSpeed = VideoSettings.sensitivity;
        cameraTransform.GetComponent<Camera>().farClipPlane = VideoSettings.viewDistance;
        QualitySettings.SetQualityLevel(VideoSettings.quality);
    }

    public void AssignOrderButtons (Dropdown dropdown1, Dropdown dropdown2, Dropdown dropdown3)
    {
        order1 = (Squad.Orders)dropdown1.value;
        order2 = (Squad.Orders)dropdown2.value;
        order3 = (Squad.Orders)dropdown3.value;
    }

    public void RestoreOrderButtons (Dropdown dropdown1, Dropdown dropdown2, Dropdown dropdown3)
    {
        dropdown1.value = (int)order1;
        dropdown2.value = (int)order2;
        dropdown3.value = (int)order3;
    }
}