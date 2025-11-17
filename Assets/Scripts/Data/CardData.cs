using UnityEngine;

// 1. ПЕРЕМЕЩЕН АТРИБУТ: Атрибут убран отсюда, так как это enum.
public enum CardClass
{
    // Порядок в Enum определяет порядок инициативы в бою (от высшего к низшему приоритету)
    // 0. Стихийные = Highest Priority
    Elemental = 0, // Стихийный
    Glass = 1,     // Стеклянный
    Wood = 2,      // Деревянный
    Mechanical = 3,// Механика
    Plush = 4      // Плюшевый = Lowest Priority
}

// 2. ПРАВИЛЬНОЕ РАСПОЛОЖЕНИЕ: Атрибут [CreateAssetMenu] должен быть над классом ScriptableObject.
[CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    // --- Игровые Механики ---
    [Header("Игровые Механики")]
    [Tooltip("Класс карты, определяет инициативу и уязвимость.")]
    public CardClass cardClass; // Используем enum CardClass

    // --- ИДЕНТИФИКАЦИЯ И ВИЗУАЛ ---
    [Header("Идентификация и Визуал")]
    [Tooltip("Уникальный идентификатор карты")]
    public int id;

    [Tooltip("Название, отображаемое на карте")]
    public string displayName;

    [Tooltip("Описание или лор карты")]
    [Multiline(4)]
    public string description;

    [Tooltip("Изображение или спрайт карты")]
    public Sprite artwork;

    [Tooltip("Префаб GameObject, который должен быть создан для этой карты.")]
    public GameObject prefab;

    // --- СТАТИСТИКИ ---

    [Header("Статистики")]
    [Tooltip("Стоимость карты в Очках Страха (Fear Points - FP)")]
    public int costFear;

    [Tooltip("Сила атаки существа")]
    public int attack;

    [Tooltip("Максимальное здоровье существа")]
    public int maxHP;
}