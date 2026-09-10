using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Oxygen")]
    [Tooltip("Maximum oxygen for the upcoming dive. Set this from the upgrade screen before StartDive().")]
    [Min(0f)]
    [SerializeField] private float maxOxygen = 100f;

    [Tooltip("Units of oxygen consumed per second while diving.")]
    [Min(0f)]
    [SerializeField] private float oxygenDepletionRate = 1f;

    [Header("Cargo")]
    [Tooltip("Total cargo slots available for the upcoming dive. Set this from the upgrade screen before StartDive().")]
    [Min(0)]
    [SerializeField] private int cargoCapacity = 5;

    /// <summary>Current oxygen remaining. Read-only from outside; drains automatically while diving.</summary>
    public float CurrentOxygen { get; private set; }

    /// <summary>True while a dive is in progress (between StartDive and ReturnToSurface/failure).</summary>
    public bool IsDiving { get; private set; }

    /// <summary>Fish currently stored in cargo for this dive.</summary>
    public IReadOnlyList<FishData> CaughtFish => _caughtFish;
    private readonly List<FishData> _caughtFish = new List<FishData>();

    /// <summary>Cargo slots currently occupied by caught fish.</summary>
    public int UsedCargoSlots => _caughtFish.Sum(f => f.cargoSlots);

    // ---- Events ----

    /// <summary>Fired whenever oxygen changes. Params: currentOxygen, maxOxygen.</summary>
    public event Action<float, float> OnOxygenChanged;

    /// <summary>Fired once when oxygen reaches zero during a dive.</summary>
    public event Action OnOxygenDepleted;

    /// <summary>Fired whenever cargo contents change. Params: usedSlots, cargoCapacity.</summary>
    public event Action<int, int> OnCargoChanged;

    /// <summary>Fired when a dive ends successfully. Param: money earned from sold fish.</summary>
    public event Action<int> OnDiveSuccess;

    /// <summary>Fired when a dive ends in failure (oxygen depleted). All cargo is lost.</summary>
    public event Action OnDiveFailed;

    private void Update()
    {
        if (!IsDiving)
            return;

        if (CurrentOxygen <= 0f)
            return; // already handled by DepleteOxygenAndCheckFailure this frame

        DepleteOxygen(oxygenDepletionRate * Time.deltaTime);
    }

    // ---- Pre-dive configuration ----

    /// <summary>Call from the upgrade screen to set oxygen capacity before the next dive.</summary>
    public void SetMaxOxygen(float newMaxOxygen)
    {
        maxOxygen = Mathf.Max(0f, newMaxOxygen);
    }

    /// <summary>Call from the upgrade screen to set cargo capacity before the next dive.</summary>
    public void SetCargoCapacity(int newCargoCapacity)
    {
        cargoCapacity = Mathf.Max(0, newCargoCapacity);
    }

    // ---- Dive lifecycle ----

    /// <summary>Begins a new dive: fills oxygen to max and clears any leftover cargo.</summary>
    public void StartDive()
    {
        CurrentOxygen = maxOxygen;
        _caughtFish.Clear();
        IsDiving = true;

        OnOxygenChanged?.Invoke(CurrentOxygen, maxOxygen);
        OnCargoChanged?.Invoke(UsedCargoSlots, cargoCapacity);
    }

    /// <summary>
    /// Voluntarily ends the dive. Sells all caught fish and reports the money earned.
    /// Does nothing if not currently diving.
    /// </summary>
    public void ReturnToSurface()
    {
        if (!IsDiving)
            return;

        int moneyEarned = _caughtFish.Sum(f => f.sellValue);
        _caughtFish.Clear();
        IsDiving = false;

        OnCargoChanged?.Invoke(UsedCargoSlots, cargoCapacity);
        OnDiveSuccess?.Invoke(moneyEarned);
    }

    // ---- Fishing / cargo ----

    /// <summary>
    /// Attempts to add a caught fish to cargo.
    /// Returns false if cargo doesn't have room; the caller (UI) is responsible for
    /// prompting the player to discard an existing fish, then calling DiscardFish + TryCatchFish again.
    /// </summary>
    public bool TryCatchFish(FishData fish)
    {
        if (fish == null || !IsDiving)
            return false;

        if (UsedCargoSlots + fish.cargoSlots > cargoCapacity)
            return false;

        _caughtFish.Add(fish);
        OnCargoChanged?.Invoke(UsedCargoSlots, cargoCapacity);
        return true;
    }

    /// <summary>Removes a specific fish from cargo (e.g. player chose to discard it to make room).</summary>
    public bool DiscardFish(FishData fish)
    {
        bool removed = _caughtFish.Remove(fish);
        if (removed)
            OnCargoChanged?.Invoke(UsedCargoSlots, cargoCapacity);
        return removed;
    }

    // ---- Oxygen ----
    private void DepleteOxygen(float amount)
    {
        CurrentOxygen = Mathf.Max(0f, CurrentOxygen - amount);
        OnOxygenChanged?.Invoke(CurrentOxygen, maxOxygen);

        if (CurrentOxygen <= 0f)
            FailDive();
    }

    private void FailDive()
    {
        _caughtFish.Clear(); // all fish lost per GDD
        IsDiving = false;

        OnCargoChanged?.Invoke(UsedCargoSlots, cargoCapacity);
        OnOxygenDepleted?.Invoke();
        OnDiveFailed?.Invoke();
    }

}
