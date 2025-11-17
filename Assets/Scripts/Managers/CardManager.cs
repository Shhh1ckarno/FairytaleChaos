using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [Header("Catalog: place unique CardData objects here")]
    public List<CardData> cardCatalog = new List<CardData>(); // заполни в инспекторе 5 SO

    private List<CardData> deck = new List<CardData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Собираем колоду: 3 копии каждой карточки
    public void BuildDeck()
    {
        deck.Clear();
        foreach (var cd in cardCatalog)
        {
            if (cd == null) continue;
            for (int i = 0; i < 3; i++)
                deck.Add(cd);
        }
        ShuffleDeck();
        Debug.Log($"Deck built: {deck.Count} cards.");
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int j = Random.Range(i, deck.Count);
            var tmp = deck[j];
            deck[j] = deck[i];
            deck[i] = tmp;
        }
    }

    // Возвращает верхнюю карту (и убирает её из колоды)
    public CardData DrawTop()
    {
        if (deck.Count == 0) return null;
        CardData top = deck[0];
        deck.RemoveAt(0);
        return top;
    }

    public int CardsRemaining()
    {
        return deck.Count;
    }

    // Для отладки
    [ContextMenu("PrintDeck")]
    public void PrintDeck()
    {
        string s = "Deck: ";
        foreach (var c in deck) s += (c != null ? c.id + "," : "null,");
        Debug.Log(s);
    }
}
