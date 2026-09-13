using System;
using UnityEngine;

/// <summary>
/// Holds the player's persistent money, carried across dives (unlike ResourceManager's
/// cargo/oxygen, which reset every dive). Subscribes to ResourceManager.OnDiveSuccess to
/// add earned money automatically on a successful return.
///
/// Singleton, same pattern as GameManager, since only one wallet should exist per save.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    [Header("Starting Balance")]
    [Tooltip("Money the player has at the very start of a new game.")]
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
        CurrentMoney = startingMoney;


    }

    private void Start()
    {
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
        return true;
    }

    /// <summary>Whether the player currently has enough money for the given cost.</summary>
    public bool CanAfford(int amount) => amount >= 0 && amount <= CurrentMoney;
}