using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPillView : MonoBehaviour
{
    [Header("Components")]

    [SerializeField] private GameObject _pill = null;
    [SerializeField] private Camera _camera = null;

    //Non-inspector variables.

    /// <summary>
    /// Private variable that holds the game object that represents the body gear on the currently displayed pill. Will be null if the pill has no body gear or if there is no pill being displayed currently.
    /// </summary>
    private GameObject bodyGear = null;
    /// <summary>
    /// Private variable that holds the game object that represents the curated eye wear on the currently displayed pill. Will be null if the pill has no curated eye wear or if there is no pill being displayed currently.
    /// </summary>
    private GameObject curatedEyeWear = null;
    /// <summary>
    /// Private variable that holds the game object that represents the head gear on the currently displayed pill. Will be null if the pill has no head gear or if there is no pill being displayed currently.
    /// </summary>
    private GameObject headGear = null;
    /// <summary>
    /// Private variable that holds the game object that represents the primary weapon of the currently displayed pill. Will be null if the pill has no primary weapon equipped or if there is no pill being displayed currently.
    /// </summary>
    private GameObject primary = null;
    /// <summary>
    /// Private variable that holds the game object that represents the secondary weapon of the currently displayed pill. Will be null if the pill has no secondary weapon equipped or if there is no pill being displayed currently.
    /// </summary>
    private GameObject secondary = null;

    /// <summary>
    /// Public property that should be used both to access and mutate the pill that is currently being displayed by the pill view.
    /// </summary>
    public NewGalaxyPill displayedPill
    {
        get => _displayedPill;
        set
        {
            //Stores the previously displayed pill in a temporary variable.
            NewGalaxyPill previouslyDisplayedPill = _displayedPill;
            //Sets the pill being displayed to the specified value.
            _displayedPill = value;
            //Checks if the previously displayed pill is still tracking this pill view and stops that if so.
            if (previouslyDisplayedPill != null && previouslyDisplayedPill.pillViews.Contains(this))
                previouslyDisplayedPill.pillViews.Remove(this);
            //Checks if the newly assigned pill to display is tracking this pill view and tells it to if not.
            if (displayedPill != null && !displayedPill.pillViews.Contains(this))
                displayedPill.pillViews.Add(this);

            //Updates the appearance of the pill display to match the new displayed pill.
            UpdatePillAppearance();

            //Updates the camera to fully capture the new displayed pill with all of their gear and weapons equipped.
            UpdateCamera();
        }
    }
    /// <summary>
    /// Private holder variable for the pill that is currently being displayed by the pill view.
    /// </summary>
    private NewGalaxyPill _displayedPill = null;

    /// <summary>
    /// Public property that should be used in order to access the bounds of the pill.
    /// </summary>
    public Bounds pillMaxBounds
    {
        get
        {
            Renderer[] renderers = _pill.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(_pill.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers)
                b.Encapsulate(r.bounds);
            return b;
        }
    }

    /// <summary>
    /// Public property that should be used in order to access the render texture output of the pill view.
    /// </summary>
    public RenderTexture renderTexture { get => _camera.targetTexture; set => _camera.targetTexture = value; }

    /// <summary>
    /// Public property that should be used both in order to access and mutate the float value that indicates the rotation of the pill in the pill view.
    /// </summary>
    public float rotation
    {
        get => _pill.transform.localEulerAngles.y - 180;
        set => _pill.transform.localEulerAngles = new Vector3(_pill.transform.localEulerAngles.x, value + 180, _pill.transform.localEulerAngles.z);
    }

    /// <summary>
    /// Public method that should be called in order to update the camera to fit the whole pill into its view.
    /// </summary>
    public void UpdateCamera()
    {
        //Fetches the max bounds of the pill from the property.
        Bounds pillMaxBounds = this.pillMaxBounds;
        //Updates the camera's position on all 3 axes.
        _camera.transform.position = new Vector3(pillMaxBounds.center.x, pillMaxBounds.center.y, 0.5f + Mathf.Abs(pillMaxBounds.max.z));
        //Updates the far clip plane of the camera to capture everything no matter how far out that is under the pill on the z axis.
        _camera.farClipPlane = 1 + Mathf.Abs(pillMaxBounds.max.z) + Mathf.Abs(pillMaxBounds.min.z);
        //Calculates the camera's orthographic size needed in order to capture everything needed on the x axis.
        float xSize = Mathf.Abs(Mathf.Abs(pillMaxBounds.max.x) - Mathf.Abs(pillMaxBounds.min.x));
        //Calculates the camera's orthographic size needed in order to capture everything needed on the y axis.
        float ySize = (Mathf.Abs(pillMaxBounds.max.y) + Mathf.Abs(pillMaxBounds.min.y)) / 2;
        //Updates the camera's orthographic size to capture everything on both the x and y axes.
        _camera.orthographicSize = xSize > ySize ? xSize : ySize;
    }

    /// <summary>
    /// Public method that should be called in order to update the appearance (body gear, curated eye wear, head gear, primary, secondary, and skin) of the pill that is currently being displayed (if there is one).
    /// </summary>
    public void UpdatePillAppearance()
    {
        //Updates the body gear.
        UpdateBodyGear();
        //Updates the curated eye wear.
        UpdateCuratedEyeWear();
        //Updates the head gear.
        UpdateHeadGear();
        //Updates the primary weapon.
        UpdatePrimary();
        //Updates the secondary weapon.
        UpdateSecondary();
        //Updates the skin on the pill.
        UpdateSkin();
        //Sets the pill game object to be active if there is a valid pill to display and inactive if there is not a valid pill to display.
        _pill.gameObject.SetActive(displayedPill != null);
    }

    /// <summary>
    /// Public method that should be called in order to update the body gear that is currently being shown on the displayed pill (if there is one).
    /// </summary>
    public void UpdateBodyGear()
    {
        //Destroys any pre-existing body gear.
        if (bodyGear != null)
        {
            Destroy(bodyGear);
            bodyGear = null;
        }

        //Checks if there is a pill being displayed currently and if there is any valid body gear to show on it. If so, the body gear is instantiated from the appropriate prefab and parented under the pill object.
        if(displayedPill != null && _pill != null && displayedPill.pillClass != null && displayedPill.pillClass.bodyGearPrefab != null)
        {
            //Instatiates a new piece of body gear from the appropriate body gear prefab.
            bodyGear = Instantiate(displayedPill.pillClass.bodyGearPrefab);

            //Sets the body gear's parent as the pill object.
            bodyGear.transform.SetParent(_pill.transform);

            //Resets the scale of the body gear.
            bodyGear.transform.localScale = Vector3.one;
            //Resets the position of the body gear.
            bodyGear.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Public method that should be called in order to update the curated eye wear that is currently being shown on the displayed pill (if there is one).
    /// </summary>
    public void UpdateCuratedEyeWear()
    {
        //Destroys any pre-existing curated eye wear.
        if (curatedEyeWear != null)
        {
            Destroy(curatedEyeWear);
            curatedEyeWear = null;
        }

        //Checks if there is a pill being displayed currently and if there is any valid curated eye wear to show on it. If so, the curated eye wear is instantiated from the appropriate prefab and parented under the pill object.
        if (displayedPill != null && _pill != null && displayedPill.pillClass != null && displayedPill.pillClass.curatedEyeWearPrefab != null)
        {
            //Instatiates a new piece of curated eye wear from the appropriate curated eye wear prefab.
            curatedEyeWear = Instantiate(displayedPill.pillClass.curatedEyeWearPrefab);

            //Sets the curated eye wear's parent as the pill object.
            curatedEyeWear.transform.SetParent(_pill.transform);

            //Resets the scale of the curated eye wear.
            curatedEyeWear.transform.localScale = Vector3.one;
            //Resets the position of the curated eye wear.
            curatedEyeWear.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Public method that should be called in order to update the head gear that is currently being shown on the displayed pill (if there is one).
    /// </summary>
    public void UpdateHeadGear()
    {
        //Destroys any pre-existing head gear.
        if (headGear != null)
        {
            Destroy(headGear);
            headGear = null;
        }

        //Checks if there is a pill being displayed currently and if there is any valid head gear to show on it. If so, the head gear is instantiated from the appropriate prefab and parented under the pill object.
        if (displayedPill != null && _pill != null && displayedPill.pillClass != null && displayedPill.pillClass.headGearPrefab != null)
        {
            //Instatiates a new piece of head gear from the appropriate head gear prefab.
            headGear = Instantiate(displayedPill.pillClass.headGearPrefab);

            //Sets the head gear's parent as the pill object.
            headGear.transform.SetParent(_pill.transform);

            //Resets the scale of the head gear.
            headGear.transform.localScale = Vector3.one;
            //Resets the position of the head gear.
            headGear.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Public method that should be called in order to update the primary weapon that is currently being shown as equipped on the displayed pill (if there is one).
    /// </summary>
    public void UpdatePrimary()
    {
        //Destroys any pre-existing primary weapon.
        if (primary != null)
        {
            Destroy(primary);
            primary = null;
        }

        //Checks if there is a pill being displayed currently and if there is any valid primary weapon equipped to show on it. If so, the primary weapon is instantiated from the appropriate prefab and parented under the pill object.
        if (displayedPill != null && _pill != null && displayedPill.pillClass != null && displayedPill.pillClass.primaryPrefab != null)
        {
            //Instatiates a new primary weapon from the appropriate primary weapon prefab.
            primary = Instantiate(displayedPill.pillClass.primaryPrefab);

            //Sets the primary weapon's parent as the pill object.
            primary.transform.SetParent(_pill.transform);

            //Resets the scale of the primary weapon.
            primary.transform.localScale = Vector3.one;
            //Sets the position of the primary weapon to be in the pill's hand.
            primary.transform.localPosition = primary.GetComponent<Item>().GetPlaceInPlayerHand();

            //Destroys the item script attached to the primary weapon since the script is only functional on the planet view and not the galaxy view.
            Destroy(primary.GetComponent<Item>());
        }
    }

    /// <summary>
    /// Public method that should be called in order to update the secondary weapon that is currently being shown as equipped on the displayed pill (if there is one).
    /// </summary>
    public void UpdateSecondary()
    {
        //Destroys any pre-existing secondary weapon.
        if (secondary != null)
        {
            Destroy(secondary);
            secondary = null;
        }

        //Checks if there is a pill being displayed currently and if there is any valid secondary weapon equipped to show on it. If so, the secondary weapon is instantiated from the appropriate prefab and parented under the pill object.
        if (displayedPill != null && _pill != null && displayedPill.pillClass != null && displayedPill.pillClass.secondaryPrefab != null)
        {
            //Instatiates a new secondary weapon from the appropriate secondary weapon prefab.
            secondary = Instantiate(displayedPill.pillClass.secondaryPrefab);

            //Sets the secondary weapon's parent as the pill object.
            secondary.transform.SetParent(_pill.transform);

            //Resets the scale of the secondary weapon.
            secondary.transform.localScale = Vector3.one;
            //Sets the position of the secondary weapon to be on the player's back.
            secondary.transform.localPosition = secondary.GetComponent<Item>().GetPlaceOnBack();
            //Sets the rotation of the secondary weapon on the back (which is item specific sometimes).
            secondary.transform.localRotation = Quaternion.Euler(secondary.GetComponent<Item>().GetRotationOnBack());

            //Destroys the item script attached to the secondary weapon since the script is only functional on the planet view and not the galaxy view.
            Destroy(secondary.GetComponent<Item>());
        }
    }

    /// <summary>
    /// Public method that should be called in order to update the skin that is currently being shown on the displayed pill (if there is one).
    /// </summary>
    public void UpdateSkin()
    {
        //Removes any pre-existing skin from the pill.
        if (_pill != null)
            _pill.GetComponent<MeshRenderer>().sharedMaterial = null;

        //Checks if a skin can be applied to the displayed pill (if there is one) and if so then the skin is then applied to said pill.
        if (displayedPill != null && _pill != null && displayedPill.pillClass != null && displayedPill.pillClass.skin != null)
            _pill.GetComponent<MeshRenderer>().sharedMaterial = displayedPill.pillClass.skin;
    }

    /// <summary>
    /// Public method that should be called in order to delete a pill view. This method simply calls the pill view manager's delete pill view method but makes the deletion simpler for the programmer instead of having to go through a bunch of extra steps.
    /// </summary>
    public void Delete()
    {
        //Checks if the galaxy manager has been initialized yet and deletes the pill view using the galaxy manager's pill view manager if so.
        if (NewGalaxyManager.isInitialized)
            NewGalaxyManager.pillViewsManager.DeletePillView(this);
    }
}
