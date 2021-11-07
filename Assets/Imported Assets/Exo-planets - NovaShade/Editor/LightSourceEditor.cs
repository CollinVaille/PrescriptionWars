using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LightSource))]

public class LightSourceEditor : Editor

 

{

   LightSource script;
   GameObject MyPlanet;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Link here the desired Game Object that will act like the Light Source", MessageType.Info);
		
	// if (GUILayout.Button ("Generate new Sun"))
   // {
				//GameObject Sun = new GameObject("Sun - LightSource");
			
			//   script = (LightSource) target;
			//   MyPlanet = script.gameObject;
			//   MyPlanet.GetComponent<LightSource>().importSun();
			//   MyPlanet.GetComponent<LightSource>().findSun();
			
			
  //  }

 DrawDefaultInspector ();
    }
}

// var findPlanets = GameObject.FindGameObjectsOfType(typeof(LightSource));