using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaughtFishDisplay : MonoBehaviour
{
    [Header("Fish UI Display Prefab")]
    [Tooltip("Prefab with an Image component. One instance is spawned per fish in cargo.")]
    [SerializeField] private GameObject fishUIDisplay;

    private ResourceManager resourceManager;

    // keep track of spawned instances to make it easier to clean
    private readonly List<GameObject> _spawnedDisplays = new List<GameObject>();

    void Start()
    {
        resourceManager = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;

        if (resourceManager == null) return;

        resourceManager.OnCargoChanged += HandleCargoChanged;

        // populate immediately in case cargo already has fish (e.g. script enabled mid-dive)
        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (resourceManager != null)
        {
            resourceManager.OnCargoChanged -= HandleCargoChanged;
        }
    }

    private void HandleCargoChanged(int usedSlots, int cargoCapacity)
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        ClearDisplay();

        if (resourceManager == null || fishUIDisplay == null) return;

        foreach (FishData fish in resourceManager.CaughtFish)
        {
            SpawnFishIcon(fish);
        }
    }

    private void SpawnFishIcon(FishData fish)
    {
        GameObject instance = Instantiate(fishUIDisplay, transform);
        _spawnedDisplays.Add(instance);

        Image image = instance.GetComponent<Image>();
        if (image != null && fish != null)
        {
            image.sprite = fish.icon;
        }
    }

    private void ClearDisplay()
    {
        foreach (var display in _spawnedDisplays)
        {
            if (display != null)
                Destroy(display);
        }

        _spawnedDisplays.Clear();
    }
}