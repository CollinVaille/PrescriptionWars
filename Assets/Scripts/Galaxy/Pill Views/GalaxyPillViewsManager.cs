using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPillViewsManager : MonoBehaviour
{
    [SerializeField, Tooltip("Float value that represents the amount of spacing between pill views.")] private float spacing = 10;

    //Non-inspector variables and properties.

    /// <summary>
    /// Private static property that should be used in order to access the prefab that all pill views within the galaxy scene are instantiated from.
    /// </summary>
    private static GameObject pillViewPrefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Pill Views/Pill View"); }

    /// <summary>
    /// Private list that contains all of the pill views within the galaxy scene.
    /// </summary>
    private List<GalaxyPillView> pillViews = null;

    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates exactly how many pill views are in the list of pill views within the galaxy.
    /// </summary>
    public int pillViewsCount { get => pillViews == null ? 0 : pillViews.Count; }

    /// <summary>
    /// Public method that should be called in order to create and return a new pill view to display the specified pill.
    /// </summary>
    /// <param name="pill"></param>
    /// <returns></returns>
    public GalaxyPillView GetNewPillView(NewGalaxyPill pill = null)
    {
        //Instantiates a new pill view from the pill view prefab and does a GetComponent on its pill view script in order to access and modify its values upon creation.
        GalaxyPillView pillView = Instantiate(pillViewPrefab).GetComponent<GalaxyPillView>();
        //Sets the pill view to be parented under the pill views manager.
        pillView.transform.SetParent(transform);
        //Resets the scale of the pill view in order to avoid any Unity shenanigans.
        pillView.transform.localScale = Vector3.one;

        //Checks if the pill views list is null and initializes it to an empty list if so.
        if (pillViews == null)
            pillViews = new List<GalaxyPillView>();
        //Adds the newly created pill view to the list of pill views within the galaxy scene.
        pillViews.Add(pillView);
        //Sets the name of the pill view's game object (Format: "Pill View #").
        pillView.gameObject.name = "Pill View " + pillViews.Count;

        //Sets the position of the pill view in order for it to not intersect with any other existing pill view.
        pillView.transform.localPosition = new Vector3(-1 * ((pillViews.Count - 1) * spacing), 0, 0);

        //Sets the pill view's displayed pill to the specified pill.
        pillView.displayedPill = pill;

        //Creates a new named render texture for the pill view's camera to render to.
        pillView.renderTexture = new RenderTexture(1080, 2160, 0);
        pillView.renderTexture.Create();
        pillView.renderTexture.name = "PillView" + pillViews.Count;

        //Returns the newly created pill view.
        return pillView;
    }

    /// <summary>
    /// Public method that should be called in order to delete a specified pill view by removing it from the list of pill views within the galaxy and shifting the rest of pill views to their new positions.
    /// </summary>
    /// <param name="pillView"></param>
    public void DeletePillView(GalaxyPillView pillView)
    {
        //Checks if the specified pill view is null or if the pill views list is null or does not contain the specified pill view and returns if so since nothing can be done.
        if (pillView == null || pillViews == null || !pillViews.Contains(pillView))
            return;

        //Grabs the specified pill view's index within the list of pill views within the galaxy.
        int removalIndex = pillViews.IndexOf(pillView);
        //Removes the specified pill view from the list of pill views within the galaxy.
        pillViews.Remove(pillView);
        //Sets the displayed pill of the pill view to null in order to remove any existing connections between the pill view and a pill.
        pillView.displayedPill = null;
        //Destroys the pill view's game object to get rid of it.
        Destroy(pillView.gameObject);

        //For loop that loops through each pill view within the list of pill views within the galaxy starting at the removal index in order to shift all of the pill views after the one deleted forward within the list properly.
        for(int pillViewIndex = removalIndex; pillViewIndex < pillViews.Count; pillViewIndex++)
        {
            //Sets the name of the pill view to match its new position within the list of pill views within the galaxy.
            pillViews[pillViewIndex].gameObject.name = "Pill View " + (pillViewIndex + 1);

            //Sets the position of the pill view to match its new position within the list of pill views within the galaxy.
            pillViews[pillViewIndex].transform.localPosition = new Vector3(-1 * (pillViewIndex * spacing), 0, 0);

            //Sets the name of the pill view's render texture to match its new position within the list of pill views within the galaxy.
            pillViews[pillViewIndex].renderTexture.name = "PillView" + (pillViewIndex + 1);
        }

        //Checks if the list of pill views within the galaxy is empty and sets it to null if so.
        if (pillViews.Count == 0)
            pillViews = null;
    }

    /// <summary>
    /// Public method that should be used in order to access the pill view at the specified index within the list of pill views within the galaxy.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GalaxyPillView GetPillViewAtIndex(int index)
    {
        //Checks if the list of pill views within the galaxy is null or if the specified index is invalid by being less than 0 or greater than the pill views list count and returns null if so since it would be impossible to return a correct pill view.
        if (pillViews == null || index < 0 || index >= pillViews.Count)
            return null;

        //Returns the pill view at the specified index within the list of pill views within the galaxy.
        return pillViews[index];
    }

    /// <summary>
    /// Public method that should be used in order to access the list of pill views that are displaying the specified pill within the galaxy scene.
    /// </summary>
    /// <param name="pill"></param>
    /// <returns></returns>
    public List<GalaxyPillView> GetPillViewsDisplayingPill(NewGalaxyPill pill)
    {
        //Initializes a list of pill views that are displaying the specified pill.
        List<GalaxyPillView> pillViewsDisplayingPill = new List<GalaxyPillView>();

        //Checks if the pill views list is null and returns the empty list if so.
        if (pillViews == null)
            return pillViewsDisplayingPill;

        //Loops through each pill view within the list of pill views within the galaxy and checks if its displayed pill is the specified pill and adds that pill view to the newly initialized list if so.
        foreach (GalaxyPillView pillView in pillViews)
            if (pillView.displayedPill == pill)
                pillViewsDisplayingPill.Add(pillView);

        //Returns the list of pill views that are displaying the specified pill.
        return pillViewsDisplayingPill;
    }
}
