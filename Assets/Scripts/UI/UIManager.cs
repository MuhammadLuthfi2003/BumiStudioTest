using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject UpgradePanel;
    [SerializeField] GameObject CurrencyPanel;
    [SerializeField] GameObject ResourcePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideGameplayPanel();
    }

    public void ShowGameplayPanel()
    {
        ResourcePanel.SetActive(true);
    }

    public void HideGameplayPanel()
    {
        ResourcePanel.SetActive(false);
    }

    public void ShowLobbyPanel()
    {
        CurrencyPanel.SetActive(true);
        UpgradePanel.SetActive(true);
    }

    public void HideLobbyPanel()
    {
        CurrencyPanel.SetActive(false);
        UpgradePanel.SetActive(false);
    }
}
