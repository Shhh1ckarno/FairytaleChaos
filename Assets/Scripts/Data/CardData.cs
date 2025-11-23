using UnityEngine;

// 1. Enum CardClass (для инициативы и уязвимости)
public enum CardClass
{
    // Порядок в Enum определяет порядок инициативы в бою (от высшего к низшему приоритету)
    Elemental = 0, // Стихийный
    Glass = 1,     // Стеклянный
    Wood = 2,      // Деревянный
    Mechanical = 3,// Механика
    Plush = 4      // Плюшевый = Lowest Priority
}

// 2. Enum AttackPattern (должен находиться в отдельном файле, но для целостности привожу его здесь)
/*
public enum AttackPattern
{
    StandardFront = 0,      // Винни-Пух
    FlexibleFront = 1,      // Буратино
    TargetThroughLine = 2,  // Мальвина
    SupportOnly = 99        // Кальцифер
}
*/
// Предполагается, что AttackPattern находится в своем файле и доступен.

[CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    // --- Игровые Механики ---
    [Header("Игровые Механики")]
    [Tooltip("Класс карты, определяет инициативу и уязвимость.")]
    public CardClass cardClass; // Используем enum CardClass

    // НОВОЕ: Определение поведения карты в фазе боя (перемещение, атака, поддержка)
    [Tooltip("Определяет, как карта выбирает цель, двигается и оказывает поддержку.")]
    public AttackPattern attackPattern;


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

    // --- Параметры Поддержки (Опционально для Кальцифера) ---
    [Header("Параметры Поддержки (если SupportOnly)")]
    [Tooltip("Модификатор лечения, если карта является саппортом (Heal +X).")]
    public int supportHealAmount = 0;
}