using System;
using System.IO;
using System.Text;
using UnityEngine;

public class Initialization : MonoBehaviour
{
    public static string gameName;
    
    [SerializeField] private GameObject manager;

    [SerializeField] private GameObject panelPrefabObject;
    [SerializeField] private Transform canvasTransform;    
    private string filePath;
    private static FileStream fileStream;

    private bool argGameNull = true;

    private void Awake()
    {
        #if !UNITY_EDITOR
        string[] args = Environment.GetCommandLineArgs();

        //The ConstantVar.StartArgs use _ for example Wuthering_Waves, but window title ofcourse use space.
        foreach (string arg in args)
        {
            if (arg.StartsWith(ConstantVar.START_ARG_WUWA))
            {
                argGameNull = false;
                gameName = ConstantVar.START_ARG_WUWA.Replace("_", " ");
                filePath = Path.Combine(Application.persistentDataPath, gameName);
                if(File.Exists(filePath))
                {
                    Application.Quit();
                    return;
                }
                CreateAndLockFile();
                break;
            }
            else if (arg.StartsWith(ConstantVar.START_ARG_GENSHIN))
            {
                argGameNull = false;
                gameName = ConstantVar.START_ARG_GENSHIN.Replace("_", " ");
                filePath = Path.Combine(Application.persistentDataPath, gameName);
                if(File.Exists(filePath))
                {
                    Application.Quit();
                    return;
                }
                CreateAndLockFile();
                break;
            }
            else if (arg.StartsWith(ConstantVar.START_ARG_HSR))
            {
                argGameNull = false;
                gameName = ConstantVar.START_ARG_HSR.Replace("_", " ");
                filePath = Path.Combine(Application.persistentDataPath, gameName);
                if(File.Exists(filePath))
                {
                    Application.Quit();
                    return;
                }
                CreateAndLockFile();
                break;
            }
            else if (arg.StartsWith(ConstantVar.START_ARG_ZZZ))
            {
                argGameNull = false;
                gameName = ConstantVar.START_ARG_ZZZ.Replace("_", " ");
                filePath = Path.Combine(Application.persistentDataPath, gameName);
                if(File.Exists(filePath))
                {
                    Application.Quit();
                    return;
                }
                CreateAndLockFile();
                break;
            }
        }

        if(argGameNull)
        {
            Application.Quit();
            return;
        }
        #endif
        
        #if UNITY_EDITOR
        gameName = "Wuthering_Waves";
        #endif
        
        manager.SetActive(true);
        Instantiate(panelPrefabObject, canvasTransform);
    }

    private void OnApplicationQuit()
    {
        ReleaseAndDeleteFile();
    }

    public void CreateAndLockFile()
    {
        try
        {
            // Create or overwrite the file and open it with exclusive lock
            fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            string content = "The app running";
            byte[] data = Encoding.UTF8.GetBytes(content);
            fileStream.Write(data, 0, data.Length);
            fileStream.Flush();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error creating or locking file: " + ex.Message);
        }
    }

    public void ReleaseAndDeleteFile()
    {
        try
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error releasing or deleting file: " + ex.Message);
        }
    }
}
