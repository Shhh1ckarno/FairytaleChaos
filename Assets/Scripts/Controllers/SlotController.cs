using UnityEngine;

// Этот класс контролирует отдельную ячейку на поле боя.
public class SlotController : MonoBehaviour
{
    // --- ДАННЫЕ СЛОТА ---

    [Tooltip("Принадлежит ли этот слот игроку-противнику.")]
    public bool isEnemySlot = false;

    // Ссылка на слот противника, находящийся напротив (для механики боя)
    [Tooltip("Слот, который находится напротив этого слота.")]
    public SlotController opponentSlot;

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
    /// Вызывается из CardController после успешного розыгрыша.
    /// </summary>
    public void SetOccupant(CardController card)
    {
        occupant = card;

        // 1. Устанавливаем слот как родительский объект карты
        // Это необходимо, чтобы localPosition и localRotation работали корректно.
        card.transform.SetParent(this.transform);

        // 2. Сброс локальной позиции
        // Позиция (0, 0, 0) в локальном пространстве слота - это его центр.
        card.transform.localPosition = Vector3.zero;

        // 3. Сброс локального вращения
        // Quaternion.identity (0, 0, 0) сбрасывает вращение, заставляя карту лечь ровно
        // (лицом вверх, параллельно слоту), независимо от ее вращения в руке.
        card.transform.localRotation = Quaternion.identity;

        // ИЛИ: Если вам нужно, чтобы карты на поле имели фиксированное вращение (например, 180 градусов):
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