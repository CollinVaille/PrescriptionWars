
using UnityEngine;
using UnityEditor;
using System.Collections;




public class AtmosphereEditor : ShaderGUI
{
	
	
    private bool m_AtmosphereShow = true;
    public Object m_LightSource;
	
   
    private Color32 m_TabColor = new Color (1,1,1,1);
  //  private bool m_bFirstGUI = true;

    
 
   

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)

    {
       
        Atmosphere(materialEditor, properties);
   

    }












    private void Atmosphere(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
      
	    //MaterialProperty IlluminationAmbient = ShaderGUI.FindProperty("_IlluminationAmbient", properties);
        MaterialProperty ExteriorIntensity = ShaderGUI.FindProperty("_ExteriorIntensity", properties);
        
        MaterialProperty AtmosphereColor = ShaderGUI.FindProperty("_AtmosphereColor", properties);
        MaterialProperty ExteriorSize = ShaderGUI.FindProperty("_ExteriorSize", properties);
        MaterialProperty EnableAtmosphere = ShaderGUI.FindProperty("_EnableAtmosphere", properties);

        m_AtmosphereShow = FxStyles.Header("Atmosphere", m_AtmosphereShow, EnableAtmosphere, m_TabColor);
        GUILayout.Space(5);
        if (m_AtmosphereShow)

        {
            using (new EditorGUI.DisabledGroupScope(EnableAtmosphere.floatValue == 0))
            {
                
                materialEditor.ShaderProperty(AtmosphereColor, (AtmosphereColor.displayName));
     
                materialEditor.ShaderProperty(ExteriorSize, (ExteriorSize.displayName));
                materialEditor.ShaderProperty(ExteriorIntensity, (ExteriorIntensity.displayName));
			//	materialEditor.ShaderProperty(IlluminationAmbient, (IlluminationAmbient.displayName));
                GUILayout.Space(5);
              
            }
        }

    }

    public static class FxStyles
    {
        public static GUIStyle header;
        public static GUIStyle headerCheckbox;
        public static GUIStyle headerFoldout;
        public static GUIStyle headerTab;
        public static GUIStyle labelStyle;
        public static GUIStyle HeaderTexture;
        public static GUIContent textureLabel;
        public static GUIContent textureLabel2;
        public static GUIStyle colorPicker;
        public static GUIStyle topIMG;


        static FxStyles()
        {
            // Tab header
            header = new GUIStyle("ShurikenModuleTitle");
            header.font = (new GUIStyle("Label")).font;
            header.border = new RectOffset(15, 7, 4, 4);
            header.fixedHeight = 24;
            header.contentOffset = new Vector2(20f, -2f);
            header.alignment = TextAnchor.MiddleCenter;
            header.fontSize = 12;
            header.fontStyle = FontStyle.Bold;

            // Tab header checkbox
            headerCheckbox = new GUIStyle("ShurikenCheckMark");
            headerFoldout = new GUIStyle("Foldout");

            labelStyle = new GUIStyle(EditorStyles.label);
            //labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.fontSize = 11;
            labelStyle.normal.textColor = new Color32(0, 0, 0, 255);
            //labelStyle.stretchWidth = 10;

            HeaderTexture = new GUIStyle(EditorStyles.label);
            HeaderTexture.alignment = TextAnchor.MiddleCenter;

            

            textureLabel = new GUIContent();
            textureLabel2 = new GUIContent();

            colorPicker = new GUIStyle(EditorStyles.colorField);
            colorPicker.fixedWidth = 85;
           
            topIMG = new GUIStyle();
            topIMG.alignment = TextAnchor.MiddleCenter;
        }

        public static bool Header(string title, bool foldout, Color color)
        {
            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            var auxColor = GUI.color;
            GUI.color = color;
            UnityEngine.GUI.Box(rect, title, FxStyles.header);
            GUI.color = auxColor;

            var foldoutRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            var e = Event.current;

            
           
            return foldout;
        }

      public static bool Header(string title, bool foldout, SerializedProperty enabledField, Color color)
        {
            var enabled = enabledField.boolValue;

            var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
            var auxColor = GUI.color;
            GUI.color = color;
            UnityEngine.GUI.Box(rect, title, FxStyles.header);
            GUI.color = auxColor;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            var e = Event.current;

            if (e.type == EventType.Repaint) FxStyles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);

            if (e.type == EventType.MouseDown)
            {
                const float kOffset = 2f;
                toggleRect.x -= kOffset;
                toggleRect.y -= kOffset;
                toggleRect.width += kOffset * 2f;
                toggleRect.height += kOffset * 2f;

                if (toggleRect.Contains(e.mousePosition))
                {
                    enabledField.boolValue = !enabledField.boolValue;
                    e.Use();
                }
                else if (rect.Contains(e.mousePosition))
                {
                    foldout = !foldout;
                    e.Use();
                }
            }

            return foldout;
        }

         public static bool Header(string title, bool foldout, MaterialProperty enabledField, Color color)
          {
              var enabled = (enabledField.floatValue == 1);

              var rect = GUILayoutUtility.GetRect(16f, 22f, FxStyles.header);
              var auxColor = GUI.color;
              GUI.color = color;
              UnityEngine.GUI.Box(rect, title, FxStyles.header);
              GUI.color = auxColor;

              var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
              var e = Event.current;

              if (e.type == EventType.Repaint) FxStyles.headerCheckbox.Draw(toggleRect, false, false, enabled, false);

              if (e.type == EventType.MouseDown)
              {
                  const float kOffset = 2f;
                  toggleRect.x -= kOffset;
                  toggleRect.y -= kOffset;
                  toggleRect.width += kOffset * 2f;
                  toggleRect.height += kOffset * 2f;

                  if (toggleRect.Contains(e.mousePosition))
                  {
                      enabledField.floatValue = (enabledField.floatValue == 0) ? 1 : 0;
                      e.Use();
                  }
                
              }

              return foldout;
    }
}
}





