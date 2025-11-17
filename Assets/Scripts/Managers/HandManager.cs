using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Этот класс управляет картами в руке игрока, их расположением и количеством.
public class HandManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static HandManager Instance;

    // --- ССЫЛКИ НА МЕНЕДЖЕРЫ ---
    private CardManager cardManager;

    // --- Настройки руки ---
    [Header("Настройки Руки")]
    [Tooltip("Объект, который является родительским для карт в руке")]
    public Transform handRoot; // Назначается в Инспекторе

    // ВАЖНО: Это ЛОКАЛЬНОЕ смещение центральной точки
    [Tooltip("Начальная ЛОКАЛЬНАЯ позиция (смещение от центра HandRoot)")]
    public Vector3 startPosition = new Vector3(0f, 0, 0);

    [Tooltip("Расстояние между центрами карт")]
    public float cardSpacing = 1.5f;

    [Tooltip("Максимальное количество карт, которое может быть в руке")]
    public int maxHandSize = 10;

    [Tooltip("Количество карт для начальной раздачи")]
    public int startingHandSize = 4;

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
        // Получаем ссылку на CardManager
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
            cardManager.BuildDeck();
            DrawStartingHand();
        }
    }

    // --- МЕТОДЫ УПРАВЛЕНИЯ РУКОЙ (Оставлены без изменений) ---

    public void DrawStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (handCards.Count >= maxHandSize || cardManager == null) return;

        CardData cardData = cardManager.DrawTop();
        if (cardData == null || cardData.prefab == null) return;

        GameObject newCardObject = Instantiate(cardData.prefab, handRoot);
        CardController cardController = newCardObject.GetComponent<CardController>();
        CardDisplay cardDisplay = newCardObject.GetComponent<CardDisplay>();

        if (cardController != null)
        {
            cardController.data = cardData;
            if (cardDisplay != null) cardDisplay.DisplayCardData(cardData);

            handCards.Add(cardController);
            UpdateHandPositions();
        }
        else
        {
            Destroy(newCardObject);
        }
    }

    public void RemoveFromHand(CardController card)
    {
        if (handCards.Remove(card))
        {
            UpdateHandPositions();
        }
    }

    /// <summary>
    /// Пересчитывает и устанавливает ЛОКАЛЬНУЮ позицию каждой карты в руке.
    /// *** ИСПОЛЬЗУЕМ ОСЬ X ДЛЯ ГОРИЗОНТАЛЬНОГО РАСПОЛОЖЕНИЯ ***
    /// </summary>
    private void UpdateHandPositions()
    {
        int count = handCards.Count;
        if (count == 0) return;

        float totalWidth = (count - 1) * cardSpacing;

        // currentOffset: Смещение, которое мы будем применять к горизонтальной оси (X)
        float currentOffset = startPosition.x - (totalWidth / 2f);

        for (int i = 0; i < count; i++)
        {
            CardController card = handCards[i];

            // 1. ПРЕДПОЛАГАЕМ, что горизонтальное смещение должно быть по оси X
            // Это стандартное поведение для объектов, которые не повернуты.
            //Vector3 targetPosition = new Vector3(
            //    currentOffset,     // X: Горизонтальное смещение
            //    startPosition.y,   // Y: Вертикальное смещение (постоянное)
            //    startPosition.z    // Z: Глубина (постоянная)
            //);

            // Если карты все еще идут вглубь (по оси Z),
            // ЗАМЕНИТЕ:
            Vector3 targetPosition = new Vector3(
                startPosition.x,   // X: Постоянное
                startPosition.y,   // Y: Постоянное
                currentOffset      // Z: Горизонтальное смещение
            );
            

            card.SetHandPosition(targetPosition);

            currentOffset += cardSpacing;
        }
    }
}