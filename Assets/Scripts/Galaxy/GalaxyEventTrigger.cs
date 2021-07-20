using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GalaxyEventTrigger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject targetObject = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        IBeginDragHandler targetObjectBeginDragHandler = targetObject.GetComponent<IBeginDragHandler>();
        if (targetObjectBeginDragHandler != null)
            targetObjectBeginDragHandler.OnBeginDrag(pointerEventData);
    }

    public void OnDrag(PointerEventData pointerEventData)
    {
        IDragHandler targetObjectDragHandler = targetObject.GetComponent<IDragHandler>();
        if (targetObjectDragHandler != null)
            targetObjectDragHandler.OnDrag(pointerEventData);
    }

    public void OnEndDrag(PointerEventData pointerEventData)
    {
        IEndDragHandler targetObjectEndDragHandler = targetObject.GetComponent<IEndDragHandler>();
        if (targetObjectEndDragHandler != null)
            targetObjectEndDragHandler.OnEndDrag(pointerEventData);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        IPointerEnterHandler targetObjectPointerEnterHandler = targetObject.GetComponent<IPointerEnterHandler>();
        if (targetObjectPointerEnterHandler != null)
            targetObjectPointerEnterHandler.OnPointerEnter(pointerEventData);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        IPointerExitHandler targetObjectPointerExitHandler = targetObject.GetComponent<IPointerExitHandler>();
        if (targetObjectPointerExitHandler != null)
            targetObjectPointerExitHandler.OnPointerExit(pointerEventData);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        IPointerClickHandler targetObjectPointerClickHandler = targetObject.GetComponent<IPointerClickHandler>();
        if (targetObjectPointerClickHandler != null)
            targetObjectPointerClickHandler.OnPointerClick(pointerEventData);
    }

    public void OnSelect(BaseEventData baseEventData)
    {
        ISelectHandler targetObjectSelectHandler = targetObject.GetComponent<ISelectHandler>();
        if (targetObjectSelectHandler != null)
            targetObjectSelectHandler.OnSelect(baseEventData);
    }

    public void OnDeselect(BaseEventData baseEventData)
    {
        IDeselectHandler targetObjectDeselectHandler = targetObject.GetComponent<IDeselectHandler>();
        if (targetObjectDeselectHandler != null)
            targetObjectDeselectHandler.OnDeselect(baseEventData);
    }
}
