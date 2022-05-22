using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillView : MonoBehaviour
{
    [Header("Components")]

    [SerializeField]
    private Camera pillViewCamera = null;

    public RenderTexture RenderTexture
    {
        get
        {
            if(pillViewCamera != null)
                return pillViewCamera.targetTexture;
            return null;
        }
    }

    [SerializeField]
    private Transform pillTransform = null;

    private GameObject currentHeadGear = null;
    private GameObject currentBodyGear = null;
    private GameObject currentPrimary = null;
    private GameObject currentSecondary = null;

    private GalaxyPill displayedPill = null;
    public GalaxyPill DisplayedPill
    {
        get
        {
            return displayedPill;
        }
        set
        {
            displayedPill = value;
            if(displayedPill != null)
            {
                //Sets the skin of the pill.
                pillTransform.GetComponent<MeshRenderer>().sharedMaterial = displayedPill.Skin;
                if (displayedPill.pillClass != null)
                {
                    //Sets the head gear of the pill.
                    SetHeadGear(displayedPill.pillClass.headGear);
                    //Sets the body gear of the pill.
                    SetBodyGear(displayedPill.pillClass.bodyGear);
                    //Sets the primary of the pill.
                    SetPrimary(displayedPill.pillClass.primary);
                    //Sets the secondary of the pill.
                    SetSecondary(displayedPill.pillClass.secondary);
                }
            }
        }
    }
    public float PillRotation
    {
        get
        {
            return pillTransform.localEulerAngles.y - 180;
        }
        set
        {
            //The rotation that the pill is supposed to now have.
            float pillRotation = value + 180;
            //Actually sets the rotation of the pill.
            pillTransform.localEulerAngles = new Vector3(pillTransform.localEulerAngles.x, pillRotation, pillTransform.localEulerAngles.z);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOtherPillViewAdditionOrDeletion(int newPillViewIndex)
    {
        //Resets the name of the pill view.
        gameObject.name = "Pill View " + (newPillViewIndex + 1);

        //Resets the position of the pill view.
        transform.localPosition = new Vector3(250 * newPillViewIndex, transform.localPosition.y, transform.localPosition.z);

        //Resets the render texture of the pill view's camera.
        if(pillViewCamera.targetTexture != null)
        {
            pillViewCamera.targetTexture.name = "PillView" + (newPillViewIndex + 1) + "RenderTexture";
        }
        else
        {
            RenderTexture renderTexture = new RenderTexture(1000, 1200, 0);
            renderTexture.Create();
            renderTexture.name = "PillView" + newPillViewIndex;
            pillViewCamera.targetTexture = renderTexture;
        }
    }

    /// <summary>
    /// This method should be called in order to update the appearance of the pill in the pill view.
    /// </summary>
    public void UpdatePillView()
    {
        DisplayedPill = DisplayedPill;
    }

    //This method should be called to delete the pill view.
    public void Delete()
    {
        PillViewsManager.DeletePillView(this);
    }

    private void SetHeadGear(GameObject headGear)
    {
        if(currentHeadGear != null)
        {
            Destroy(currentHeadGear);
        }

        if(headGear != null)
        {
            currentHeadGear = Instantiate(headGear);
            currentHeadGear.transform.SetParent(pillTransform);
            currentHeadGear.transform.localRotation = Quaternion.Euler(0, 0, 0);
            currentHeadGear.transform.localPosition = Vector3.up * 0.5f;
        }
    }

    private void SetBodyGear(GameObject bodyGear)
    {
        if(currentBodyGear != null)
        {
            Destroy(currentBodyGear);
        }

        if(bodyGear != null)
        {
            currentBodyGear = Instantiate(bodyGear);
            currentBodyGear.transform.SetParent(pillTransform);
            currentBodyGear.transform.localRotation = Quaternion.Euler(0, 0, 0);
            currentBodyGear.transform.localPosition = Vector3.zero;
        }
    }

    private void SetPrimary(GameObject primary)
    {
        if (currentPrimary != null)
        {
            Destroy(currentPrimary);
        }

        if(primary != null)
        {
            currentPrimary = Instantiate(primary, pillTransform.position, pillTransform.rotation);
            currentPrimary.transform.SetParent(pillTransform);

            currentPrimary.transform.localPosition = new Vector3(0.5f, -0.25f, 0.0f) + Vector3.up * 0.5f;
            currentPrimary.transform.localRotation = Quaternion.Euler(0, 0, 0);

            Destroy(currentPrimary.GetComponent<Item>());
            Destroy(currentPrimary.GetComponent<Rigidbody>());
            Destroy(currentPrimary.GetComponent<BoxCollider>());

            currentPrimary.name = currentPrimary.name.Substring(0, currentPrimary.name.Length - 7);
        }
    }

    private void SetSecondary(GameObject secondary)
    {
        if (currentSecondary != null)
        {
            Destroy(currentSecondary);
        }

        if (secondary != null && displayedPill.isSecondaryVisible)
        {
            currentSecondary = Instantiate(secondary, pillTransform.position, pillTransform.rotation);
            currentSecondary.transform.SetParent(pillTransform);
            Item item = currentSecondary.GetComponent<Item>();

            currentSecondary.transform.localPosition = item.GetPlaceOnBack();
            currentSecondary.transform.localRotation = Quaternion.Euler(item.GetRotationOnBack());

            Destroy(item);
            Destroy(currentSecondary.GetComponent<Rigidbody>());
            Destroy(currentSecondary.GetComponent<BoxCollider>());

            currentSecondary.name = currentSecondary.name.Substring(0, currentSecondary.name.Length - 7);
        }
    }
}
