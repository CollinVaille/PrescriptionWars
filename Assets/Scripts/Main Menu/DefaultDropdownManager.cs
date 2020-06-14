using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultDropdownManager : MonoBehaviour
{
    public Sprite dropdownItemSelected;
    public Sprite dropdownItemUnselected;

    public AudioClip dropdownOptionSelected;

    public AudioSource sfxSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDropDownItemEnter(Transform item)
    {
        item.GetComponent<Image>().sprite = dropdownItemSelected;
        sfxSource.PlayOneShot(dropdownOptionSelected);
    }

    public void OnDropDownItemExit(Transform item)
    {
        item.GetComponent<Image>().sprite = dropdownItemUnselected;
    }
}
