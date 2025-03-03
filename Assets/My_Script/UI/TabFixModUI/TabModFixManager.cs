using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using TMPro;
using DG.Tweening;
using UnityEngine.Networking;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class TabModFixManager : MonoBehaviour
{
    [SerializeField] private float animationDuration;


    [Header("Mod Path")]
    //also called from ItemFixHandler
    public TMP_InputField modPathField;


    [Header("\nGameList")]
    [SerializeField] private CanvasGroup canvasGroupGameList;
    [SerializeField] private GameObject gameObjectGameList;


    [Header("\nExpandedGame/FixList")]
    [SerializeField] private CanvasGroup canvasGroupGameExpanded;
    [SerializeField] private GameObject gameObjectExpandedGame;
    [SerializeField] private TextMeshProUGUI textModFixLoadingInfo;
    [SerializeField] private GameObject[] expandedGames;
    [SerializeField] private Transform[] itemFixContentParentTransform;
    [SerializeField] private GameObject itemFixPrefab;
    private string textInfoAfterLoading;
    private ModFixGame selectedGame;
    List<ModFixData> modFixDatas = new List<ModFixData>();


    [Header("\nLogging")]
    [SerializeField] private ModFixLogToUI modFixLogToUI;
    [SerializeField] private CanvasGroup canvasGroupLogUI;
    [SerializeField] private GameObject gameObjectLogUI;
    [SerializeField] private Button doneButton;

    private void Awake()
    {
        //For log to ui
        Application.logMessageReceived += modFixLogToUI.LogCallback;
        ModFixer.Initialize();
    }

    private void InstantiateModFixPrefabs()
    {
        // Sort the ModFixData objects by modifiedDate (newest first)
        var sortedModFixDatas = modFixDatas.OrderByDescending(x => x.modifiedDate).ToList();

        // Instantiate prefabs for each ModFixData
        foreach (var fixData in sortedModFixDatas)
        {
            GameObject itemFixInstantiated = Instantiate(itemFixPrefab, itemFixContentParentTransform[(int)selectedGame]);
            itemFixInstantiated.GetComponent<ItemFixHandler>().modFixData = fixData;
            itemFixInstantiated.GetComponent<ItemFixHandler>().tabModFixManager = this;
        }
    }

    #region ModPath
    //Called from button modpath folder icon
    public void SelectModPath()
    {
        string[] folder = OpenFileExplorer.OpenFolder("Select a mod folder to be fixed");
        if(folder.Length > 0)
        {
            modPathField.text = folder[0];
        }
    }
    #endregion

    #region GameList
    //Called from button
    public void ButtonGameList(int game)
    {
        selectedGame = (ModFixGame)game;
        canvasGroupGameList.DOFade(0, animationDuration).OnComplete(
            () => {
                gameObjectGameList.SetActive(false);
                canvasGroupGameExpanded.alpha = 0;
                gameObjectExpandedGame.SetActive(true);
                expandedGames[game].SetActive(true);
                canvasGroupGameExpanded.DOFade(1, animationDuration);
                GetFixesList();
            }
        );
    }
    #endregion

    #region FixList/ExpandedGame
    public void ButtonCloseFixList()
    {
        foreach (Transform child in itemFixContentParentTransform[(int)selectedGame])
        {
            Destroy(child.gameObject);
        }
        modFixDatas.Clear();
        
        canvasGroupGameExpanded.DOFade(0, animationDuration).OnComplete(
            () => {
                gameObjectExpandedGame.SetActive(false);
                canvasGroupGameList.alpha = 0;
                gameObjectGameList.SetActive(true);
                expandedGames[(int)selectedGame].SetActive(false);
                canvasGroupGameList.DOFade(1, animationDuration);
            }
        );
    }
    #endregion

    #region  LOG UI
    //Called from ItemFixHandler
    public void ToggleDoneButton(bool isInteractible)
    {
        if(isInteractible)
        {
            doneButton.interactable = true;
        }
        else
        {
            doneButton.interactable = false;
        }
    }
    public void ShowLog()
    {
        //From ItemFixHandler
        canvasGroupGameExpanded.DOFade(0, animationDuration).OnComplete( () =>
                {
                    gameObjectExpandedGame.SetActive(false);
                    canvasGroupLogUI.alpha = 0;
                    gameObjectLogUI.SetActive(true);
                    canvasGroupLogUI.DOFade(1, animationDuration);
                }
            );
    }
    //From Button
    public void DoneButton()
    {
        modFixLogToUI.logText.text = "";
        canvasGroupLogUI.DOFade(0, animationDuration).OnComplete( () =>
                {
                    gameObjectLogUI.SetActive(false);
                    canvasGroupGameExpanded.alpha = 0;
                    gameObjectExpandedGame.SetActive(true);
                    canvasGroupGameExpanded.DOFade(1, animationDuration);
                }
            );
        //DO RELOAD MOD HERE
    }
    #endregion

    #region Retrive fixes data
    private async void GetFixesList()
    {
        //Back/close button
        expandedGames[(int)selectedGame].GetComponentInChildren<Button>().interactable = false;

        string cachedDir = Path.Join(Application.persistentDataPath, ConstantVar.PATH_CACHED_FIXES, selectedGame.ToString());

        if (Directory.Exists(cachedDir))
        {
            // Load cached JSON files asynchronously
            textModFixLoadingInfo.text = "Loading";
            string[] fixFiles = Directory.GetFiles(cachedDir, "*.json");
            foreach (string file in fixFiles)
            {
                await LoadModFixAsync(file);
            }
            textModFixLoadingInfo.text = textInfoAfterLoading;
            InstantiateModFixPrefabs();
        }
        else
        {
            // Download JSON files from GitHub
            textModFixLoadingInfo.text = "Downloading";
            await DownloadJsonFiles(ConstantVar.LINK_PATH_MODFIXES[(int)selectedGame]);
            textModFixLoadingInfo.text = textInfoAfterLoading;
            InstantiateModFixPrefabs();
        }
        //Back/close button
        expandedGames[(int)selectedGame].GetComponentInChildren<Button>().interactable = true;
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
                Debug.LogError("Error Deserialize JSON: " + ex.Message);
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
