using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Предполагается, что CardData - это ScriptableObject
// public class CardData : ScriptableObject { public string displayName; public GameObject prefab; /* ... */ }

public class CardManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static CardManager Instance;

    [Header("Данные и Настройки")]
    [Tooltip("Список всех доступных ScriptableObject CardData для построения колоды.")]
    public List<CardData> cardList;

    // --- Колода ---
    private List<CardData> deck = new List<CardData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Если нужно сохранять между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BuildDeck();
    }

    /// <summary>
    /// Собирает колоду из списка доступных карт и перемешивает ее.
    /// </summary>
    public void BuildDeck()
    {
        if (cardList == null || cardList.Count == 0)
        {
            Debug.LogError("[CM] Список cardList пуст! Назначьте CardData в Инспекторе.");
            return;
        }

        deck.Clear();

        // ВАЖНО: Здесь вы определяете состав колоды.
        foreach (CardData cardData in cardList)
        {
            // Например: добавляем каждую карту дважды
            deck.Add(cardData);
            deck.Add(cardData);
        }

        ShuffleDeck();
        Debug.Log($"[CM] Колода построена и перемешана. Карт в колоде: {deck.Count}");
    }

    /// <summary>
    /// Выдает верхнюю карту из колоды.
    /// </summary>
    public CardData DrawTop()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("[CM] Колода пуста! Больше карт для добора нет.");
            return null;
        }

        CardData drawnCard = deck[0];
        deck.RemoveAt(0);
        return drawnCard;
    }

    /// <summary>
    /// Перемешивает колоду (простой алгоритм Фишера-Йетса).
    /// </summary>
    public void ShuffleDeck()
    {
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            CardData value = deck[k];
            deck[k] = deck[n];
            deck[n] = value;
        }
    }
}