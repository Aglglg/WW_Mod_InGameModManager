using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ModManagerUtils
{
    [DllImport("dds_converter_from_mod_manager", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool convert_to_dds(string input_path, string output_path);

    private const int WidthModIcon = 216;
    private const int HeightModIcon = 312;
    private const int WidthHeightGroupIcon = 160;
    
    public static void SaveManagedModData()
    {
        string pathJson = Path.Combine(
            PlayerPrefs.GetString(ConstantVar.PREFIX_PLAYERPERFKEY_MODPATH + Initialization.gameName),
            ConstantVar.MANAGED_PATH,
            ConstantVar.MODDATA_JSON_FILE
        );
        string json = JsonUtility.ToJson(TabModManager.modData, true);
        File.WriteAllText(pathJson, json);
    }

    public static void CreateIcon(string inputPath, string outputPath, bool isGroupIcon)
    {
        Texture2D originalImage = LoadImage(inputPath);
        
        int targetWidth = isGroupIcon ? WidthHeightGroupIcon : WidthModIcon;
        int targetHeight = isGroupIcon ? WidthHeightGroupIcon : HeightModIcon;
        float ratio = isGroupIcon ? 1/1 : 2/3;

        Texture2D modifiedImage = CropAndResize(originalImage, ratio, targetWidth, targetHeight);
        SaveResizedImage(modifiedImage, outputPath);

        string inputPathDds = outputPath;
        string outputPathDds = Path.ChangeExtension(outputPath, ".dds");

        bool success = convert_to_dds(inputPathDds, outputPathDds);

        if (success)
        {
            Debug.Log("DDS conversion successful!");
        }
        else
        {
            Debug.LogError("DDS conversion failed!");
        }
    }


    #region ImageProcessing
    private static Texture2D CropAndResize(Texture2D originalTexture, float targetAspectRatio, int targetWidth, int targetHeight)
    {
        // Step 1: Crop to aspect ratio
        Texture2D croppedTexture = CropToAspectRatio(originalTexture, targetAspectRatio);

        // Step 2: Resize to target resolution
        return ResizeTexture(croppedTexture, targetWidth, targetHeight);
    }

    private static Texture2D CropToAspectRatio(Texture2D texture, float aspectRatio)
    {
        int originalWidth = texture.width;
        int originalHeight = texture.height;

        float currentAspectRatio = (float)originalWidth / originalHeight;

        int cropWidth = originalWidth;
        int cropHeight = originalHeight;

        // Crop to the target aspect ratio
        if (currentAspectRatio > aspectRatio)
        {
            // Crop horizontally
            cropWidth = Mathf.RoundToInt(originalHeight * aspectRatio);
        }
        else
        {
            // Crop vertically
            cropHeight = Mathf.RoundToInt(originalWidth / aspectRatio);
        }

        int startX = (originalWidth - cropWidth) / 2;
        int startY = (originalHeight - cropHeight) / 2;

        Color[] pixels = texture.GetPixels(startX, startY, cropWidth, cropHeight);

        Texture2D croppedTexture = new Texture2D(cropWidth, cropHeight);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    private static Texture2D ResizeTexture(Texture2D texture, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture.active = rt;

        Graphics.Blit(texture, rt);

        Texture2D resizedTexture = new Texture2D(width, height);
        resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resizedTexture.Apply();

        RenderTexture.active = null;
        rt.Release();

        return resizedTexture;
    }

    private static Texture2D LoadImage(string filePath)
    {
        byte[] imageData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    private static void SaveResizedImage(Texture2D texture, string outputPath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
    }
    #endregion
}
