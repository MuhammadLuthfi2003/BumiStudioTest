using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [Header("UI Refrences")]
    [SerializeField] private TextMeshProUGUI moneyDisplay;

    private CurrencyManager currencyManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currencyManager = GameManager.Instance != null ? GameManager.Instance.CurrencyManager : null;

        if (currencyManager == null) return;

        currencyManager.OnMoneyChanged += UpdateMoney;

        if (moneyDisplay != null)
        {
            moneyDisplay.text = "Current Money : $" + currencyManager.CurrentMoney.ToString(); 
        }
    }

    private void OnDisable()
    {
        if (currencyManager != null)
        {
            currencyManager.OnMoneyChanged -= UpdateMoney;
        }
    }

    // Update is called once per frame
    void UpdateMoney(int newValue)
    {
        moneyDisplay.text = "Current Money : $" + newValue.ToString();
    }
}
