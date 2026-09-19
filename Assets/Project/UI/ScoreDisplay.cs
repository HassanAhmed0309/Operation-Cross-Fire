using TMPro;
using UnityEngine;

// Displays total score. Reads the live value once, then only updates on ScoreChangedSignal.
public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;

    void Start()
    {
        IScoreService scoreService = ServiceLocator.Get<IScoreService>();
        SetText(scoreService.TotalScore);
    }

    void OnEnable() => EventBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);

    void OnDisable() => EventBus.Unsubscribe<ScoreChangedSignal>(OnScoreChanged);

    void OnScoreChanged(ScoreChangedSignal signal) => SetText(signal.TotalScore);

    void SetText(int score) => scoreText.SetText("SCORE {0:0}", score);
}
