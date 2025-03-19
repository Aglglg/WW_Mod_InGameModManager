using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using DG.Tweening;
using UnityEngine.Networking;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WindowsInput;
public class TabModFixManager : MonoBehaviour
{
    [SerializeField] private float animationDuration;


    [Header("Mod Path")]
    //also used from ItemFixHandler
    public TMP_InputField modPathField;
    [SerializeField] private TextMeshProUGUI modPathTitleText;


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

    [Header("Revert&ApplyPermanent")]
    [SerializeField] private GameObject buttonRevertFix;
    [SerializeField] private GameObject buttonApplyPermanent;
    [SerializeField] private GameObject buttonConfirmRevert;
    [SerializeField] private GameObject buttonConfirmApply;
    [SerializeField] private TextMeshProUGUI textInfoRevertAndApplyPermanent;
    [SerializeField] private Button buttonInfoRevertAndApplyPermanent;

    private void Awake()
    {
        //For log to ui
        Application.logMessageReceived += modFixLogToUI.LogCallback;
        ModFixer.Initialize();

        //For getting latest mod fixes, the mod fix retrive data from cloud once only, and will use cached fixes except app quit & this called once only.
        DeleteModFixCache();
    }
    private void OnEnable()
    {
        modPathField.onDeselect.AddListener(OnModPathDeselect);
    }
    private void OnDisable()
    {
        modPathField.onDeselect.RemoveListener(OnModPathDeselect);
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
    
    #region RevertFixes
    public void ButtonRevertFixes()
    {
        if(Directory.Exists(modPathField.text))
        {
            bool success = true;
            string[] filesBackup = FindIniFiles.FindIniFilesFixBackupRecursive(modPathField.text);
            foreach (string backupFile in filesBackup)
            {
                string originalFile = Path.ChangeExtension(backupFile, ".ini");
                if (File.Exists(backupFile))
                {
                    try
                    {
                        File.Copy(backupFile, originalFile, true);
                        File.Delete(backupFile);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Failed to revert fix: {e}");
                        success = false;
                    }
                }
            }
            buttonApplyPermanent.SetActive(false);
            buttonRevertFix.SetActive(false);
            buttonConfirmApply.SetActive(false);
            buttonConfirmRevert.SetActive(false);
            buttonInfoRevertAndApplyPermanent.gameObject.SetActive(true);
            buttonInfoRevertAndApplyPermanent.onClick.RemoveAllListeners();
            buttonInfoRevertAndApplyPermanent.onClick.AddListener(() => { KeyPressSimulator.SimulateKey(virtualKeys: VirtualKeyCode.F10);});
            if(success)
            {
                textInfoRevertAndApplyPermanent.text = "Revert success, please <u>reload</u>!";
            }
            else
            {
                textInfoRevertAndApplyPermanent.text = "Some files failed, try to close your file explorer";
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(modPathField.gameObject);
            modPathTitleText.text = "<color=#3BBBFF>Mod path empty or doesn't exist.</color>";
        }
    }
    #endregion
    #region ApplyPermanent
    public void ButtonApplyFixesPermanently()
    {
        if(Directory.Exists(modPathField.text))
        {
            bool success = true;
            string[] filesBackup = FindIniFiles.FindIniFilesFixBackupRecursive(modPathField.text);
            foreach (string backupFile in filesBackup)
            {
                try
                {
                    File.Delete(backupFile);
                }
                catch
                {
                    success = false;
                }
            }
            buttonApplyPermanent.SetActive(false);
            buttonRevertFix.SetActive(false);
            buttonConfirmApply.SetActive(false);
            buttonConfirmRevert.SetActive(false);
            buttonInfoRevertAndApplyPermanent.gameObject.SetActive(true);
            buttonInfoRevertAndApplyPermanent.onClick.RemoveAllListeners();
            if(success)
            {
                textInfoRevertAndApplyPermanent.text = "Success to apply permanent fix, backup deleted.";
            }
            else
            {
                textInfoRevertAndApplyPermanent.text = "Some files failed, try to close your file explorer";
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(modPathField.gameObject);
            modPathTitleText.text = "<color=#3BBBFF>Mod path empty or doesn't exist.</color>";
        }
    }
    #endregion
    
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
    private void OnModPathDeselect(string arg0)
    {
        modPathTitleText.text = "Mod path";
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
        KeyPressSimulator.SimulateKey(virtualKeys: VirtualKeyCode.F10);
    }
    #endregion

    #region Retrive fixes data
    private async void GetFixesList() //FIRST check fixes list from local or cloud
    {
        //Back/close button
        expandedGames[(int)selectedGame].GetComponentInChildren<Button>().interactable = false;

        string cachedDir = Path.Join(Application.persistentDataPath, ConstantVar.Path_Cached_Fixes, selectedGame.ToString());

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
            await DownloadJsonFixesListFile(ConstantVar.Link_ListModFixes[(int)selectedGame]);
            textModFixLoadingInfo.text = textInfoAfterLoading;
            InstantiateModFixPrefabs();
        }
        //Back/close button
        expandedGames[(int)selectedGame].GetComponentInChildren<Button>().interactable = true;
    }

    private async Task DownloadJsonFixesListFile(string url) //SECOND, if local cache not found, check & download fixes list on cloud
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("User-Agent", "UnityWebRequest");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            textInfoAfterLoading = $"<color=red>{request.error}</color>";
        }
        else
        {
            string modFixListJson = request.downloadHandler.text;
            ModFixList modFixList = JsonUtility.FromJson<ModFixList>(modFixListJson);

            foreach (string modFixLink in modFixList.fixesLink)
            {
                await DownloadModFixJsonFileAsync(modFixLink);
            }
        }
    }

