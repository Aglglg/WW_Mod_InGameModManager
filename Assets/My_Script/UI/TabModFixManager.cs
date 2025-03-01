using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System;
using TMPro;
using DG.Tweening;

public class TabModFixManager : MonoBehaviour
{
    [SerializeField] private float animationDuration;
    [SerializeField] private CanvasGroup gameListCanvasGroup;
    [SerializeField] private CanvasGroup expandedGameListCanvasGroup;
    [SerializeField] private GameObject[] expandedGameObjects;
    private int selectedGame;

    void Start()
    {
        // CreateAndLockFile();
    }

    void OnApplicationQuit()
    {
        // ReleaseAndDeleteFile();
    }

    private void TestLoadingAndAddingModFix()
    {
        List<ModFixData> modFixDatas = new List<ModFixData>();
        Dictionary<string, string> pairs = new Dictionary<string, string>();
        pairs.Add("AAAA", "BBBB");
        ModFixData modFixData = new ModFixData
        {
            fixName = "test fix",
            dateTime = "24/2/2025",
            modFixGame = ModFixGame.WUWA,
            modFixType = ModFixType.HashReplacement,
            note = "test note",
            submitter = "test submitter",
            hashpair = pairs
        };
        modFixDatas.Add(modFixData);
        string jsonData = JsonConvert.SerializeObject(modFixDatas, Formatting.Indented);;
        Debug.Log(jsonData);

        List<ModFixData> deserializedDatas = JsonConvert.DeserializeObject<List<ModFixData>>(jsonData);
        Debug.Log(deserializedDatas[0].hashpair.ContainsKey("AAAA"));
    }

    //Called from dropdown button
    public void ChangeSelectedGame(int selectedGameIndex)
    {
        selectedGame = selectedGameIndex;
    }
    public void ExpandModFix(bool isExpand)
    {
        if(isExpand)
        {
            gameListCanvasGroup.DOFade(0, animationDuration).OnComplete(
                () =>
                {
                    foreach (GameObject go in expandedGameObjects)
                    {
                        go.SetActive(false);
                    }
                    expandedGameObjects[selectedGame].SetActive(true);

                    gameListCanvasGroup.gameObject.SetActive(false);
                    expandedGameListCanvasGroup.alpha = 0;
                    expandedGameListCanvasGroup.gameObject.SetActive(true);
                    expandedGameListCanvasGroup.DOFade(1, animationDuration);
                }
            );
        }
        else
        {
            expandedGameListCanvasGroup.DOFade(0, animationDuration).OnComplete(
                () =>
                {
                    expandedGameListCanvasGroup.gameObject.SetActive(false);
                    gameListCanvasGroup.alpha = 0;
                    gameListCanvasGroup.gameObject.SetActive(true);
                    gameListCanvasGroup.DOFade(1, animationDuration);
                }
            );
        }
    }

    //private static FileStream fileStream;
    //private static string filePath;

    // public static void CreateAndLockFile()
    // {
    //     // Define the file path in Application.persistentDataPath
    //     filePath = Path.Combine(Application.persistentDataPath, "WuWa Mod Manager Still Running.txt");
    //     Debug.Log(filePath);

    //     try
    //     {
    //         // Create or overwrite the file and open it with exclusive lock
    //         fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

    //         // Write some initial data
    //         string content = "That means \n";
    //         byte[] data = Encoding.UTF8.GetBytes(content);
    //         fileStream.Write(data, 0, data.Length);
    //         fileStream.Flush();

    //         Debug.Log("File created and locked: " + filePath);
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.LogError("Error creating or locking file: " + ex.Message);
    //     }
    // }

    // public static void ReleaseAndDeleteFile()
    // {
    //     try
    //     {
    //         if (fileStream != null)
    //         {
    //             fileStream.Close();
    //             fileStream.Dispose();
    //             fileStream = null;
    //         }

    //         if (File.Exists(filePath))
    //         {
    //             File.Delete(filePath);
    //             Debug.Log("File deleted: " + filePath);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.LogError("Error releasing or deleting file: " + ex.Message);
    //     }
    // }
}
