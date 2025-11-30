using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class CardController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ... (Остальные переменные CardData, currentHP и т.д. остаются без изменений) ...
    [Header("Data")]
    public CardData data;
    public int currentHP;
    [HideInInspector] public SlotController currentSlot;
    [HideInInspector] public bool isPlayerCard = false;

    // Флаг защиты от повторной смерти
    private bool isDead = false;

    // ... (Buff State переменные: hasBlock, hasRebirth, isOnFire, bonusDamage) ...
    [Header("Buff State")]
    [SerializeField] private bool hasBlock = false;
    [SerializeField] private bool hasRebirth = false;
    [SerializeField] private bool isOnFire = false;
    [HideInInspector] public int bonusDamage = 0;

    // ... (Настройки Drag & Drop) ...
    [Header("Drag & Drop Settings")]
    public LayerMask slotLayer;
    public float dragHeight = 0.1f;
    private Vector3 handPosition;
    private Quaternion handRotation;
    private Transform originalParent;
    private Collider cardCollider;
    private Rigidbody rb;
    private bool isDragging = false;
    private CardDisplay cardDisplay;
    private HandManager handManager;
    private BattleManager battleManager;

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
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        if (transform.parent != null) originalParent = transform.parent;

        handRotation = Quaternion.Euler(90f, 90f, 0f);
        if (!isDragging && currentSlot == null) transform.localRotation = handRotation;

        if (data != null && currentHP == 0) { currentHP = data.maxHP; UpdateVisuals(); }
    }

    public void Initialize(CardData cardData)
    {
        data = cardData;
        originalParent = transform.parent;
        if (cardDisplay != null) cardDisplay.DisplayCardData(data);
        currentHP = data.maxHP;
        UpdateVisuals();
    }

    // ... (Методы Drag & Drop: OnPointerDown, OnBeginDrag, OnDrag, OnEndDrag, PlayCard - БЕЗ ИЗМЕНЕНИЙ) ...
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot != null) return;
        if (HandManager.Instance != null && !HandManager.Instance.HandContains(this)) return;
        isDragging = true;
        transform.SetParent(null);
        if (cardCollider != null) cardCollider.enabled = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)) transform.position = hit.point + Vector3.up * dragHeight;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (Camera.main == null) { ReturnToHand(); return; }
        SlotController targetSlot = null;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, slotLayer)) targetSlot = hit.collider.GetComponent<SlotController>();

        bool isSlotValid = targetSlot != null && !targetSlot.IsOccupied() && !targetSlot.isEnemySlot;
        bool isCostMet = false;
        if (isSlotValid && battleManager != null && data != null) isCostMet = battleManager.TrySpendFearPoints(BattleManager.PlayerType.Player, data.costFear);

        if (isSlotValid && isCostMet) PlayCard(targetSlot);
        else
        {
            ReturnToHand();
            if (handManager != null) handManager.UpdateHandPositions();
        }
    }
    private void PlayCard(SlotController targetSlot)
    {
        currentSlot = targetSlot;
        targetSlot.SetOccupant(this);
        if (handManager != null) handManager.RemoveFromHand(this);
        if (battleManager != null) battleManager.RegisterCardDeployment(this, BattleManager.PlayerType.Player);
    }

    // ... (Helper Methods: ReturnToHand, SetHandPosition, UpdateVisuals, GetTotalAttack - БЕЗ ИЗМЕНЕНИЙ) ...
    public void ReturnToHand()
    {
        if (originalParent != null) transform.SetParent(originalParent);
        else if (handManager != null && handManager.handRoot != null) { transform.SetParent(handManager.handRoot); originalParent = handManager.handRoot; }
        if (handManager != null && !handManager.HandContains(this)) handManager.AddCardBackToHand(this);
        transform.localPosition = handPosition;
        transform.localRotation = handRotation;
        currentSlot = null;
    }
    public void SetHandPosition(Vector3 pos) { handPosition = pos; if (currentSlot == null) { transform.localPosition = pos; transform.localRotation = handRotation; } }
    private void UpdateVisuals() { if (cardDisplay != null) { cardDisplay.UpdateHPText(currentHP); if (isOnFire) cardDisplay.UpdateAttackText(GetTotalAttack()); } }
    public int GetTotalAttack() { return data.attack + bonusDamage; }

    // ... (Logic: TakeDamage, Heal, ApplyBlock, ApplyRebirth, ApplyOnFire, RemoveBlock, RemoveAllDebuffs - БЕЗ ИЗМЕНЕНИЙ) ...
    public void TakeDamage(int damage)
    {
        if (hasBlock) { Debug.Log($"{data.displayName} blocked damage!"); RemoveBlock(); return; }
        int newHP = currentHP - damage;
        StartCoroutine(AnimateHealthChange(newHP));
        Debug.Log($"{data.displayName} took {damage} damage.");
        CheckDeath(newHP);
    }
    public void Heal(int amount) { int newHP = Mathf.Min(currentHP + amount, data.maxHP); StartCoroutine(AnimateHealthChange(newHP)); }
    public void ApplyBlock() { hasBlock = true; if (cardDisplay != null) cardDisplay.UpdateStatusText("ЗАКАЛКА", true); }
    private void RemoveBlock() { hasBlock = false; if (cardDisplay != null) cardDisplay.UpdateStatusText("", false); }
    public void ApplyRebirth() { hasRebirth = true; if (cardDisplay != null) cardDisplay.UpdateStatusText("ПЕРЕПЛАВКА", true); }
    public void ApplyOnFire(int bonusAtk) { if (isOnFire) return; isOnFire = true; bonusDamage = bonusAtk; if (cardDisplay != null) { cardDisplay.UpdateStatusText("ПОДЖОГ", true); cardDisplay.UpdateAttackText(GetTotalAttack()); } }
    public void RemoveAllDebuffs() { if (isOnFire) { isOnFire = false; bonusDamage = 0; if (cardDisplay != null) { cardDisplay.UpdateStatusText("", false); cardDisplay.UpdateAttackText(data.attack); } } }

    // --- ЛОГИКА СМЕРТИ (ОБНОВЛЕННАЯ) ---

    private void CheckDeath(int futureHP)
    {
        if (isDead) return; // Если уже мертва, не трогаем

        if (futureHP <= 0)
        {
            if (hasRebirth)
            {
                Debug.Log($"{data.displayName} ПЕРЕРОЖДАЕТСЯ!");
                int rebirthHP = 1;
                StartCoroutine(AnimateHealthChange(rebirthHP));
                hasRebirth = false;
                if (cardDisplay != null) cardDisplay.UpdateStatusText("", false);
                return;
            }

            // Ставим флаг смерти СРАЗУ, чтобы не вызвать логику дважды
            isDead = true;
            StartCoroutine(DieRoutine());
        }
    }

    private IEnumerator AnimateHealthChange(int targetHP, float duration = 0.3f)
    {
        float startTime = Time.time;
        int initialHP = currentHP;
        currentHP = targetHP;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            int animatedHP = (int)Mathf.Lerp(initialHP, targetHP, t);
            if (cardDisplay != null) cardDisplay.UpdateHPText(animatedHP);
            yield return null;
        }
        if (cardDisplay != null) cardDisplay.UpdateHPText(targetHP);
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(0.5f); // Ждем анимацию
        Die();
    }

    private void Die()
    {
        Debug.Log($"Card {data.displayName} destroyed.");

        // 1. Очищаем слот
        if (currentSlot != null)
        {
            currentSlot.ClearOccupant();
        }

        // 2. Удаляем из списков битвы
        if (battleManager != null)
        {
            battleManager.DeregisterCard(this);
        }

        // 3. Уничтожаем объект
        Destroy(gameObject);
    }
}