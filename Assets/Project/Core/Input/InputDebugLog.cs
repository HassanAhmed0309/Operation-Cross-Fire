using System.Text;
using TMPro;
using UnityEngine;

// On-screen input log for testing the setup on a device. Debug only; not part of the gameplay path.
public class InputDebugLog : MonoBehaviour
{
    [SerializeField] TMP_Text logText;     // scrolling list of input events
    [SerializeField] TMP_Text stateText;   // optional live readout of the current intents
    [SerializeField] int maxLines = 14;
    [SerializeField] bool logEnabled = true;

    static InputDebugLog instance;

    string[] lines;
    int lineCount;
    int nextLine;                          // ring buffer write position
    readonly StringBuilder builder = new StringBuilder(512);

    // Call sites check this before building a message, so logging costs nothing when it is off.
    public static bool Enabled => instance != null && instance.logEnabled;

    void Awake()
    {
        if (instance == null)
            instance = this;

        lines = new string[Mathf.Max(1, maxLines)];
        Clear();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Write(string message)
    {
        if (instance != null && instance.logEnabled)
            instance.Append(message);
    }

    // Allocation-free live readout: TMP formats the values straight into its own buffer.
    public static void ShowState(in PilotIntent pilot, in GunnerIntent gunner)
    {
        if (instance == null || !instance.logEnabled || instance.stateText == null)
            return;

        instance.stateText.SetText(
            "MOVE {0:2}   BOOST {1}   AIM {2:0},{3:0}   FIRE {4}   SHIELD {5}",
            pilot.Move,
            pilot.BoostPressed ? 1f : 0f,
            gunner.AimDelta.x,
            gunner.AimDelta.y,
            gunner.FireHeld ? 1f : 0f,
            gunner.ShieldPressed ? 1f : 0f);
    }

    public void Clear()
    {
        lineCount = 0;
        nextLine = 0;
        if (logText != null)
            logText.text = string.Empty;
    }

    void Append(string message)
    {
        lines[nextLine] = Time.time.ToString("0.00") + "  " + message;
        nextLine = (nextLine + 1) % lines.Length;
        if (lineCount < lines.Length)
            lineCount++;

        if (logText == null)
            return;

        builder.Clear();
        int first = (nextLine - lineCount + lines.Length) % lines.Length;
        for (int i = 0; i < lineCount; i++)
        {
            if (i > 0)
                builder.Append('\n');
            builder.Append(lines[(first + i) % lines.Length]);
        }
        logText.SetText(builder);
    }
}
