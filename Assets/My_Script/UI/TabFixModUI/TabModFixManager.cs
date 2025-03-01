using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using Unity.VisualScripting;

public class TabModFixManager : MonoBehaviour
{
    [SerializeField] private float animationDuration;
    [SerializeField] private CanvasGroup gameListCanvasGroup;
    [SerializeField] private CanvasGroup expandedGameListCanvasGroup;
    [SerializeField] private GameObject[] expandedGameObjects;

    private ModFixGame selectedGame;
    private Coroutine downloadDataCoroutine;
    private List<string> modFixDatasJson = new List<string>();

    private void TestLoadingAndAddingModFix()
    {
        List<ModFixData> modFixDatas = new List<ModFixData>();
        Dictionary<string, string> pairs = new Dictionary<string, string>();
        pairs.Add("AAAA", "BBBB");
        ModFixData modFixData = new ModFixData
        {
            modFixGame = ModFixGame.Wuwa,
            modFixType = ModFixType.HashReplacement,
            note = "<align=center><b>Fix Ultra Performance & Performance Graphic Setting\nby <link=\"https://gamebanana.com/tools/18999\"><u>@agulag</u></link></b>01/03/2025</align>\n\nMost characters/entity mods only works in Quality. Using Ultra Performance/Performance game setting will give you more FPS because textures have lower resolution (great for low VRAM computer).\nMake sure your mod already works fine in 'Quality'.\n\n<align=center><link=\"FIX\"><b><color=#3BBBFF><u>FIX</u></color></b></link></align>",
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
        selectedGame = (ModFixGame)selectedGameIndex;
        GetFixesList();
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
                    expandedGameObjects[(int)selectedGame].SetActive(true);

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

    private void GetFixesList()
    {
        StartCoroutine(DownloadJsonFilesFromGitHub(ConstantVar.LINK_PATH_MODFIXES[(int)selectedGame]));
    }

    IEnumerator DownloadJsonFilesFromGitHub(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        request.SetRequestHeader("User-Agent", "UnityWebRequest");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching file list: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            ProcessFileList(jsonResponse);
        }
    }

    void ProcessFileList(string jsonResponse)
    {
        var contents = JsonConvert.DeserializeObject<List<GitHubContent>>(jsonResponse);

        // Filter JSON files
        var jsonFiles = contents.Where(c => c.type == "file" && c.name.EndsWith(".json")).ToArray();

        //Download the content of each JSON file
        foreach (var file in jsonFiles)
        {
            // StartCoroutine(DownloadJsonFile(file.download_url));
        }
    }

    IEnumerator DownloadJsonFile(string downloadUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(downloadUrl);

        request.SetRequestHeader("User-Agent", "UnityWebRequest");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading JSON file: " + request.error);
        }
        else
        {
            string jsonContent = request.downloadHandler.text;
            modFixDatasJson.Add(jsonContent);
            Debug.Log("Downloaded JSON: " + jsonContent);
        }
    }
}

public class GitHubContent
{
    public string name;
    public string type; // "dir" for folders, "file" for files
    public string download_url;
    public string path;
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
