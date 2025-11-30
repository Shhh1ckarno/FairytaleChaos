using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections; // Required for Coroutines (HP Animation)

// Add required components automatically
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class CardController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ========================================================================================
    //                                    DATA AND STATE
    // ========================================================================================

    [Header("Data")]
    public CardData data;

    [Header("Current State")]
    [Tooltip("Current Health Points.")]
    public int currentHP;

    [HideInInspector] public SlotController currentSlot; // The slot this card occupies

    [HideInInspector] // Скрываем в Инспекторе, т.к. устанавливается кодом
    public bool isPlayerCard = false;

    // --- SUPPORT FLAGS (Buffs from Calcifer etc.) ---
    [Header("Buff State")]
    [SerializeField] private bool hasBlock = false;      // Tempering (Block first damage)
    [SerializeField] private bool hasRebirth = false;    // Smelting (Rebirth with 1 HP)
    [SerializeField] private bool isOnFire = false;      // Ignition (Bonus Damage)

    [HideInInspector] public int bonusDamage = 0;        // Bonus damage value from Fire

    // ========================================================================================
    //                                 DRAG & DROP SETTINGS
    // ========================================================================================

    [Header("Drag & Drop Settings")]
    public LayerMask slotLayer;      // Layer for DropZones
    public float dragHeight = 0.1f;  // Height lift when dragging

    // --- Positioning Variables ---
    private Vector3 handPosition;
    private Quaternion handRotation;
    private Transform originalParent;

    // --- Components ---
    private Collider cardCollider;
    private Rigidbody rb;
    private bool isDragging = false;
    private CardDisplay cardDisplay;

    // --- Managers ---
    private HandManager handManager;
    private BattleManager battleManager;

    // ========================================================================================
    //                                 INITIALIZATION
    // ========================================================================================

    private void Awake()
    {
        handManager = HandManager.Instance;
        battleManager = BattleManager.Instance;
        cardDisplay = GetComponent<CardDisplay>();
    }

    private void Start()
    {
        cardCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Save initial parent (usually HandRoot)
        if (transform.parent != null)
        {
            originalParent = transform.parent;
        }

        // Set correct rotation for hand
        handRotation = Quaternion.Euler(90f, 90f, 0f);

        // Apply rotation immediately if not dragging
        if (!isDragging && currentSlot == null)
        {
            transform.localRotation = handRotation;
        }

        // Fallback initialization
        if (data != null && currentHP == 0)
        {
            currentHP = data.maxHP;
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Main setup method called by HandManager.
    /// </summary>
    public void Initialize(CardData cardData)
    {
        data = cardData;

        // Save parent assigned by HandManager
        originalParent = transform.parent;

        // Visual setup
        if (cardDisplay != null)
        {
            cardDisplay.DisplayCardData(data);
        }

        // HP setup
        currentHP = data.maxHP;
        UpdateVisuals();
    }

    // ========================================================================================
    //                                DRAG & DROP HANDLING
    // ========================================================================================

    public void OnPointerDown(PointerEventData eventData) { }

    // CardController.cs (Добавляем проверку в OnBeginDrag)

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Проверка: Если карта в слоте, двигать нельзя
        if (currentSlot != null) return;

        // 2. НОВАЯ ПРОВЕРКА: Если это карта врага, двигать нельзя!
        // Как понять, что карта врага?
        // Самый простой способ: если она не в руке и не в слоте, или если мы пометили её.

        // Добавим простой флаг, который мы не использовали, но сейчас пригодится.
        // Или проверим, является ли она ребенком HandRoot игрока.

        if (HandManager.Instance != null && !HandManager.Instance.HandContains(this))
        {
            // Если карты нет в руке игрока, значит это карта врага (или она уже на столе)
            return;
        }

        isDragging = true;
        transform.SetParent(null);
        if (cardCollider != null) cardCollider.enabled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            transform.position = hit.point + Vector3.up * dragHeight;
        }

        // OPTIONAL: Add cost warning logic here (turn red if FP too low)
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (Camera.main == null) { ReturnToHand(); return; }

        SlotController targetSlot = null;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        // 1. Find Slot
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, slotLayer))
        {
            targetSlot = hit.collider.GetComponent<SlotController>();
        }

        // 2. Validate Slot & Cost
        bool isSlotValid = targetSlot != null && !targetSlot.IsOccupied() && !targetSlot.isEnemySlot;
        bool isCostMet = false;

        if (isSlotValid && battleManager != null && data != null)
        {
            isCostMet = battleManager.TrySpendFearPoints(BattleManager.PlayerType.Player, data.costFear);
        }

        // 3. Success or Fail
        if (isSlotValid && isCostMet)
        {
            PlayCard(targetSlot);
        }
        else
        {
            if (!isCostMet && isSlotValid)
            {
                Debug.LogWarning($"[Card] Not enough FP for {data.displayName}.");
            }

            ReturnToHand();

            if (handManager != null)
            {
                handManager.UpdateHandPositions();
            }
        }
    }

    private void PlayCard(SlotController targetSlot)
    {
        currentSlot = targetSlot;
        targetSlot.SetOccupant(this);

        if (handManager != null)
        {
            handManager.RemoveFromHand(this);
        }

        if (battleManager != null)
        {
            battleManager.RegisterCardDeployment(this, BattleManager.PlayerType.Player);
        }
    }

    // ========================================================================================
    //                                HELPER METHODS
    // ========================================================================================

    public void ReturnToHand()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }
        else if (handManager != null && handManager.handRoot != null)
        {
            transform.SetParent(handManager.handRoot);
            originalParent = handManager.handRoot;
        }

        if (handManager != null && !handManager.HandContains(this))
        {
            handManager.AddCardBackToHand(this);
        }

        transform.localPosition = handPosition;
        transform.localRotation = handRotation;
        currentSlot = null;
    }

    public void SetHandPosition(Vector3 pos)
    {
        handPosition = pos;
        if (currentSlot == null)
        {
            transform.localPosition = pos;
            transform.localRotation = handRotation;
        }
    }

    private void UpdateVisuals()
    {
        if (cardDisplay != null)
        {
            cardDisplay.UpdateHPText(currentHP);

            // If we have bonus damage, update attack text too
            if (isOnFire)
            {
                cardDisplay.UpdateAttackText(GetTotalAttack());
            }
        }
    }

    /// <summary>
    /// Gets total attack including buffs.
    /// </summary>
    public int GetTotalAttack()
    {
        return data.attack + bonusDamage;
    }

    // ========================================================================================
    //                            COMBAT & SUPPORT LOGIC
    // ========================================================================================

    /// <summary>
    /// Takes damage, accounting for Block. Starts HP animation.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (hasBlock)
        {
            Debug.Log($"{data.displayName} blocked damage (Tempering)!");
            RemoveBlock(); // Block is one-time use
            return;
        }

        // Calculate new HP
        int newHP = currentHP - damage;

        // Start smooth animation
        StartCoroutine(AnimateHealthChange(newHP));

        Debug.Log($"{data.displayName} took {damage} damage.");
        CheckDeath(newHP); // Pass the future HP to check death logic
    }

    /// <summary>
    /// Heals the unit (Winnie Pooh support).
    /// </summary>
    public void Heal(int amount)
    {
        int newHP = Mathf.Min(currentHP + amount, data.maxHP);
        StartCoroutine(AnimateHealthChange(newHP));
        Debug.Log($"{data.displayName} healed for {amount}.");
    }

    // --- BUFF METHODS ---

    /// <summary>
    /// Apply "Tempering" (Block).
    /// </summary>
    public void ApplyBlock()
    {
        hasBlock = true;
        if (cardDisplay != null) cardDisplay.UpdateStatusText("ЗАКАЛКА", true); // TEMPERING
        Debug.Log($"{data.displayName} received Block.");
    }

    private void RemoveBlock()
    {
        hasBlock = false;
        if (cardDisplay != null) cardDisplay.UpdateStatusText("", false);
    }

    /// <summary>
    /// Apply "Smelting" (Rebirth).
    /// </summary>
    public void ApplyRebirth()
    {
        hasRebirth = true;
        if (cardDisplay != null) cardDisplay.UpdateStatusText("ПЕРЕПЛАВКА", true); // REBIRTH
        Debug.Log($"{data.displayName} received Rebirth.");
    }

    /// <summary>
    /// Apply "Ignition" (Fire/Bonus Damage).
    /// </summary>
    public void ApplyOnFire(int bonusAtk)
    {
        if (isOnFire) return;

        isOnFire = true;
        bonusDamage = bonusAtk;

        if (cardDisplay != null)
        {
            cardDisplay.UpdateStatusText("ПОДЖОГ", true); // IGNITION
            cardDisplay.UpdateAttackText(GetTotalAttack());
        }
        Debug.Log($"{data.displayName} is On Fire! (+{bonusAtk} ATK).");
    }

    /// <summary>
    /// Remove all debuffs (Cauterization).
    /// </summary>
    public void RemoveAllDebuffs()
    {
        // 1. Remove Fire
        if (isOnFire)
        {
            isOnFire = false;
            bonusDamage = 0;
            if (cardDisplay != null)
            {
                cardDisplay.UpdateStatusText("", false);
                cardDisplay.UpdateAttackText(data.attack); // Reset attack text
            }
        }

        // Add logic for other debuffs here...
        Debug.Log($"{data.displayName}: All debuffs removed.");
    }

    // --- DEATH LOGIC ---

    private void CheckDeath(int futureHP)
    {
        // We check against the future HP because animation takes time,
        // but logic should happen immediately or after animation.
        // For now, let's update currentHP immediately for logic, 
        // but visual is handled by Coroutine.

        // Wait for animation to finish before destroying? 
        // For simplicity in this turn-based logic, we check immediately.

        if (futureHP <= 0)
        {
            if (hasRebirth)
            {
                Debug.Log($"{data.displayName} REBIRTHS!");

                // Rebirth logic
                int rebirthHP = 1;
                StartCoroutine(AnimateHealthChange(rebirthHP)); // Animate back to 1

                hasRebirth = false;
                if (cardDisplay != null) cardDisplay.UpdateStatusText("", false); // Remove Rebirth text
                return;
            }

            // Delay death slightly to allow hit animation
            StartCoroutine(DieRoutine());
        }
    }

    private IEnumerator AnimateHealthChange(int targetHP, float duration = 0.3f)
    {
        float startTime = Time.time;
        int initialHP = currentHP;
        currentHP = targetHP; // Update logic immediately

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            int animatedHP = (int)Mathf.Lerp(initialHP, targetHP, t);

            if (cardDisplay != null)
                cardDisplay.UpdateHPText(animatedHP);

            yield return null;
        }

        if (cardDisplay != null)
            cardDisplay.UpdateHPText(targetHP);
    }

    private IEnumerator DieRoutine()
    {
        // Wait for a moment so player sees 0 HP
        yield return new WaitForSeconds(0.5f);
        Die();
    }

    private void Die()
    {
        Debug.Log($"Card {data.displayName} destroyed.");

        if (currentSlot != null)
        {
            currentSlot.ClearOccupant();
        }

        if (battleManager != null)
        {
            battleManager.DeregisterCard(this);
        }

        Destroy(gameObject);
    }
}