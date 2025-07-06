using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogPanel : MonoBehaviour
{
    public TMP_Text logText;
    public ScrollRect scrollRect;
    private const int MAX_LINES = 50;
    private Queue<string> logLines = new Queue<string>();
    public void AddLog(string message)
    {
        logLines.Enqueue(message);

        // 保持最大行数
        if (logLines.Count > MAX_LINES)
        {
            logLines.Dequeue();
        }

        // 重建日志文本
        RebuildLogText();
    }
    private void RebuildLogText()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (string line in logLines)
        {
            sb.AppendLine(line);
        }
        logText.text = sb.ToString();

        // 自动滚动
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}