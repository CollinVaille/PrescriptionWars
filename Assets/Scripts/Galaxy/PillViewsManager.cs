using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillViewsManager : MonoBehaviour
{
    //Non-inspector variables.

    public static GameObject pillViewPrefab = null;

    private static List<PillView> pillViews = new List<PillView>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static PillView GetNewPillView()
    {
        //Instantiates a new pill view from the pill view prefab.
        GameObject pillView = Instantiate(pillViewPrefab);
        PillView pillViewScript = pillView.GetComponent<PillView>();

        //pillView.transform.SetParent();
        //Adds the new pill view to the list of pill views.
        pillViews.Add(pillViewScript);
        //Informs all of the pill views that a pill view has been added or deleted.
        OnPillViewAdditionOrDeletion();

        //Returns the newly created pill view.
        return pillViewScript;
    }

    public static void DeletePillView(PillView pillView)
    {
        //Removes the pill view from the list of pill views.
        pillViews.Remove(pillView);

        //Destroys the game object of the pill view.
        Destroy(pillView.gameObject);

        //Informs the rest of the pill views that a pill view has been added or deleted.
        OnPillViewAdditionOrDeletion();
    }

    //This method should be called whenever a new pill view is created or a pill view is deleted.
    private static void OnPillViewAdditionOrDeletion()
    {
        for(int pillViewIndex = 0; pillViewIndex < pillViews.Count; pillViewIndex++)
        {
            pillViews[pillViewIndex].OnOtherPillViewAdditionOrDeletion(pillViewIndex);
        }
    }
}
