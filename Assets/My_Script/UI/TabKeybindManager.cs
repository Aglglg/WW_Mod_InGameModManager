using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabKeybindManager : MonoBehaviour
{
    private const int KeyNameChildIndex = 0;
    private const int KeyBindChildIndex = 1;

    //Called from ContextMenuModManager
    [HideInInspector] public string modFolderPath;
    [HideInInspector] public string modName;
    [HideInInspector] public string modGroup;

    [SerializeField] private GameObject prefabKeybind;
    [SerializeField] private Button buttonEdit;
    [SerializeField] private Button buttonSave;
    [SerializeField] private TextMeshProUGUI modNameTitle;
    [SerializeField] private Transform contentTransform;

    private List<string[]> iniFileAndLines = new();
    private string[] iniFiles;
    private int totalKey = 0;

    private void OnEnable()
    {
        if(string.IsNullOrEmpty(modFolderPath))
        {
            modNameTitle.text = "";
            buttonSave.gameObject.SetActive(false);
            buttonEdit.gameObject.SetActive(false);
            return;
        }

        UpdateKeybindData(modFolderPath);
        modNameTitle.text = totalKey > 0 ? $"{modGroup} - {modName}" : $"No keybinds on {modGroup} - {modName}";
        buttonEdit.gameObject.SetActive(true);
        buttonSave.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ResetPreviousKeybindData();
    }

    private void UpdateKeybindData(string modFolder)
    {
        GetIniFiles(modFolder);
    }

    private void ResetPreviousKeybindData()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        iniFileAndLines.Clear();
        iniFiles = null;
        totalKey = 0;
    }

    private void GetIniFiles(string modPath)
    {
        iniFiles = FindIniFiles.FindIniFilesRecursive(modPath);

        for (int i = 0; i < iniFiles.Length; i++)
        {
            ReadIniFile(iniFiles[i], i);
        }
    }

    private void ReadIniFile(string iniFilePath, int indexIniFile)
    {
        string[] lines = File.ReadAllLines(iniFilePath);
        iniFileAndLines.Add(lines);

        GameObject instantiatedKeybindItem = null;
        for (int i = 0; i < lines.Length; i++)
        {
            if(lines[i].ToLower().Trim().StartsWith("[key"))
            {
                totalKey++;
                instantiatedKeybindItem = Instantiate(prefabKeybind, contentTransform);
                instantiatedKeybindItem.transform.GetChild(KeyNameChildIndex).GetComponent<TextMeshProUGUI>().text = lines[i].Trim();
            }
            else if(lines[i].ToLower().Trim().StartsWith("key"))
            {
                if(instantiatedKeybindItem != null)
                {
                    string[] parts = lines[i].Split('=');
                    if(parts.Length == 2)
                    {
                        TMP_InputField inputField = instantiatedKeybindItem.transform.GetChild(KeyBindChildIndex).GetComponent<TMP_InputField>();
                        inputField.text = lines[i].Split('=')[1].Trim();
                        AddListenerToInputField(inputField, indexIniFile, i);
                    }
                }
            }
        }
    }

    private void AddListenerToInputField(TMP_InputField inputField, int fileIndex, int lineIndex)
    {
        inputField.onValueChanged.AddListener(value => OnInputFieldValueChanged(fileIndex, lineIndex, value));
    }

    private void OnInputFieldValueChanged(int fileIndex, int lineIndex, string value)
    {
        iniFileAndLines[fileIndex][lineIndex] = $"key = {value}";
    }




    public void SaveKeybindButton()
    {
        for (int i = 0; i < iniFiles.Length; i++)
        {
            File.WriteAllLines(iniFiles[i], iniFileAndLines[i]);
        }

        KeyPressSimulator.SimulateKey(WindowsInput.VirtualKeyCode.F10);
    }

    public void ToggleEditMode(bool editMode)
    {
        TMP_InputField[] inputFields = contentTransform.GetComponentsInChildren<TMP_InputField>(true);
        CanvasGroup[] canvasGroups = contentTransform.GetComponentsInChildren<CanvasGroup>(true);

        if(editMode)
        {
            //Enter edit mode
            buttonSave.gameObject.SetActive(true);
            buttonEdit.gameObject.SetActive(false);
            
            foreach (TMP_InputField inputField in inputFields)
            {
                inputField.interactable = true;
            }

            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            //Exit edit mode
            buttonSave.gameObject.SetActive(false);
            buttonEdit.gameObject.SetActive(true);

            foreach (TMP_InputField inputField in inputFields)
            {
                inputField.interactable = false;
            }

            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}