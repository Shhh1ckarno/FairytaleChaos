using UnityEngine;
using UnityEngine.EventSystems;

// Добавляем необходимые компоненты
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class CardController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // --- ДАННЫЕ И СОСТОЯНИЕ ---

    [Header("Данные и состояние")]
    public CardData data;

    [Tooltip("Текущее здоровье карты. Инициализируется в Start().")]
    public int currentHP;

    [HideInInspector] public SlotController currentSlot; // Слот, в котором находится карта

    // --- Настройки Drag & Drop ---
    [Header("Настройки Drag & Drop")]
    public LayerMask slotLayer;
    public float dragHeight = 0.1f; // Высота подъема карты при перетаскивании

    // --- Приватные переменные для локальной позиции ---
    private Vector3 handPosition;
    private Quaternion handRotation; // Сохраняет желаемое ЛОКАЛЬНОЕ вращение
    private Transform originalParent; // Ссылка на HandRoot (родительский Transform)
    private Collider cardCollider;
    private Rigidbody rb;
    private bool isDragging = false;
    private CardDisplay cardDisplay; // Для обновления UI

    // --- ИНТЕГРАЦИЯ С МЕНЕДЖЕРАМИ ---
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

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Сохраняем родителя (HandRoot)
        originalParent = transform.parent;

        // Сохраняем начальную ЛОКАЛЬНУЮ позицию
        handPosition = transform.localPosition;

        // *** КОРРЕКЦИЯ ВРАЩЕНИЯ: Установка желаемого ЛОКАЛЬНОГО вращения ***
        // (X=90, Y=90, Z=0) - типичное вращение для вертикальной карты,
        // повернутой к игроку/камере на плоскости.
        handRotation = Quaternion.Euler(90f, 90f, 0f);

        // Применяем вращение немедленно
        transform.localRotation = handRotation;

        // ИНИЦИАЛИЗАЦИЯ HP
        if (data != null)
        {
            currentHP = data.maxHP;
        }
    }

    // --- ОБРАБОТЧИКИ DRAG & DROP ---

    public void OnPointerDown(PointerEventData eventData)
    {
        // Логика при клике
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSlot != null)
        {
            return;
        }

        isDragging = true;
        // Отключаем родителя для Dragging в мировых координатах
        transform.SetParent(null);
        if (cardCollider != null) cardCollider.enabled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Используем transform.position в мировых координатах
            transform.position = hit.point + Vector3.up * dragHeight;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (Camera.main == null) { ReturnToHand(); return; }

        SlotController targetSlot = null;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        // 1. Поиск DropZone (SlotController)
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, slotLayer))
        {
            targetSlot = hit.collider.GetComponent<SlotController>();
        }

        // 2. Проверки перед выставлением (доступность слота, FP)
        bool isSlotValid = targetSlot != null && !targetSlot.IsOccupied() && !targetSlot.isEnemySlot;
        bool isCostMet = false;

        if (isSlotValid && battleManager != null && data != null)
        {
            isCostMet = battleManager.TrySpendFearPoints(
                BattleManager.PlayerType.Player,
                data.costFear
            );
        }

        // 3. Результат выставления
        if (isSlotValid && isCostMet)
        {
            // Успешный розыгрыш
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
        else
        {
            // Неудача: вернуть карту в руку
            if (!isCostMet && isSlotValid)
            {
                Debug.LogWarning($"Ошибка: Недостаточно FP для {data.displayName}.");
            }
            ReturnToHand();
        }
    }

    // --- Вспомогательные методы ---

    /// <summary>
    /// Возвращает карту в руку игрока (в позицию, сохраненную HandManager).
    /// </summary>
    public void ReturnToHand()
    {
        // 1. Устанавливаем родителя (HandRoot)
        if (originalParent != null)
            transform.SetParent(originalParent);

        // 2. Применяем ЛОКАЛЬНУЮ позицию и вращение, сохраненные HandManager.
        transform.localPosition = handPosition;
        transform.localRotation = handRotation;
    }

    /// <summary>
    /// Устанавливает ЛОКАЛЬНУЮ целевую позицию, предоставленную HandManager.
    /// </summary>
    public void SetHandPosition(Vector3 pos)
    {
        handPosition = pos; // Сохраняем новую локальную позицию

        if (currentSlot == null)
        {
            // Применяем ЛОКАЛЬНУЮ позицию при нахождении в руке
            transform.localPosition = pos;
        }
    }

    // --- ЛОГИКА БОЯ ---

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        CheckDeath();
    }

    public void CheckDeath()
    {
        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
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