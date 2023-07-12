using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class LoadingScreensHandler : MonoBehaviour
    {
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
    }
}