    private async Task DownloadModFixJsonFileAsync(string modFixLink)
    {
        UnityWebRequest request = UnityWebRequest.Get(modFixLink);
        request.SetRequestHeader("User-Agent", "UnityWebRequest");

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading JSON file: " + request.error);
        }
        else
        {
            string jsonContent = request.downloadHandler.text;
            string cachedDir = Path.Join(Application.persistentDataPath, ConstantVar.Path_Cached_Fixes, selectedGame.ToString());

            if (!Directory.Exists(cachedDir))
            {
                Directory.CreateDirectory(cachedDir);
            }

            string filePath = Path.Join(cachedDir, GetFileNameFromUrl(modFixLink));
            await WriteFileAsync(filePath, jsonContent);
            
            await LoadModFixAsync(filePath);
        }
    }

    private string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);

        string fileName = Path.GetFileName(uri.LocalPath);

        return fileName;
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
        if (file.EndsWith(ConstantVar.File_Fixes_Log))
        {
            textInfoAfterLoading = jsonData;
        }
        else
        {
            try
            {
                // Deserialize JSON into the wrapper class
                ModFixDataWrapper wrapper = JsonUtility.FromJson<ModFixDataWrapper>(jsonData);

                // Convert the wrapper to ModFixData
                ModFixData fixData = new ModFixData
                {
                    title = wrapper.title,
                    note = wrapper.note,
                    modFixType = wrapper.modFixType,
                    modFixGame = wrapper.modFixGame,
                    hashpair = wrapper.hashpair.ToDictionary(pair => pair.key, pair => pair.value), 
                    modifiedDate = DateTime.Parse(wrapper.modifiedDate)
                };

                // Add to list
                modFixDatas.Add(fixData);
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

    #region Delete mod fix cache
    private void DeleteModFixCache()
    {
        if(Directory.Exists(Path.Join(Application.persistentDataPath, ConstantVar.Path_Cached_Fixes)))
        {
            Directory.Delete(Path.Join(Application.persistentDataPath, ConstantVar.Path_Cached_Fixes), recursive: true);
        }
    }
    #endregion
}