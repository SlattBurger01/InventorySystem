using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class LoadingScreensHandler : MonoBehaviour
    {
        [Header("Build-in screens")]
        [SerializeField] [UnnecessaryProperty] private GameObject inializeGameScreen;
        [SerializeField] [UnnecessaryProperty] private GameObject loadGameScreen;
        [SerializeField] [UnnecessaryProperty] private GameObject syncGameScreen;

        [Header("Custom screens")]
        [SerializeField] private GameObject[] loadingScreens;

        private void Awake()
        {
            for (int i = 0; i < loadingScreens.Length; i++) { EnableLoadingScreen(i, false); }
        }

        public void EnableLoadingScreen(int loadingScreen, bool enable) 
        {
            loadingScreens[loadingScreen].SetActive(enable); 
            Console.Add($"Set active screen: {loadingScreen}, {enable}", FindObjectOfType<Console>()); 
        }

        private void EnableLoadingScreenF(GameObject obj, bool enable)
        {
            if (obj) obj.SetActive(enable);
        }

        public void EnableInializeGameLoadingScreen(bool enable) => EnableLoadingScreenF(inializeGameScreen, enable);
        public void EnableLoadGameLoadingScreen(bool enable) => EnableLoadingScreenF(loadGameScreen, enable);
        public void EnableSyncGameLoadingScreen(bool enable) => EnableLoadingScreenF(syncGameScreen, enable);
    }
}
