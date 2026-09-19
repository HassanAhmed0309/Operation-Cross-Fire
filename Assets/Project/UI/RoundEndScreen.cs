using TMPro;
using UnityEngine;

// Shows a win/lose message when the round ends. No logic - spawning/input already stop themselves
// by reacting to the same RoundEndedSignal elsewhere.
public class RoundEndScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text messageText;

    void OnEnable()
    {
        EventBus.Subscribe<RoundEndedSignal>(OnRoundEnded);
        panel.SetActive(false);
    }

    void OnDisable() => EventBus.Unsubscribe<RoundEndedSignal>(OnRoundEnded);

    void OnRoundEnded(RoundEndedSignal signal)
    {
        messageText.text = signal.Won ? "MISSION COMPLETE" : "MISSION FAILED";
        panel.SetActive(true);
    }
}
