using UnityEngine;
using TMPro; // Для работы с трехмерным текстом (TextMeshPro)
using UnityEngine.UI; // Оставлено на случай, если background или artwork являются UI-изображениями

// CardDisplay отвечает только за визуальное представление карты и ее данных.
public class CardDisplay : MonoBehaviour
{
    // --- Ссылки UI, которые нужно перетащить в Инспекторе ---

    [Header("Базовые данные")]
    // КРИТИЧНО: Используем TextMeshPro, если текст трехмерный в мировом пространстве
    public TextMeshPro nameText;
    public TextMeshPro descriptionText;

    // Artwork может быть как SpriteRenderer (для 3D), так и Image (для UI Canvas)
    public SpriteRenderer artworkImage;

    [Header("Статистики")]
    // КРИТИЧНО: Используем TextMeshPro
    public TextMeshPro attackText;
    public TextMeshPro hpText;
    public TextMeshPro costText;

    // --- МЕТОДЫ ОТОБРАЖЕНИЯ ---

    /// <summary>
    /// Инициализирует все визуальные элементы карты (Вызывается HandManager).
    /// </summary>
    public void DisplayCardData(CardData data)
    {
        if (data == null)
        {
            Debug.LogError("Попытка отобразить CardData, которая равна null.");
            return;
        }

        // --- Базовые данные ---
        if (nameText != null)
            nameText.text = data.displayName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (artworkImage != null)
            artworkImage.sprite = data.artwork;

        // --- Статистики ---
        // Инициализация статичных чисел (Attack и Cost)
        if (attackText != null)
            attackText.text = data.attack.ToString();

        if (costText != null)
            costText.text = data.costFear.ToString();

        // Инициализация HP (используем maxHP для начала)
        // CardController позаботится о том, чтобы вызвать это после инициализации HP.
        // Здесь мы можем использовать maxHP, чтобы гарантировать отображение числа.
        UpdateHPText(data.maxHP);
    }

    /// <summary>
    /// Динамически обновляет отображаемый текст здоровья карты (Вызывается CardController при получении урона).
    /// </summary>
    public void UpdateHPText(int newHP)
    {
        if (hpText != null)
        {
            hpText.text = newHP.ToString();
        }
    }
}