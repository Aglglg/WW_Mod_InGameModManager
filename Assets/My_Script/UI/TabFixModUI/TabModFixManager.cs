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
using System.Threading.Tasks;
using UnityEngine.UI;
public class TabModFixManager : MonoBehaviour
{
    [SerializeField] private float animationDuration;
    public TMP_InputField modPathField;
    [SerializeField] private CanvasGroup gameListCanvasGroup;
    [SerializeField] private CanvasGroup expandedGameListCanvasGroup;
    [SerializeField] private Transform[] contentsRootTransform;
    [SerializeField] private GameObject itemFixPrefab;
    [SerializeField] private GameObject[] expandedGameObjects;

    [SerializeField] private TextMeshProUGUI textModFixLoadingInfo;
    private string textInfoAfterLoading;

    private List<ModFixData> modFixDatas = new List<ModFixData>();
    private ModFixGame selectedGame;    

    private void OnEnable()
    {
    }

    private void OnDisable()
    {

    }

    #region MODS PATH
    //Called from Button
    public void SelectModPath()
    {
        string[] folder = OpenFileExplorer.OpenFolder("Select a mod folder to be fixed");
        if(folder.Length > 0)
        {
            modPathField.text = folder[0];
        }
    }
    #endregion

    //Called from dropdown button
    public void ChangeSelectedGame(int selectedGameIndex)
    {
        selectedGame = (ModFixGame)selectedGameIndex;
        Debug.Log(Application.persistentDataPath);
        GetFixesList();
    }
    //Called from button & PlayerInput InputSystem
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
            modFixDatas.Clear();
            foreach (Transform child in contentsRootTransform[(int)selectedGame])
            {
                Destroy(child.gameObject);
            }
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

    #region Retrive fixes data
    private async void GetFixesList()
    {
        expandedGameObjects[(int)selectedGame].GetComponentInChildren<Button>().interactable = false;

        string cachedDir = Path.Join(Application.persistentDataPath, ConstantVar.PATH_CACHED_FIXES, selectedGame.ToString());

        if (Directory.Exists(cachedDir))
        {
            // Load cached JSON files asynchronously
            Debug.Log("Loading Json");
            textModFixLoadingInfo.text = "Loading";
            string[] fixFiles = Directory.GetFiles(cachedDir, "*.json");
            foreach (string file in fixFiles)
            {
                await LoadModFixAsync(file);
            }
            Debug.Log("Loaded");
            textModFixLoadingInfo.text = textInfoAfterLoading;
            InstantiateModFixPrefabs();
        }
        else
        {
            // Download JSON files from GitHub
            Debug.Log("Downloading");
            textModFixLoadingInfo.text = "Downloading";
            await DownloadJsonFiles(ConstantVar.LINK_PATH_MODFIXES[(int)selectedGame]);
            Debug.Log("Finished");
            textModFixLoadingInfo.text = textInfoAfterLoading;
            InstantiateModFixPrefabs();
        }
        expandedGameObjects[(int)selectedGame].GetComponentInChildren<Button>().interactable = true;
    }

    private async Task DownloadJsonFiles(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("User-Agent", "UnityWebRequest");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching file list: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            await ProcessFileListAsync(jsonResponse);
        }
    }

    private async Task ProcessFileListAsync(string jsonResponse)
    {
        var contents = JsonConvert.DeserializeObject<List<GitHubContent>>(jsonResponse);
        Debug.Log(jsonResponse);

        // Filter JSON files
        var jsonFiles = contents.Where(c => c.type == "file" && c.name.EndsWith(".json")).ToArray();

        // Download the content of each JSON file asynchronously
        foreach (var file in jsonFiles)
        {
            await DownloadJsonFileAsync(file);
        }
    }

    private async Task DownloadJsonFileAsync(GitHubContent gitHubContent)
    {
        UnityWebRequest request = UnityWebRequest.Get(gitHubContent.download_url);
        request.SetRequestHeader("User-Agent", "UnityWebRequest");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading JSON file: " + request.error);
        }
        else
        {
            string jsonContent = request.downloadHandler.text;
            string cachedDir = Path.Join(Application.persistentDataPath, ConstantVar.PATH_CACHED_FIXES, selectedGame.ToString());

            if (!Directory.Exists(cachedDir))
            {
                Directory.CreateDirectory(cachedDir);
            }

            string filePath = Path.Join(cachedDir, gitHubContent.name);
            await WriteFileAsync(filePath, jsonContent);
            
            await LoadModFixAsync(filePath);
        }
    }

    private async Task WriteFileAsync(string filePath, string content)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            await writer.WriteAsync(content);
        }
        Debug.Log("File written: " + filePath);
    }

    private async Task LoadModFixAsync(string file)
    {
        string jsonData = await ReadFileAsync(file);
        if(file.EndsWith(ConstantVar.FILE_FIX_LOG))
        {
            textInfoAfterLoading = jsonData;
        }
        else
        {
            try
            {
                ModFixData fixData = JsonConvert.DeserializeObject<ModFixData>(jsonData);
                modFixDatas.Add(fixData);
            }
            catch (JsonSerializationException ex)
            {
                Debug.LogError("JSON deserialization error: " + ex.Message);
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError("JSON parsing error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogError("An unexpected error occurred: " + ex.Message);
            }
        }
        
    }

    private async Task<string> ReadFileAsync(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            return await reader.ReadToEndAsync();
        }
    }

    private void InstantiateModFixPrefabs()
    {
        // Sort the ModFixData objects by modifiedDate (newest first)
        var sortedModFixDatas = modFixDatas.OrderByDescending(x => x.modifiedDate).ToList();

        // Instantiate prefabs for each ModFixData
        foreach (var fixData in sortedModFixDatas)
        {
            GameObject itemFixInstantiated = Instantiate(itemFixPrefab, contentsRootTransform[(int)selectedGame]);
            ItemFixHandler itemFixHandler = itemFixInstantiated.GetComponent<ItemFixHandler>();
            itemFixHandler.modFixData = fixData;
            itemFixHandler.LoadModFixData();
            itemFixHandler.modFixManager = this;
        }
    }
    #endregion
}

public class GitHubContent
{
    public string name;
    public string type; // "dir" for folders, "file" for files
    public string download_url;
    public string path;
}

    // Directory.Delete(cachePath, recursive: true);
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
