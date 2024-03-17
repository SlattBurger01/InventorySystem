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
        public static int playerToSpawnId = 0; // this id is also used for player respawning (only one player per session is expected)
        // -----

        public int gameSceneBuildIndex;

        [Tooltip("Object that will be enabled on localPlayer's dead")]
        [SerializeField] private GameObject playerRespawnMenu;

        [Header("PREFABS")]
        public GameObject[] playerPrefabs;
        public static GameObject[] pPrefabs;

        [Header("MULTIPLAYER SETTINGS")]
        public bool multiplayerMode_ = true;
        [SerializeField] private bool offlineMode;

        public static bool multiplayerMode;

        [HideInNormalInspector] public InventoryCore localPlayer;

        public static Action updateIsMasterclientBool = delegate { };
        public static bool m_isMasterClient = true;
        public static bool IsMasterClient { get { updateIsMasterclientBool.Invoke(); return m_isMasterClient; } }

        private void Awake()
        {
            pPrefabs = playerPrefabs;
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
            TryStartStackHandlerCoroutine();

            localPlayer = FindObjectOfType<InventoryCore>();
        }

        public static void TryStartStackHandlerCoroutine()
        {
            PickupableItemsStacksHandler pStacksHandler = FindObjectOfType<PickupableItemsStacksHandler>();
            if (pStacksHandler) pStacksHandler.StartLoop();
        }

        public static void TrySetUpSaveAndLoadSystemAndLoadRecentSave()
        {
            print($"SSED: {gameSaveToLoad}");

            if (gameSaveToLoad == null || gameSaveToLoad.savesName.Length == 0) return;

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

        /// <summary> Synchronizes item count with all players </summary>
        public static void SetItemCount(PickupableItem item, int dropCount)
        {
            if (multiplayerMode) setItemCount.Invoke(item, dropCount);
            else item.itemCount = dropCount;
        }

        public static Action<PickupableItem, float> setDurabilityToPItem = delegate { }; // ITEM, DURABILITY

        /// <summary> Synchronizes durability with all players </summary>
        public static void SetDurabilityToPItem(PickupableItem pItem, float itemDurability)
        {
            if (multiplayerMode) setDurabilityToPItem.Invoke(pItem, itemDurability);
            else pItem.itemDurability = itemDurability;
        }

        /// <summary> creates new trigger entry (based on type) and adds it into 'trigger.triggers' array </summary>
        /// <returns></returns>
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

            PickupableItem pItem = clone.GetComponent<PickupableItem>();

            if (multiplayerMode)
            {
                SetItemCount(pItem, itemCount);
                SetDurabilityToPItem(pItem, itemToSpawn.durability);
            }
            else
            {
                pItem.SetValues(itemCount, itemToSpawn.durability);

                /*pItem.itemCount = itemCount;
                pItem.item_item = itemToSpawn.item;
                pItem.itemDurability = itemToSpawn.durability;*/
            }

            onItemSpawned.Invoke();

            return clone;
        }

        public static Action<Vector3, int> spawnPlayer_ = delegate { };
        public static Action<InventoryCore> onPlayerSpawned = delegate { };

        public static GameObject SpawnPlayer(Vector3 position, int prefabId)
        {
            spawnPlayer_.Invoke(position, prefabId);
            onPlayerSpawned.Invoke(spawnedObject.GetComponent<InventoryCore>());
            return spawnedObject;
        }

        public void OnPlayerDead()
        {
            TryOpenPlayerRespawnMenu(true);
            DestroyObjectForAll(gameObject);
        }

        // --- PLAYER RESPAWNING
        /// <summary> Changes respawn menu state based on 'open' bool (if exists) </summary>
        public void TryOpenPlayerRespawnMenu(bool open)
        { 
            if (playerRespawnMenu) OpenPlayerRespawnMenu(open);
        }

        /// <summary> Sets menu active state and cursor lock state (based on 'open') </summary>
        private void OpenPlayerRespawnMenu(bool open)
        { 
            playerRespawnMenu.SetActive(open); 
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked; 
        }

        /// <summary> Spawns player ('playerToSpawnId') on 'position' </summary>
        public void RespawnPlayerDefault(Vector3 position) => RespawnPlayer(position, playerToSpawnId);

        public void RespawnPlayer(Vector3 position, int id)
        {
            SpawnPlayer(position, id);
            TryOpenPlayerRespawnMenu(false);
        }

        public void RespawnPlayerOnDefaultSpawnPos() => RespawnPlayerDefault(defaultPlayerSpawnPos);

        public static readonly Vector3 defaultPlayerSpawnPos = new Vector3(0, 10, 0);
    }
}
