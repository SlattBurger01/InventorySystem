using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.SaveAndLoadSystem_;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InventorySystem
{
    public class InventoryGameManager : MonoBehaviour
    {
        // ROOM CREATING
        public static string playersName = "someName";
        public static bool createRoom = true;
        public static string roomName = "someRoomName";
        public static TotalGameSave gameSaveToLoad = null;
        // -----

        //[SerializeField] private ItemsDatabase itemsDatabase;

        [Tooltip("Object that will be enabled on localPlayer's dead")]
        [SerializeField] private GameObject playerRespawnMenu;

        [Header("PREFABS")]
        public GameObject playerPrefab;
        public static GameObject pPrefab;

        [Header("MULTIPLAYER SETTINGS")]
        public bool multiplayerMode_ = true;
        [SerializeField] private bool offlineMode;

        public static bool multiplayerMode;

        [HideInNormalInspector] public InventoryCore localPlayer;

        public static Action updateIsMasterclientBool = delegate { }; // REMOVE THIS WITH ACTION ! (CREATE VARIABLE IS MASTERCLIENT AND UPDATE IT BEFORE USE)
        public static bool m_isMasterClient = true;

        public static bool IsMasterClient { get { updateIsMasterclientBool.Invoke(); return m_isMasterClient; } }

        private void Awake()
        {
            pPrefab = playerPrefab;
        }

        private void Start()
        {
            if (playerRespawnMenu) playerRespawnMenu.SetActive(false);
            multiplayerMode = multiplayerMode_;

            if (!multiplayerMode)
            {
                print("INIALIZING SINGLE PLAYER GAME");

                InializeOfflineGame();
                return;
            }

            inializeMultiplayerGame.Invoke(offlineMode, createRoom, roomName);
        }

        public static Action<bool, bool, string> inializeMultiplayerGame = delegate { }; // bool offlineMode, bool createRoom, string roomName

        public void InializeOfflineGame()
        {
            PickupableItemsStacksHandler pStacksHandler = FindObjectOfType<PickupableItemsStacksHandler>();
            if (pStacksHandler) pStacksHandler.StartLoop();

            localPlayer = FindObjectOfType<InventoryCore>();
        }

        public static void TrySetUpSaveAndLoadSystemAndLoadRecentSave()
        {
            print($"SSED: {gameSaveToLoad}");

            if (gameSaveToLoad == null) return;
            if (gameSaveToLoad.savesName.Length == 0) return;

            SaveAndLoadSystem saveAndLoadSystem = FindObjectOfType<SaveAndLoadSystem>();

            saveAndLoadSystem.savesNames = gameSaveToLoad.savesName.ToList();

            if (gameSaveToLoad.savesName.Length > 0)
            {
                saveAndLoadSystem.LoadGameSaveFromDisk(gameSaveToLoad.savesName[^1], out GameSave usedSave);
                saveAndLoadSystem.recentSave = usedSave;
            }
        }

        public static Action<InventoryCore, string> setPlayerNickname = delegate { }; // player, name
        public static Action<GameObject> destroyObject = delegate { };

        public static void DestroyObjectForAll(GameObject obj)
        {
            if (multiplayerMode) destroyObject.Invoke(obj);
            else Destroy(obj);
        }

        public static Action<GameObject, Vector3> spawnGameObject = delegate { };
        public static GameObject spawnedObject;

        /// <summary> Spawns object (works in singleplayer as well as in multiplayer) </summary>
        /// <returns> Spawned object </returns>
        public static GameObject SpawnGameObjectForAll(GameObject prefab, Vector3 position)
        {
            if (multiplayerMode)
            {
                spawnGameObject.Invoke(prefab, position);
                return spawnedObject;
            }
            else
            {
                return Instantiate(prefab, position, Quaternion.identity);
            }
        }

        public static Action<PickupableItem, int> setItemCount = delegate { }; // ITEM TYPE, COUNT

        public static void SetItemCount(PickupableItem item, int dropCount)
        {
            if (InventoryGameManager.multiplayerMode) setItemCount.Invoke(item, dropCount);
            else item.itemCount = dropCount;
        }

        public static Action<PickupableItem, float> setDurabilityToPItem = delegate { }; // ITEM, DURABILITY

        public static void SetDurabilityToPItem(PickupableItem pItem, float itemDurability)
        {
            if (multiplayerMode) setDurabilityToPItem.Invoke(pItem, itemDurability);
            else pItem.itemDurability = itemDurability;
        }

        public static EventTrigger.Entry NewEventTrigerEvent(EventTrigger trigger, EventTriggerType type)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            trigger.triggers.Add(entry);
            return entry;
        }

        public static Action<MonoBehaviour, float> syncIDamagableTakeDamage = delegate { };

        public static void SyncIDamageableTakeDamage(MonoBehaviour targetComp, float damage)
        {
            if (multiplayerMode) syncIDamagableTakeDamage.Invoke(targetComp, damage);
            else SyncTakeDamageF(targetComp, damage);
        }

        public static void SyncTakeDamageF(MonoBehaviour targetComp, float damage)
        {
            print("TAKING DAMAGE");

            IDamageable targetInterface = GetInterfaceOnGameObject<IDamageable>(targetComp);

            targetInterface.TakeDamage(damage);
        }

        private static T GetInterfaceOnGameObject<T>(Component component) where T : class
        {
            Component[] components = component.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is T) return components[i] as T;
            }

            return null;
        }

        public static Action onItemSpawned = delegate { };

        public static GameObject SpawnItem(ItemInInventory itemToSpawn, Vector3 position, int itemCount) // ITEM COUNT STANDS FOR ITEM COUNT IN PICKUPABLE ITEM, NOT INSTANCES COUNT
        {
            GameObject clone = SpawnGameObjectForAll(itemToSpawn.item.object3D, position);

            if (multiplayerMode)
            {
                SetItemCount(clone.GetComponent<PickupableItem>(), itemCount);
                SetDurabilityToPItem(clone.GetComponent<PickupableItem>(), itemToSpawn.durability);
            }
            else
            {
                clone.GetComponent<PickupableItem>().itemCount = itemCount;
                clone.GetComponent<PickupableItem>().item_item = itemToSpawn.item;
                clone.GetComponent<PickupableItem>().itemDurability = itemToSpawn.durability;
            }

            onItemSpawned.Invoke();

            return clone;
        }

        public static Action<Vector3> spawnPlayer_ = delegate { };

        public static Action<InventoryCore> onPlayerSpawned = delegate { };

        public static GameObject SpawnPlayer(Vector3 position)
        {
            spawnPlayer_.Invoke(position);
            onPlayerSpawned.Invoke(spawnedObject.GetComponent<InventoryCore>());
            return spawnedObject;
        }

        // --- PLAYER RESPAWNING
        public void TryOpenPlayerRespawnMenu(bool open) { if (!playerRespawnMenu) return; OpenPlayerRespawnMenu(open); }

        private void OpenPlayerRespawnMenu(bool open) { playerRespawnMenu.SetActive(open); Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked; }

        public void RespawnPlayer()
        {
            SpawnPlayer(defaultPlayerSpawnPos);
            OpenPlayerRespawnMenu(false);
        }

        public static readonly Vector3 defaultPlayerSpawnPos = new Vector3(0, 10, 0);
    }
}
