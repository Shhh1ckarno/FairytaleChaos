using UnityEngine;

// Этот скрипт должен быть прикреплен к объектам, представляющим игрока и противника
public class PlayerController : MonoBehaviour
{
    // --- ОСНОВНЫЕ СТАТИСТИКИ ---
    [Header("Статистики")]
    [Tooltip("Текущее здоровье портрета игрока/противника")]
    public int health = 30;
    [Tooltip("Максимальное количество FP, которое может быть у игрока.")]
    public int maxFearPoints = 1; // Будет обновляться по раундам

    // --- УПРАВЛЕНИЕ РЕСУРСАМИ И КАРТАМИ ---
    [Header("Ресурсы")]
    public int currentMana = 0; // Если вы используете ману
    public int maxMana = 1;

    // Ссылки на менеджеры
    private HandManager handManager;
    private BattleManager battleManager;

    private void Start()
    {
        // Поиск менеджеров в сцене
        handManager = HandManager.Instance;
        battleManager = BattleManager.Instance;

        // Начальная инициализация
        Initialize();
    }

    public void Initialize()
    {
        // Инициализация при старте игры
        Debug.Log($"{gameObject.name} инициализирован. HP: {health}");
        // Здесь можно добавить обновление UI портрета
    }

    // --- МЕТОДЫ, ВЫЗЫВАЕМЫЕ BATTLEMANAGER ---

    /// <summary>
    /// Восстанавливает ресурсы (мана или FP) в начале хода.
    /// </summary>
    public void RefillMana()
    {
        // Логика для системы, основанной на Мане:
        // maxMana = Mathf.Min(maxMana + 1, 10); // Увеличиваем макс. ману
        // currentMana = maxMana; // Восстанавливаем ману

        // Логика для системы, основанной на FP (управляется BattleManager):
        // BattleManager уже восстанавливает FP в StartRound().

        Debug.Log($"{gameObject.name}: Ресурсы восстановлены.");
    }

    /// <summary>
    /// Вызывает HandManager для взятия новой карты.
    /// </summary>
    public void DrawCard()
    {
        if (handManager != null)
        {
            // Здесь должна быть логика, чтобы различать, кто берет карту:
            // if (gameObject.name == "Player") handManager.DrawCardForPlayer();
            // else handManager.DrawCardForEnemy();

            // Пока что просто вызываем DrawCard
            handManager.DrawCard();
        }
        Debug.Log($"{gameObject.name} взял карту.");
    }

    // --- МЕТОДЫ ФАЗЫ БОЯ ---

    /// <summary>
    /// Принимает прямой урон от существа, которое атакует портрет.
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;

        Debug.Log($"{gameObject.name} получил {damage} прямого урона. Осталось HP: {health}");

        // Здесь необходимо обновить UI портрета игрока/противника
        // PlayerUIDisplay.Instance.UpdateHealth(health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} побежден!");

        // Здесь должна быть логика, которая сообщает BattleManager о конце игры
        // battleManager.EndGame(this); 
    }
}