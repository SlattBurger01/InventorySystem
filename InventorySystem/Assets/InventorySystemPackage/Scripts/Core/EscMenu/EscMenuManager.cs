using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PageContent;
using System;

namespace InventorySystem.EscMenu
{
    public class EscMenuManager : MonoBehaviour
    {
        [HideInNormalInspector] public bool opened;

        [SerializeField] private SaveAndLoadMenu saveAndLoadMenu;
        [SerializeField] private GameObject escMenu;

        [SerializeField] private KeyCode openKey = KeyCode.Escape;

        public Action<bool> onMenuOpened = delegate { }; // opened

        private void Awake() => OpenMenu(false);

        private void Update()
        {
            if (Input.GetKeyDown(openKey)) OpenMenu(!escMenu.activeSelf);
        }

        public void OpenMenu(bool open)
        {
            escMenu.SetActive(open);
            opened = open;

            onMenuOpened.Invoke(open);

            GoBack();
        }

        public void GoBack() => CloseAllMenus();

        public void OpenSaveAndLoadMenu() => saveAndLoadMenu.Open(true); // USED IN BUTTON

        private void CloseAllMenus()
        {
            saveAndLoadMenu.Open(false);
        }
    }
}
