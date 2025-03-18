using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ModFixLogToUI : MonoBehaviour
{
    public TextMeshProUGUI logText;
    [SerializeField] private Scrollbar scrollbar;

    private string[] colors = new string[3]{
        "#ffffff", // White
        "#ffdd33", // Yellow
        "#ff6666"  // Red
    };

    private void ScrollDown(){
        scrollbar.value = 1f;
    }

    //Called from TabModFixManager, making sure it's always ready
    public void LogCallback(string message, string stackTrace, LogType type)
    {
        if(!message.StartsWith("UI--")) return;
        message = message.Replace("UI--","");
        //logTypeIndex => normal:0 , warning:1 , error:2
        int logTypeIndex = (type==LogType.Log)?0:(type==LogType.Warning)?1:2;
        logText.text += $"<sprite={logTypeIndex}><color={colors[logTypeIndex]}> {message}</color>\n\n";
        ScrollDown();
    }
}
