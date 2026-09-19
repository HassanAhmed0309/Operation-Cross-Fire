using TMPro;
using UnityEngine;

// Polls the round timer every frame - continuously changing, not event-driven.
public class RoundTimerDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;

    IRoundService roundService;

    void Start() => roundService = ServiceLocator.Get<IRoundService>();

    void Update()
    {
        float remaining = roundService.TimeRemaining;
        int minutes = (int)(remaining / 60f);
        int seconds = (int)(remaining % 60f);
        timerText.SetText("{0:00}:{1:00}", minutes, seconds);
    }
}
