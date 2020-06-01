using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    public Sprite dropdownItemSelected;
    public Sprite dropdownItemUnselected;

    public AudioSource sfxSource;
    public AudioClip dropdownOptionSelected;

    private Transform sections;
    private Dropdown sectionDropdown;

    public void Start()
    {
        //Get references
        sections = transform.Find("Sections");
        sectionDropdown = transform.Find("Section Dropdown").GetComponent<Dropdown>();

        //Perform set up
        InitializeDropdownOptions();
        LoadSectionPage();
    }

    private void InitializeDropdownOptions ()
    {
        //Get list of section names
        List<Dropdown.OptionData> sectionTitles = new List<Dropdown.OptionData>();
        foreach (Transform section in sections)
            sectionTitles.Add(new Dropdown.OptionData(section.name));

        //Add that list to dropdown
        sectionDropdown.ClearOptions();
        sectionDropdown.AddOptions(sectionTitles);
    }

    public void LoadSectionPage ()
    {
        //Get name of page selected by dropdown
        string newPageName = sectionDropdown.options[sectionDropdown.value].text;

        //Enable page selected by dropdown and disable all other pages
        foreach(Transform section in sections)
        {
            if (section.name.Equals(newPageName))
                section.gameObject.SetActive(true);
            else
                section.gameObject.SetActive(false);
        }
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
