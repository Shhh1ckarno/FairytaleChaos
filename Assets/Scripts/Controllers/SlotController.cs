using UnityEngine;

// Этот класс контролирует отдельную ячейку на поле боя.
public class SlotController : MonoBehaviour
{
    // --- ДАННЫЕ СЛОТА ---

    [Header("Базовые Свойства")]
    [Tooltip("Принадлежит ли этот слот игроку-противнику.")]
    public bool isEnemySlot = false;

    // Ссылка на слот противника, находящийся напротив (для механики стандартного боя)
    [Tooltip("Слот, который находится напротив этого слота (на другой стороне поля).")]
    public SlotController opponentSlot;

    [Header("Свойства Линии и Перемещение")]
    [Tooltip("Является ли этот слот передней линией? (false = задняя линия)")]
    public bool isFrontLine = true;

    // НОВОЕ ПОЛЕ: Критично для перемещения и поддержки (Кальцифер)
    [Tooltip("Ссылка на слот передней линии в нашей дорожке (только если это задний слот).")]
    public SlotController forwardSlot;

    // Ссылка на карту, которая сейчас занимает этот слот
    private CardController occupant;

    // --- УПРАВЛЕНИЕ СОСТОЯНИЕМ ---

    /// <summary>
    /// Проверяет, занят ли слот картой.
    /// </summary>
    public bool IsOccupied()
    {
        return occupant != null;
    }

    /// <summary>
    /// Устанавливает карту в этот слот и позиционирует ее.
    /// Вызывается из CardController после успешного розыгрыша И в фазе Перемещения.
    /// </summary>
    public void SetOccupant(CardController card)
    {
        occupant = card;

        // Обновляем текущий слот в самой карте
        card.currentSlot = this;

        // 1. Устанавливаем слот как родительский объект карты
        card.transform.SetParent(this.transform);

        // 2. Сброс локальной позиции
        // Позиция (0, 0, 0) в локальном пространстве слота - это его центр.
        card.transform.localPosition = Vector3.zero;

        // 3. Установка локального вращения (коррекция для поля)
        // Вы можете настроить это значение, чтобы карта смотрела правильно.
        // Quaternion.Euler(0, 90, 0) - это пример для вертикально расположенной карты.
        card.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }

    /// <summary>
    /// Удаляет карту из слота.
    /// </summary>
    public void ClearOccupant()
    {
        occupant = null;
    }

    /// <summary>
    /// Получает карту, занимающую этот слот.
    /// </summary>
    public CardController GetOccupant()
    {
        return occupant;
    }
}