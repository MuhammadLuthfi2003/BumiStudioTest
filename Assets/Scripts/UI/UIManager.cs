using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] CanvasGroup UpgradePanel;
    [SerializeField] CanvasGroup CurrencyPanel;
    [SerializeField] CanvasGroup ResourcePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideGameplayPanel();
    }

    public void ShowGameplayPanel()
    {
        ResourcePanel.alpha = 1.0f;
        ResourcePanel.interactable = true;
    }

    public void HideGameplayPanel()
    {
        ResourcePanel.alpha = 0.0f;
        ResourcePanel.interactable = false;
    }

    public void ShowLobbyPanel()
    {
        CurrencyPanel.alpha = 1.0f;
        UpgradePanel.alpha = 1.0f;

        CurrencyPanel.interactable = true;
        UpgradePanel.interactable = true;
    }

    public void HideLobbyPanel()
    {
        CurrencyPanel.alpha = 0.0f;
        UpgradePanel.alpha = 0.0f;

        CurrencyPanel.interactable = false;
        UpgradePanel.interactable = false;
    }
}
