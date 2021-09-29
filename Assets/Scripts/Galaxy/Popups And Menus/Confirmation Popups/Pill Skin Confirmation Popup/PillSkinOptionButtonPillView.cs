using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PillSkinOptionButtonPillView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler
{
    private PillSkinOptionButton pillSkinOptionButton = null;

    private float initialMouseXOnPillViewDrag;
    private float initialPillRotationOnPillViewDrag;

    // Start is called before the first frame update
    void Start()
    {
        pillSkinOptionButton = transform.parent.GetComponent<PillSkinOptionButton>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //This method is called whenever the pointer enters the pill skin option button pill view.
    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(pillSkinOptionButton.MouseOverPillViewCursor, new Vector2(0, 10), CursorMode.Auto);
    }

    //This method is called whenever the pointer exits the pill skin option button pill view.
    public void OnPointerExit(PointerEventData eventData)
    {
        GeneralHelperMethods.ResetCursorTexture();
    }

    //This method is called whenever the pointer begins to drag on the pill skin option button pill view.
    public void OnBeginDrag(PointerEventData eventData)
    {
        initialMouseXOnPillViewDrag = Input.mousePosition.x;
        initialPillRotationOnPillViewDrag = pillSkinOptionButton.PillView.PillRotation;
    }

    //This method is called whenever the pointer is being dragged on the pill skin option button pill view.
    public void OnDrag(PointerEventData eventData)
    {
        pillSkinOptionButton.PillView.PillRotation = initialPillRotationOnPillViewDrag - ((Input.mousePosition.x - initialMouseXOnPillViewDrag) * pillSkinOptionButton.PillViewRotationSpeed);
    }

    //This method is called whenever the pill view raw image game object is disabled and resets the mouse cursor.
    private void OnDisable()
    {
        GeneralHelperMethods.ResetCursorTexture();
    }

    //This method is called whenever the pill view raw image game object is destroyed and resets the mouse cursor.
    private void OnDestroy()
    {
        GeneralHelperMethods.ResetCursorTexture();
    }
}
