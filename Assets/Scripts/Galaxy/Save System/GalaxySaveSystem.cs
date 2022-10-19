using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public static class GalaxySaveSystem
{
    /// <summary>
    /// This public static function takes the galaxy manager as an argument and should be called in order to save the galaxy data into a prescription wars save file.
    /// </summary>
    /// <param name="galaxyManager"></param>
    public static void SaveGalaxy()
    {
        //Checks that the current scene open is the galaxy scene and returns if not.
        if(!SceneManager.GetActiveScene().name.Equals("New Galaxy"))
        {
            Debug.LogWarning("Galaxy cannot be saved from a scene besides the galaxy scene. Will now return and not save anything.");
            return;
        }

        //Creates the binary formatter and specifies the file path.
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + NewGalaxyManager.saveName + ".prescriptionwarssave";
        FileStream stream = new FileStream(path, FileMode.Create);

        //Creates the galaxy data object that will be saved using the galaxy manager.
        GalaxyData data = new GalaxyData(NewGalaxyManager.galaxyManager);

        //Saves the data and closes the file stream in order to avoid any errors.
        formatter.Serialize(stream, data);
        stream.Close();
    }

    /// <summary>
    /// This public static function should be called in order to obtain the galaxy data of a prescription wars save file.
    /// </summary>
    /// <param name="saveName"></param>
    /// <returns></returns>
    public static GalaxyData LoadGalaxy(string saveName)
    {
        //Determines the exact path that the specified save file is stored at.
        string path = Application.persistentDataPath + "/" + saveName + ".prescriptionwarssave";
        //The specified save file exists.
        if (File.Exists(path))
        {
            //Creates the binary formatter and file stream objects.
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //Uses the previously created binary formatter and file stream to obtain the galaxy data from the specified save file.
            GalaxyData data = formatter.Deserialize(stream) as GalaxyData;
            stream.Close();

            //Returns the galaxy data from the specified save file.
            return data;
        }
        //The specified save file does not exist.
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }

    }
}
