using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HandManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static HandManager Instance;

    // --- ССЫЛКИ НА МЕНЕДЖЕРЫ ---
    private CardManager cardManager;

    // --- Настройки руки ---
    [Header("Настройки Руки")]
    [Tooltip("Объект, который является родительским для карт в руке")]
    public Transform handRoot; // Назначьте объект-контейнер!

    [Tooltip("Начальная ЛОКАЛЬНАЯ позиция (смещение от центра HandRoot)")]
    public Vector3 startPosition = new Vector3(0f, 0, 0);

    [Tooltip("Расстояние между центрами карт")]
    public float cardSpacing = 1.5f;

    [Tooltip("Максимальное количество карт, которое может быть в руке")]
    public int maxHandSize = 10;

    [Tooltip("Количество карт для начальной раздачи")]
    public int startingHandSize = 5;

    // Список всех карт, которые в настоящее время находятся в руке
    private List<CardController> handCards = new List<CardController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (CardManager.Instance != null)
        {
            cardManager = CardManager.Instance;
        }

        if (handRoot == null)
        {
            handRoot = transform;
        }

        if (cardManager != null)
        {
            // !!! ИСПРАВЛЕНИЕ: УДАЛИТЬ ЭТОТ ВЫЗОВ !!!
            // DrawStartingHand();
        }
        else
        {
            Debug.LogError("[HM] CardManager не найден! Невозможно начать добор карт.");
        }
    }

    // ========================================================================================
    //                                ЛОГИКА ДОБОРА И УДАЛЕНИЯ
    // ========================================================================================

    public void DrawStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard();
        }
    }

    /// <summary>
    /// Добирает одну карту из колоды, создает ее и помещает в руку.
    /// </summary>
    public void DrawCard()
    {
        if (handCards.Count >= maxHandSize || cardManager == null) return;

        CardData cardData = cardManager.DrawTop();

        if (cardData == null) return;

        if (cardData.prefab == null)
        {
            Debug.LogError($"[HM] В CardData '{cardData.displayName}' не назначен префаб! Карту невозможно создать.");
            return;
        }

        // 1. СОЗДАНИЕ И УСТАНОВКА РОДИТЕЛЯ
        // Карта создается как дочерний объект handRoot (это критично для локальных позиций)
        GameObject newCardObject = Instantiate(cardData.prefab, handRoot);

        CardController cardController = newCardObject.GetComponent<CardController>();

        if (cardController != null)
        {
            // 2. ИНИЦИАЛИЗАЦИЯ И ДОБАВЛЕНИЕ
            cardController.Initialize(cardData);
            handCards.Add(cardController);

            // 3. ПЕРЕРАСЧЕТ ПОЗИЦИЙ
            UpdateHandPositions();
        }
        else
        {
            Debug.LogError($"[HM] Префаб карты {cardData.displayName} не содержит CardController!");
            Destroy(newCardObject);
        }
    }

    /// <summary>
    /// Удаляет карту из руки (вызывается CardController при розыгрыше на поле).
    /// </summary>
    public void RemoveFromHand(CardController card)
    {
        if (handCards.Remove(card))
        {
            UpdateHandPositions();
        }
    }

    /// <summary>
    /// Добавляет карту обратно в руку (используется CardController при неудачном Drag & Drop).
    /// </summary>
    public void AddCardBackToHand(CardController card)
    {
        if (!handCards.Contains(card))
        {
            handCards.Add(card);
            // UpdateHandPositions() будет вызван CardController в OnEndDrag, 
            // но мы можем вызвать его и здесь для страховки.
            // UpdateHandPositions(); 
        }
    }

    /// <summary>
    /// Проверяет, находится ли карта в списке (используется CardController).
    /// </summary>
    public bool HandContains(CardController card)
    {
        return handCards.Contains(card);
    }

    // ========================================================================================
    //                                ЛОГИКА ПОЗИЦИОНИРОВАНИЯ
    // ========================================================================================

    /// <summary>
    /// Пересчитывает и устанавливает ЛОКАЛЬНУЮ позицию каждой карты в руке.
    /// Считает, что карты расположены горизонтально по оси Z.
    /// </summary>
    public void UpdateHandPositions()
    {
        int count = handCards.Count;
        if (count == 0) return;

        // Определяем общую ширину руки
        float totalWidth = (count - 1) * cardSpacing;

        // Вычисляем начальную точку для первой карты, чтобы центрировать руку
        float currentOffset = startPosition.z - (totalWidth / 2f);

        for (int i = 0; i < count; i++)
        {
            CardController card = handCards[i];

            // Создаем целевую ЛОКАЛЬНУЮ позицию
            Vector3 targetPosition = new Vector3(
                startPosition.x,    // X: Постоянное
                startPosition.y,    // Y: Постоянное
                currentOffset       // Z: Горизонтальное смещение
            );

            // Вызываем CardController для установки позиции
            card.SetHandPosition(targetPosition);

            currentOffset += cardSpacing;
        }
    }
    /// <summary>
    /// Удаляет все карты из руки и уничтожает их GameObjects.
    /// </summary>
    public void ClearAllCards()
    {
        // 1. Уничтожаем все объекты карт в руке
        foreach (var card in handCards)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }

        // 2. Очищаем список
        handCards.Clear();

        Debug.Log("[HM] Рука очищена.");
    }
}