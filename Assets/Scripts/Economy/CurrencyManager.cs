using System;
using UnityEngine;

/// <summary>
/// Holds the player's persistent money, carried across dives (unlike ResourceManager's
/// cargo/oxygen, which reset every dive). Subscribes to ResourceManager.OnDiveSuccess to
/// add earned money automatically on a successful return.
///
/// Money is persisted via SaveManager: loaded on Start, written back on every change.
///
/// Singleton, same pattern as GameManager, since only one wallet should exist per save.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    [Header("Starting Balance")]
    [Tooltip("Money the player has at the very start of a brand new save (no save file found yet).")]
    [Min(0)]
    [SerializeField] private int startingMoney = 0;

    private ResourceManager resourceManager;

    /// <summary>Current money the player has available to spend on upgrades.</summary>
    public int CurrentMoney { get; private set; }

    /// <summary>Fired whenever money changes. Param: new current money.</summary>
    public event Action<int> OnMoneyChanged;

    public static CurrencyManager Instance { get; private set; }

    private void Awake()
    {
        // Enforce the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Load persisted money if a save exists; otherwise fall back to the configured
        // starting balance (first-ever launch). SaveManager's own Awake() has already
        // run and loaded the file by the time any Start() executes.
        CurrentMoney = SaveManager.Instance != null
            ? SaveManager.Instance.Data.currentMoney
            : startingMoney;

        OnMoneyChanged?.Invoke(CurrentMoney);

        resourceManager = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;

        if (resourceManager != null)
        {
            resourceManager.OnDiveSuccess += HandleDiveSuccess;
        }
    }

    private void OnDisable()
    {
        if (resourceManager != null)
            resourceManager.OnDiveSuccess -= HandleDiveSuccess;
    }

    private void HandleDiveSuccess(int moneyEarned)
    {
        Add(moneyEarned);
    }

    /// <summary>Adds money (e.g. from a sold dive's catch). Amount should be >= 0.</summary>
    public void Add(int amount)
    {
        if (amount <= 0) return;

        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        PersistMoney();
    }

    /// <summary>
    /// Attempts to spend money. Returns false (and spends nothing) if the player
    /// can't afford it.
    /// </summary>
    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > CurrentMoney)
            return false;

        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        PersistMoney();
        return true;
    }

    /// <summary>Whether the player currently has enough money for the given cost.</summary>
    public bool CanAfford(int amount) => amount >= 0 && amount <= CurrentMoney;

    /// <summary>Pushes CurrentMoney into the save file. Called after every change so a
    /// crash/force-quit never loses more than the most recent transaction.</summary>
    private void PersistMoney()
    {
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.Data.currentMoney = CurrentMoney;
        SaveManager.Instance.Save();
    }
}