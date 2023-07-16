using UnityEngine;
using System;
using InventorySystem.PageContent;
using InventorySystem.EscMenu;

namespace InventorySystem
{
    public class InventoryCore : MonoBehaviour
    {
        private bool EscMenuOpened { get { EscMenuManager e = FindObjectOfType<EscMenuManager>(); return e ? e.opened : false; } }
        private bool InvMenuOpened { get { InventoryMenu i = GetComponent<InventoryMenu>(); return i ? i.opened : false; } }

        public bool AnyMenuIsOpened => InvMenuOpened || EscMenuOpened;

        [SerializeField] private Camera playersCamera; // USED ONLY FOR RAYCAST

        [HideInNormalInspector] public bool isMine;

        // MULTIPLAYER
        public Action GetOwnershipStatus = delegate { };

        // DEBUG
        [SerializeField] [HideInSinglePlayerInspector] private bool forceNotMine = false;
        [HideInInspector] public InventoryEventSystem inventoryEventSystem;

        private void Awake() { inventoryEventSystem = gameObject.AddComponent<InventoryEventSystem>();}

        private void Start()
        {
            isMine = true;

            GetOwnershipStatus.Invoke();

            if (forceNotMine) isMine = false;

            if (!isMine) DisableLocalComponents();
        }

        private readonly Type[] localComponents = new Type[] { typeof(Camera), typeof(Canvas) };

        /// <summary> DISABLES COMPONENTS OF PREDIFINED TYPE: 'localComponents' (MULTIPLAYER ONLY) </summary>
        private void DisableLocalComponents()
        {
            for (int i = 0; i < localComponents.Length; i++)
            {
                GetComponentInChildren(localComponents[i]).gameObject.SetActive(false);
            }
        }

        public void SetNickname(string nickname) { onNicknameSet.Invoke(nickname); }

        public Action<string> onNicknameSet = delegate { };

        // RAYCAST

        /// <returns> IF RAYCAST WAS SUCESFULL </returns>
        public bool RaycastFromCameraMid(float range, out RaycastHit hit) => RaycastFromCamera(range, new Vector2(Screen.width / 2, Screen.height / 2), out hit);

        /// <returns> IF RAYCAST WAS SUCESFULL </returns>
        public bool RaycastFromCurrentMousePos(float range, out RaycastHit hit) => RaycastFromCamera(range, Input.mousePosition, out hit);

        /// <returns> IF RAYCAST WAS SUCESFULL </returns>
        private bool RaycastFromCamera(float range, Vector2 position, out RaycastHit hit)
        {
            Ray ray = playersCamera.ScreenPointToRay(position);
            return Physics.Raycast(ray, out hit, range);
        }
        // --- ---

        /// <returns> WORLD TO SCREEN POINT BASED ON PLAYERS CAMERA </returns>
        public Vector3 WorldToScreenPoint(Vector3 position) => playersCamera.WorldToScreenPoint(position);

        /// <returns> WORLD TO VIEWPORT BASED ON PLAYERS CAMERA </returns>
        public Vector3 WorldToViewportPoint(Vector3 position) => playersCamera.WorldToViewportPoint(position);

        /// <summary> CURRENT POSITION OF PLAYERS CAMERA </summary>
        public Vector3 CameraPosition => playersCamera.transform.position;
    }
}
