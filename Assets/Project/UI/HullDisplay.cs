using TMPro;
using UnityEngine;

// Displays current/max hull. Reads the live value once, then only updates on HullChangedSignal.
public class HullDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text hullText;

    void Start()
    {
        IHullService hullService = ServiceLocator.Get<IHullService>();
        SetText(hullService.CurrentHull, hullService.MaxHull);
    }

    void OnEnable() => EventBus.Subscribe<HullChangedSignal>(OnHullChanged);

    void OnDisable() => EventBus.Unsubscribe<HullChangedSignal>(OnHullChanged);

    void OnHullChanged(HullChangedSignal signal) => SetText(signal.CurrentHull, signal.MaxHull);

    void SetText(int current, int max) => hullText.SetText("HULL {0:0}/{1:0}", current, max);
}
