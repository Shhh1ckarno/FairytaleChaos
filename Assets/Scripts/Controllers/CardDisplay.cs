using UnityEngine;
using TMPro;
using UnityEngine.UI; // Для работы с Image, если используете UI

// CardDisplay отвечает только за визуальное представление карты и ее данных.
// Он НЕ ДОЛЖЕН хранить CardData или текущее HP.
public class CardDisplay : MonoBehaviour
{
    // --- Ссылки UI, которые нужно перетащить в Инспекторе ---

    [Header("Базовые данные")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public SpriteRenderer artworkImage; // Используйте Image, если это UI Canvas

    [Header("Статистики")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI costText;

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

        if (nameText != null)
            nameText.text = data.displayName;

        // *** Устранение ошибки CS1061: Требует наличия поля 'description' в CardData.cs ***
        if (descriptionText != null)
            descriptionText.text = data.description;

        // *** Устранение ошибки CS1061: Требует наличия поля 'artwork' в CardData.cs ***
        if (artworkImage != null)
            artworkImage.sprite = data.artwork;

        // Инициализация статичных чисел
        if (attackText != null)
            attackText.text = data.attack.ToString();

        if (costText != null)
            costText.text = data.costFear.ToString();

        // Инициализация HP (используем maxHP для начала)
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

    // Удаляем все поля Start(), currentAttack, currentHP, и SetStats(), 
    // так как они дублируют логику CardController.
}