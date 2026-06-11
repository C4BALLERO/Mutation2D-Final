using System;
using System.IO;
using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Guardado local: opciones en PlayerPrefs, progreso en JSON.
    /// </summary>
    public class Script_05_SaveManager : MonoBehaviour
    {
        public static Script_05_SaveManager Instance { get; private set; }

        private const string SaveFileName = "mutationswarm_save.json";
        private const string PrefsPrefix = "MS_";

        [Serializable]
        public class SaveData
        {
            public int highScore;
            public int maxWaveReached;
            public string[] unlockedUpgrades;
            public string metaEcosystemJson;
        }

        public SaveData CurrentSave { get; private set; } = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            transform.SetParent(null); // Must be root for DontDestroyOnLoad
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            string path = GetSavePath();
            if (!File.Exists(path))
            {
                CurrentSave = new SaveData();
                return;
            }

            string json = File.ReadAllText(path);
            CurrentSave = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(CurrentSave, true);
            File.WriteAllText(GetSavePath(), json);
        }

        public void SetFloat(string key, float value) =>
            PlayerPrefs.SetFloat(PrefsPrefix + key, value);

        public float GetFloat(string key, float defaultValue = 0f) =>
            PlayerPrefs.GetFloat(PrefsPrefix + key, defaultValue);

        public void SetInt(string key, int value) =>
            PlayerPrefs.SetInt(PrefsPrefix + key, value);

        public int GetInt(string key, int defaultValue = 0) =>
            PlayerPrefs.GetInt(PrefsPrefix + key, defaultValue);

        private static string GetSavePath() =>
            Path.Combine(Application.persistentDataPath, SaveFileName);
    }
}
