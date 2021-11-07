
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;



public class PlanetEditor : ShaderGUI
{
	
  //  private bool m_IlluminationShow = true;
    private bool m_BaseColorShow = true;
    private bool m_WaterShow = true;
    private bool m_CitiesShow = true;
    private bool m_CloudsShow = true;
    private bool m_AtmosphereShow = true;
    public Object m_LightSource;

    public static string _filePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
    public static string _assetsToFilePath = _filePath.Substring(Directory.GetCurrentDirectory().Length + 1).Replace("\\", "/");
    public static string _texturesPath = _assetsToFilePath.Substring(0, _assetsToFilePath.Length - "/Editor/PlanetEditor.cs".Length) + "/Sources Files/Textures/";

    public Texture2D Clouds_Light = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "Clouds_Light.tga", typeof(Texture2D));
    public Texture2D Clouds_Average = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "Clouds_Average.tga", typeof(Texture2D));
    public Texture2D Clouds_Heavy = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "Clouds_Heavy.tga", typeof(Texture2D));

    public Texture2D Clouds_Light_N = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "Clouds_Light_N.tga", typeof(Texture2D));
    public Texture2D Clouds_Average_N = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "Clouds_Average_N.tga", typeof(Texture2D));
    public Texture2D Clouds_Heavy_N = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "Clouds_Heavy_N.tga", typeof(Texture2D));

    public Texture2D headerTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(_texturesPath + "headerTexture.png", typeof(Texture2D));

    public enum OPTIONS
    {
        Average,
        CloseSmall,
        FarLarge,
    }

    public enum OPTIONS2
    {
        HeavyClouds = 0,
        AverageClouds = 1,
        LightClouds = 2,
        Custom = 3,

    }

    private OPTIONS AtmosphereSizes;
    private OPTIONS2 CloudsText;

    public float Size_Close = 1.1f;
    public float Size_Middle = 4f;
    public float Size_Far = 48;

    public float Sharp_Close = 1.75f;
    public float Sharp_Middle = 3.5f;
    public float Sharp_Far = 1.75f;
    private Color32 m_TabColor = new Color (1,1,1,1);
    private bool m_bFirstGUI = true;



    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)

    {
       

        if (m_bFirstGUI)
        {
            m_bFirstGUI = false;

            MaterialProperty EnumFloat = ShaderGUI.FindProperty("_EnumFloat", properties);
            CloudsText = (OPTIONS2)EnumFloat.floatValue;

        }

       
        
	   
	    GUILayout.Label(headerTexture, FxStyles.HeaderTexture);
        Atmosphere(materialEditor, properties);
        Clouds(materialEditor, properties);
        Cities(materialEditor, properties);
        Water(materialEditor, properties);
        BaseColor(materialEditor, properties);

    }


    private void Water(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
      
        MaterialProperty NecessaryWaterMask = ShaderGUI.FindProperty("_NecessaryWaterMask", properties);
        MaterialProperty WaterColor = ShaderGUI.FindProperty("_WaterColor", properties);  
        MaterialProperty SpecularIntensity = ShaderGUI.FindProperty("_SpecularIntensity", properties);
        MaterialProperty EnableWater = ShaderGUI.FindProperty("_EnableWater", properties);
        m_WaterShow = FxStyles.Header("Water", m_WaterShow, EnableWater, m_TabColor);
        GUILayout.Space(5);
    
        if (m_WaterShow)

        {
            using (new EditorGUI.DisabledGroupScope(EnableWater.floatValue == 0))
            {

                EditorGUILayout.BeginHorizontal();
                {

                    GUILayout.Label("Water Mask", FxStyles.labelStyle);
                    materialEditor.TexturePropertySingleLine(FxStyles.textureLabel, NecessaryWaterMask);

                }

                EditorGUILayout.EndHorizontal();
                materialEditor.ShaderProperty(WaterColor, (WaterColor.displayName));
                materialEditor.ShaderProperty(SpecularIntensity, (SpecularIntensity.displayName));
        

            }
        }
    }


 private void Atmosphere(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
      
	 
	    MaterialProperty IlluminationAmbient = ShaderGUI.FindProperty("_IlluminationAmbient", properties);
        MaterialProperty IlluminationBoost = ShaderGUI.FindProperty("_IlluminationBoost", properties);
        MaterialProperty InteriorIntensity = ShaderGUI.FindProperty("_InteriorIntensity", properties);
        MaterialProperty AtmosphereColor = ShaderGUI.FindProperty("_AtmosphereColor", properties);
        MaterialProperty EnableAtmosphere = ShaderGUI.FindProperty("_EnableAtmosphere", properties);
        MaterialProperty InteriorSize = ShaderGUI.FindProperty("_InteriorSize", properties);
		MaterialProperty SkyblendA = ShaderGUI.FindProperty("_SkyblendA", properties);
        m_AtmosphereShow = FxStyles.Header("Atmosphere", m_AtmosphereShow, EnableAtmosphere, m_TabColor);
        GUILayout.Space(5);
        if (m_AtmosphereShow)

        {
            using (new EditorGUI.DisabledGroupScope(EnableAtmosphere.floatValue == 0))
            {
                EditorGUILayout.HelpBox("Exterior atmosphere is on a separate mesh", MessageType.Info);
                
              
				
				materialEditor.ShaderProperty(AtmosphereColor, (AtmosphereColor.displayName));
                materialEditor.ShaderProperty(InteriorSize, (InteriorSize.displayName));
                materialEditor.ShaderProperty(InteriorIntensity, (InteriorIntensity.displayName)); 
				materialEditor.ShaderProperty(IlluminationAmbient, (IlluminationAmbient.displayName));
				materialEditor.ShaderProperty(IlluminationBoost, (IlluminationBoost.displayName));
				materialEditor.ShaderProperty(SkyblendA, (SkyblendA.displayName));
                GUILayout.Space(5);
              
            }
        }

    }

    private void BaseColor(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty ColorTexture = ShaderGUI.FindProperty("_ColorTexture", properties);
        MaterialProperty Normals = ShaderGUI.FindProperty("_Normals", properties);
        MaterialProperty NormalsIntensity = ShaderGUI.FindProperty("_NormalsIntensity", properties);
        m_BaseColorShow = FxStyles.Header("Base", m_BaseColorShow, m_TabColor);
		
        GUILayout.Space(5);
        if (m_BaseColorShow)

        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Color Texture    ", FxStyles.labelStyle);
                materialEditor.TexturePropertySingleLine(FxStyles.textureLabel, ColorTexture);
            }
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginHorizontal();
            {
                
                GUILayout.Label("Normals Texture", FxStyles.labelStyle);
                materialEditor.TexturePropertySingleLine(FxStyles.textureLabel, Normals);
            }
            EditorGUILayout.EndHorizontal();
            materialEditor.ShaderProperty(NormalsIntensity, (NormalsIntensity.displayName));
          
            GUILayout.Space(5);
			EditorGUILayout.HelpBox("To have the planet reacting to a light source, check to have the LightScource script added on the parent, with a game object linked to it.", MessageType.Info);
			
       if (GUILayout.Button("Link to the Asset's page to give a review"))
		{	
         Application.OpenURL("https://assetstore.unity.com/packages/slug/163061/");
	   }
        }
		
    }


    private void Cities(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty Citiescolor = ShaderGUI.FindProperty("_Citiescolor", properties);
        MaterialProperty CitiesTexture = ShaderGUI.FindProperty("_CitiesTexture", properties);
        MaterialProperty CitiesDetail = ShaderGUI.FindProperty("_CitiesDetail", properties);
        MaterialProperty EnableCities = ShaderGUI.FindProperty("_EnableCities", properties);

        m_CitiesShow = FxStyles.Header("Cities", m_CitiesShow, EnableCities, m_TabColor);
        GUILayout.Space(5);
        if (m_CitiesShow)
        {
            using (new EditorGUI.DisabledGroupScope(EnableCities.floatValue == 0))
            {
                
          
                materialEditor.ShaderProperty(Citiescolor, (Citiescolor.displayName));
                materialEditor.ShaderProperty(CitiesDetail, (CitiesDetail.displayName));
                GUILayout.Space(5);
            }
        }
    }

    private void Clouds(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty ColorA = ShaderGUI.FindProperty("_ColorA", properties);
        MaterialProperty CloudsTexture = ShaderGUI.FindProperty("_CloudsTexture", properties);
        MaterialProperty CloudSpeed = ShaderGUI.FindProperty("_CloudSpeed", properties);
        MaterialProperty EnableClouds = ShaderGUI.FindProperty("_EnableClouds", properties);
        MaterialProperty ShadowsSharpness = ShaderGUI.FindProperty("_ShadowsSharpness", properties);
        MaterialProperty ShadowsXOffset = ShaderGUI.FindProperty("_ShadowsXOffset", properties);
        MaterialProperty ShadowsYOffset = ShaderGUI.FindProperty("_ShadowsYOffset", properties);
        MaterialProperty CloudsNormals = ShaderGUI.FindProperty("_CloudsNormals", properties);
        MaterialProperty ReliefIntensity = ShaderGUI.FindProperty("_ReliefIntensity", properties);
		MaterialProperty ShadowColorA = ShaderGUI.FindProperty("_ShadowColorA", properties);
		MaterialProperty ReliefSmoothness = ShaderGUI.FindProperty("_ReliefSmoothness", properties);
        MaterialProperty EnumFloat = ShaderGUI.FindProperty("_EnumFloat", properties);

        m_CloudsShow = FxStyles.Header("Clouds", m_CloudsShow, EnableClouds, m_TabColor);
        GUILayout.Space(5);
        if (m_CloudsShow)
        {
            using (new EditorGUI.DisabledGroupScope(EnableClouds.floatValue == 0))
            {

                CloudsText = (OPTIONS2)EditorGUILayout.EnumPopup("Clouds Types", CloudsText);
                switch (CloudsText)
                {
                    case OPTIONS2.LightClouds:
                        CloudsTexture.textureValue = Clouds_Light;
                        CloudsNormals.textureValue = Clouds_Light_N;
                        CloudsText = OPTIONS2.LightClouds;
                        break;
                    case OPTIONS2.AverageClouds:
                        CloudsTexture.textureValue = Clouds_Average;
                        CloudsNormals.textureValue = Clouds_Average_N;
                        break;
                    case OPTIONS2.HeavyClouds:
                        CloudsTexture.textureValue = Clouds_Heavy;
                        CloudsNormals.textureValue = Clouds_Heavy_N;
                        break;
                    case OPTIONS2.Custom:
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Clouds Texture", FxStyles.labelStyle);
                            materialEditor.TexturePropertySingleLine(FxStyles.textureLabel, CloudsTexture);

                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Clouds Normals", FxStyles.labelStyle);
                            materialEditor.TexturePropertySingleLine(FxStyles.textureLabel, CloudsNormals);

                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                }

                EnumFloat.floatValue = (float)CloudsText;



                materialEditor.ShaderProperty(CloudSpeed, (CloudSpeed.displayName));
                materialEditor.ShaderProperty(ColorA, (ColorA.displayName));
				materialEditor.ShaderProperty(ReliefIntensity, (ReliefIntensity.displayName));
				materialEditor.ShaderProperty(ReliefSmoothness, (ReliefSmoothness.displayName));
				materialEditor.ShaderProperty(ShadowColorA, (ShadowColorA.displayName));
                materialEditor.ShaderProperty(ShadowsXOffset, (ShadowsXOffset.displayName));
                materialEditor.ShaderProperty(ShadowsYOffset, (ShadowsYOffset.displayName));
                materialEditor.ShaderProperty(ShadowsSharpness, (ShadowsSharpness.displayName));
               
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





