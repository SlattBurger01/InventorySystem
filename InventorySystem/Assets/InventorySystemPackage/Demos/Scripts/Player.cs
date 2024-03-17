using InventorySystem.EscMenu;
using InventorySystem.Inventory_;
using InventorySystem.Items;
using InventorySystem.PageContent;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour, IDamageable
    {
        private string playerName;

        // COMPONENTS
        private CharacterController controller;

        [Header("OBJECTS")]
        [SerializeField] private Transform head;
        [SerializeField] private GameObject crossHair;
        [SerializeField] private TextMeshProUGUI nameText;

        [Header("SLIDERS")]
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider hpSupportSlider;

        // GRAVITY
        private Vector3 velocity;
        private bool grounded;
        private float fallDamage;

        // ROTATION
        [HideInInspector] public float xRotation, yRotation;
        private bool rotationPaused;
        private float mouseSensitivity = 100;

        // MOVEMENT
        private float movementSpeed = 10;
        private float currentSpeed;

        // STATS
        // BASE STATS
        [Header("STATS")]
        [SerializeField] private float baseMaxStamina;
        private float maxStamina;
        [HideInInspector] public float stamina;

        [SerializeField] private float baseMaxHp;
        private float maxHp;
        [HideInInspector] public float hp;

        // OTHER STATS
        [SerializeField] private float baseArmor;
        public float armor;

        [SerializeField] private float baseStrenght;
        private float strenght;

        private InventoryCore core;
        public bool isDead;

        public Action onDead = delegate { };

        private void Awake()
        {
            core = GetComponent<InventoryCore>();
            controller = GetComponent<CharacterController>();

            core.onNicknameSet += SetNickName;
        }

        private void LockMovementAndRotation(bool pause) { LockMovement(pause); PauseRotation(pause); }

        public void SetNickName(string nickName) // PhotonInventoryGameManager
        {
            playerName = nickName;
            nameText.text = nickName;
        }

        private void Start()
        {
            OnItemEquip_UpdateStats(new List<ItemInInventory>()); // SET DEFAULT STATS

            hp = maxHp;
            stamina = maxStamina;
            currentSpeed = movementSpeed;

            UpdateSliders_Def();

            GetComponent<Inventory>().OnItemEquiped_ChangeStats += OnItemEquip_UpdateStats;

            InventoryMenu menu = GetComponent<InventoryMenu>();
            EscMenuManager manager = FindObjectOfType<EscMenuManager>();

            if (menu) menu.onMenuOpenStateChange += LockMovementAndRotation;
            if (manager) manager.onMenuOpened += LockMovementAndRotation;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private bool movementLocked;

        [HideInNormalInspector] private int mLockStackCount = 0; // HOW MANY MENUS LOCKED THIS
        [HideInNormalInspector] private int pLockStackCount = 0; // HOW MANY MENUS LOCKED THIS

        private void LockMovement(bool lock_)
        {
            Lock(lock_, ref mLockStackCount, ref movementLocked);
        }

        private void PauseRotation(bool pause)
        {
            if (isDead) return;

            Lock(pause, ref pLockStackCount, ref rotationPaused);

            Cursor.lockState = rotationPaused ? CursorLockMode.None : CursorLockMode.Locked;
            if (crossHair) crossHair.SetActive(!rotationPaused);
        }

        private static void Lock(bool pause, ref int count, ref bool lock_)
        {
            count += pause ? 1 : -1;

            if (count < 0) count = 0;

            lock_ = count != 0;
        }

        private void OnDestroy()
        {
            GetComponent<Inventory>().OnItemEquiped_ChangeStats -= OnItemEquip_UpdateStats;
        }

        private void Update()
        {
            if (!core.isMine) return;
            if (isDead) return;

            GravityHandler();

            if (!movementLocked)
            {
                MovementHandler();
                RotationHandler();
            }

            UiHandler();

            if (Input.GetKeyDown(KeyCode.P)) Heal(20);
        }

        private Vector2 sliderPos01 = new Vector2(-50, -40), sliderPos02 = new Vector2(-50, -110);

        public void Heal(float amount) { (this as IDamageable).TakeDamage(-amount); }

        void IDamageable.TakeDamage(float damage)
        {
            hp -= damage;

            if (hp <= 0) Die();
        }

        public void Die()
        {
            if (isDead) return;

            isDead = true;

            EscMenuManager manager = FindObjectOfType<EscMenuManager>();
            if (manager) manager.OpenMenu(false);

            onDead.Invoke();

            FindObjectOfType<InventoryGameManager>().OnPlayerDead();
        }

        //private Slider hpSupportSlider;
        private float displayedHp;

        private void UiHandler()
        {
            if (displayedHp > hp) displayedHp -= Time.deltaTime * 25;
            else displayedHp = hp;

            UpdateHpSlider();
            UpdateStaminaSlider();
            UpdateSlidersPositions();
        }

        private void UpdateHpSlider()
        {
            if (!hpSlider) return;

            hpSupportSlider.value = displayedHp;
            hpSlider.value = hp;

            hpSlider.transform.parent.gameObject.SetActive(hp != maxHp);
        }

        private void UpdateStaminaSlider()
        {
            if (!staminaSlider) return;

            staminaSlider.value = stamina;

            staminaSlider.gameObject.SetActive(stamina != maxStamina);
        }

        private void UpdateSlidersPositions()
        {
            if (!hpSlider || !staminaSlider) return;

            if (hpSlider.transform.parent.gameObject.activeSelf)
            {
                hpSlider.transform.parent.GetComponent<RectTransform>().anchoredPosition = sliderPos01;
                staminaSlider.GetComponent<RectTransform>().anchoredPosition = sliderPos02;
            }
            else
            {
                hpSlider.transform.parent.GetComponent<RectTransform>().anchoredPosition = sliderPos02;
                staminaSlider.GetComponent<RectTransform>().anchoredPosition = sliderPos01;
            }
        }

        private void RotationHandler()
        {
            if (rotationPaused) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            yRotation += mouseX;

            UpdateRotation();
        }

        public void UpdateRotation() // SaveAndLoadSystem
        {
            head.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        }

        private void MovementHandler()
        {
            Vector3 move = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");

            bool exhausted = stamina < maxStamina / 10;

            CurrentSpeedHandler(move, exhausted);

            controller.Move(move * currentSpeed * Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && grounded) velocity.y = exhausted ? 3 : 7;

            controller.stepOffset = grounded ? .3f : 0;
        }

        private void CurrentSpeedHandler(Vector3 move, bool exhausted) // AND STAMINA HANDLER
        {
            float targetSpeed = movementSpeed;

            float staminaIncreaseRate = 0;

            if (move != Vector3.zero)
            {
                if (!exhausted)
                {
                    if (Input.GetKey(KeyCode.LeftShift)) // SPRINTING
                    {
                        targetSpeed = movementSpeed * 1.5f;
                        staminaIncreaseRate = -Time.deltaTime * 10;
                    }
                    else
                    {
                        if (stamina < maxStamina) staminaIncreaseRate = Time.deltaTime / 3; // WALKING
                    }
                }
                else
                {
                    targetSpeed = movementSpeed / 1.5f; // SLOW WALKING
                    staminaIncreaseRate = -Time.deltaTime * 1.5f;
                }
            }
            else
            {
                if (move == Vector3.zero)
                {
                    if (stamina < maxStamina) staminaIncreaseRate = Time.deltaTime * 10; // STAING STILL
                }
            }

            stamina += staminaIncreaseRate;
            if (stamina > maxStamina) stamina = maxStamina;

            if (currentSpeed <= targetSpeed - Time.deltaTime * 5) currentSpeed += Time.deltaTime * 5;
            else if (currentSpeed >= targetSpeed + Time.deltaTime * 15) currentSpeed -= Time.deltaTime * 15;
            else currentSpeed = targetSpeed;
        }

        private void GravityHandler()
        {
            controller.Move(velocity * Time.deltaTime);
            grounded = controller.isGrounded;

            if (grounded)
            {
                velocity.y = -2;

                if (fallDamage > 7.5f) (this as IDamageable).TakeDamage(fallDamage);

                fallDamage = 0;
            }
            else
            {
                velocity.y -= Time.deltaTime * 25;
                fallDamage += Mathf.Abs(velocity.y) * Time.deltaTime;
            }
        }

        private void OnItemEquip_UpdateStats(List<ItemInInventory> equipedItems)
        {
            // RESET CURRENT VALUES TO BASE VALUES
            maxStamina = baseMaxStamina;
            maxHp = baseMaxHp;
            armor = baseArmor;
            strenght = baseStrenght;

            // ADD ITEM STATS
            for (int i = 0; i < equipedItems.Count; i++)
            {
                Item it = equipedItems[i].item;

                float durabPrecentage = it.baseStatsOnDurability ? equipedItems[i].durability / it.maxDurability : 1;

                if (it.TryGetCustomValue<float>("addStamina", out float val01)) maxStamina += val01;
                if (it.TryGetCustomValue<float>("addHps", out float val02)) maxHp += val02 * durabPrecentage;
                if (it.TryGetCustomValue<float>("addArmor", out float val03)) armor += val03 * durabPrecentage;
                if (it.TryGetCustomValue<float>("addStrenght", out float val04)) strenght += val04 * durabPrecentage;
            }

            UpdateSliders_Def();
        }

        private void UpdateSliders_Def()
        {
            Sliders_UpdateStamina();
            Sliders_UpdateHp();
        }

        private void Sliders_UpdateHp()
        {
            if (!hpSlider) return;

            hpSlider.maxValue = maxHp;
            hpSlider.value = hp;

            hpSupportSlider.maxValue = maxHp;
            hpSupportSlider.maxValue = hp;
        }

        private void Sliders_UpdateStamina()
        {
            if (!staminaSlider) return;

            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = stamina;
        }
    }
}
