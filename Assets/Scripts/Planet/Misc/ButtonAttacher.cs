using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAttacher : MonoBehaviour, IPointerEnterHandler
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => PlanetPauseMenu.pauseMenu.OnButtonClick(transform));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlanetPauseMenu.pauseMenu.OnButtonMouseOver(transform);
    }
}
