using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#region Editor
#if UNITY_EDITOR
using UnityEditor.Build;
#endif
#endregion

public class ArmyIconNamesLoader : SpriteNameLoader
{
    //------------------------
    //Non-inspector variables.
    //------------------------

    public static string[] armyIconNames { get; private set; }
    protected override string[] SpriteNames { get => armyIconNames; set => armyIconNames = value; }

    #region Editor
    #if UNITY_EDITOR
    private static bool armyIconNamesSaved = false;
    protected override bool SpriteNamesSaved { get => armyIconNamesSaved; set => armyIconNamesSaved = value; }
    protected override string pathFromResourcesFolder => "Army Icons";
    #endif
    #endregion
}

#region Editor
#if UNITY_EDITOR
public class ArmyIconNamesLoaderBuildProcessor : SpriteNameLoaderBuildProcessor, IPreprocessBuildWithReport
{
    protected override string textFilePath => "Assets/Read Only At Runtime Data/ArmyIconNames.txt";
    protected override SpriteNameLoader spriteLoader => new ArmyIconNamesLoader();
}
#endif
#endregion