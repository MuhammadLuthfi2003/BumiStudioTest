using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Owns the single save file on disk and the in-memory SaveData that everything else
/// reads from / writes to. Singleton, same pattern as GameManager/CurrencyManager.
///
/// Loads automatically in Awake(), which Unity guarantees runs (for every object
/// already in the scene) before any Start() call fires that same frame — so
/// CurrencyManager/UpgradeManager can safely read SaveManager.Instance.Data from
/// their own Start() without worrying about script execution order.
///
/// Other systems don't write the file directly; they mutate SaveManager.Instance.Data
/// and call Save(). This keeps "what persists" in one place instead of scattered
/// PlayerPrefs calls, and means adding a new persisted field later is a one-file change.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    /// <summary>Current save data, in memory. Read/write this directly, then call Save().</summary>
    public SaveData Data { get; private set; } = new SaveData();

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        // Enforce the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Load();
    }

    /// <summary>Reads save.json from disk into Data. Falls back to a fresh SaveData if
    /// no file exists yet (first launch) or the file is corrupt/unreadable.</summary>
    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            else
            {
                Data = new SaveData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: failed to load '{SavePath}', starting with a fresh save. {e}");
            Data = new SaveData();
        }
    }

    /// <summary>Writes the current in-memory Data to disk.</summary>
    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: failed to save to '{SavePath}'. {e}");
        }
    }

    /// <summary>Wipes progress and immediately persists the blank state. Handy for a
    /// "New Game" / debug reset button.</summary>
    public void ResetSave()
    {
        Data = new SaveData();
        Save();
    }

    // Safety nets in case something changes Data without calling Save() directly
    // (e.g. mobile platforms killing the app instead of cleanly quitting).
    private void OnApplicationQuit() => Save();

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            Save();
    }
}