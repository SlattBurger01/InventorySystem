using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace InventorySystem.SaveAndLoadSystem_
{
    public static class DiskDataManager
    {
        private static readonly string fileType = "savefile";
        private static readonly string totalGameDataFileType = "gamedata";

        private static readonly string totalFileName = "TotalGameSave";
        private static readonly string gameDataFileName = "totalGameData";

        /// <summary> Saves GameSave on local disk </summary>
        public static void SaveDataOntoDisk(string saveName, GameSave save, string currentGameName)
        {
            string path = Application.persistentDataPath + $"/{currentGameName}/{saveName}.{fileType}";
            SaveDataOntoDiskF(path, save);
        }

        /// <returns> Loaded GameSave from disk </returns>
        public static GameSave LoadSaveFromDisk(string saveName, string currentGameName)
        {
            string path = Application.persistentDataPath + $"/{currentGameName}/{saveName}.{fileType}";
            return LoadDataFromDiskF<GameSave>(path);
        }

        /// <summary> Creates and saves TotalGameSave on local disk </summary>
        public static void SaveTotalGameDataOntoDisk(List<string> savesNames, string currentGameName)
        {
            string path = Application.persistentDataPath + $"/{currentGameName}/{totalFileName}.{fileType}";
            TotalGameSave save = new TotalGameSave(savesNames.ToArray());
            SaveDataOntoDiskF(path, save);
        }

        public static TotalGameSave LoadTotalGameSaveDataFromDisk(string GameName)
        {
            string path = Application.persistentDataPath + $"/{GameName}/{totalFileName}.{fileType}";
            return LoadDataFromDiskF<TotalGameSave>(path);
        }

        // NOT USED IN GAME, BUT IN MENU ( ON CREATING NEW GAME )
        public static void SaveGameDataOntoDisk(List<string> gameDataNames)
        {
            string path = Application.persistentDataPath + $"/{gameDataFileName}.{totalGameDataFileType}";
            GameData save = new GameData(gameDataNames.ToArray());
            SaveDataOntoDiskF(path, save);
        }

        /// <returns> Gamedata on disk (if found, otherwise null) </returns>
        public static GameData LoadGameDataFromDisk()
        {
            string path = Application.persistentDataPath + $"/{gameDataFileName}.{totalGameDataFileType}";
            return LoadDataFromDiskF<GameData>(path);
        }

        /// <returns> Deserialized data on 'path' </returns>
        private static T LoadDataFromDiskF<T>(string path)
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                object data = formatter.Deserialize(stream);
                stream.Close();

                return (T)data;
            }

            Debug.LogWarning($"File was not found! {path}");
            return default(T);
        }

        private static void SaveDataOntoDiskF(string path, object data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);

            stream.Close();
        }
    }
}
