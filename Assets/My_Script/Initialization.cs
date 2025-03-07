using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-game" && i + 1 < args.Length)
            {
                argGameNull = false;
                gameName = args[i + 1];
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

        SetToEfficiencyMode();
        
        #if UNITY_EDITOR
        gameName = "Wuthering Waves";
        #endif
        
        manager.SetActive(true);
        Instantiate(panelPrefabObject, canvasTransform);
    }



#region Set To Efficiency
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetThreadPriority(IntPtr hThread, int nPriority);
    private const int THREAD_PRIORITY_IDLE = -15;

    private void SetToEfficiencyMode()
    {
        Process currentProcess = Process.GetCurrentProcess();
        currentProcess.PriorityClass = ProcessPriorityClass.Idle;

        IntPtr threadHandle = GetCurrentThread();
        SetThreadPriority(threadHandle, THREAD_PRIORITY_IDLE);
    }
#endregion



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
            UnityEngine.Debug.LogError("Error creating or locking file: " + ex.Message);
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
            UnityEngine.Debug.LogError("Error releasing or deleting file: " + ex.Message);
        }
    }
}
