using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillView : MonoBehaviour
{
    [SerializeField]
    private Camera pillViewCamera = null;

    [SerializeField]
    private Transform pill = null;

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
        transform.localPosition = new Vector3(10 * newPillViewIndex, transform.localPosition.y, transform.localPosition.z);

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

    //This method should be used in order to set the y rotation of the pill.
    public void SetPillRotation(float pillRotationValue)
    {
        pill.localEulerAngles = new Vector3(pill.localEulerAngles.x, pillRotationValue, pill.localEulerAngles.z);
    }

    //This method returns the target render texture of the pill view's camera.
    public RenderTexture GetRenderTexture()
    {
        return pillViewCamera.targetTexture;
    }

    //This method should be called to delete the pill view.
    public void Delete()
    {
        PillViewsManager.DeletePillView(this);
    }
}
