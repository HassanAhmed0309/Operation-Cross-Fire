using TMPro;
using UnityEngine;

// Shows the pre-transition warning and the "roles reversed" banner, auto-hiding after a short delay.
public class QuantumFluxBanner : MonoBehaviour
{
    [SerializeField] TMP_Text bannerText;
    [SerializeField] float warningBannerDuration = 3f; // should match RoundData.warningLeadTime
    [SerializeField] float triggeredBannerDuration = 2f;

    float hideTimeRemaining;

    void OnEnable()
    {
        EventBus.Subscribe<QuantumFluxWarningSignal>(OnWarning);
        EventBus.Subscribe<QuantumFluxTriggeredSignal>(OnTriggered);
        bannerText.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<QuantumFluxWarningSignal>(OnWarning);
        EventBus.Unsubscribe<QuantumFluxTriggeredSignal>(OnTriggered);
    }

    void Update()
    {
        if (hideTimeRemaining <= 0f)
            return;

        hideTimeRemaining -= Time.deltaTime;
        if (hideTimeRemaining <= 0f)
            bannerText.gameObject.SetActive(false);
    }

    void OnWarning(QuantumFluxWarningSignal signal) => Show("QUANTUM FLUX IN 3... 2... 1...", warningBannerDuration);

    void OnTriggered(QuantumFluxTriggeredSignal signal) => Show("QUANTUM FLUX - ROLES REVERSED", triggeredBannerDuration);

    void Show(string message, float duration)
    {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        hideTimeRemaining = duration;
    }
}
