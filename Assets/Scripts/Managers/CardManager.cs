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
    public List<CardData> cardList; // <--- Список мастер-данных

    // --- Колода ---
    private List<CardData> deck = new List<CardData>(); // <--- Ваша переменная для колоды

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
        // При старте игры вызывается BuildDeck()
        BuildDeck();
    }

    // ПЕРЕИМЕНОВАНИЕ: BuildDeck теперь будет использоваться только для первичной сборки
    // При рестарте игры мы будем использовать ResetDeckAndShuffle()
    //
    /// <summary>
    /// Собирает колоду из списка доступных карт и перемешивает ее.
    /// </summary>
    public void BuildDeck()
    {
        // ВАЖНО: При первой сборке мы используем CardData напрямую
        // При рестарте мы используем ResetDeckAndShuffle()
        deck.Clear();

        if (cardList == null || cardList.Count == 0)
        {
            Debug.LogError("[CM] Список cardList пуст! Назначьте CardData в Инспекторе.");
            return;
        }

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

    /// <summary>
    /// Сбрасывает текущую колоду и собирает ее заново из мастер-списка.
    /// Этот метод используется для RestartGame().
    /// </summary>
    public void ResetDeckAndShuffle()
    {
        // 1. Очищаем текущую колоду
        deck.Clear(); // <--- ИСПОЛЬЗУЕМ ВАШЕ ИМЯ ПЕРЕМЕННОЙ: 'deck'

        if (cardList == null || cardList.Count == 0)
        {
            Debug.LogError("[CM] Список cardList пуст! Невозможно сбросить колоду.");
            return;
        }

        // 2. Копируем мастер-список (cardList) обратно в колоду
        // (Мы не Instatiate ScriptableObject здесь, чтобы не плодить ассеты.
        // Вместо этого мы полагаемся на то, что CardController сбросит здоровье.)
        foreach (CardData cardMaster in cardList) // <--- ИСПОЛЬЗУЕМ ВАШЕ ИМЯ: 'cardList'
        {
            // Например: добавляем каждую карту дважды
            deck.Add(cardMaster);
            deck.Add(cardMaster);
        }

        // 3. Перемешиваем колоду
        ShuffleDeck();

        Debug.Log($"[CM] Колода успешно сброшена и перемешана. Карт в колоде: {deck.Count}");
    }
}