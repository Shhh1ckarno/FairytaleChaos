using UnityEngine;

// GameManager — центральный Singleton, который управляет общим состоянием игры,
// фазами и координирует работу других менеджеров.
public class GameManager : MonoBehaviour
{
    // *** ПАТТЕРН SINGLETON ***
    public static GameManager Instance;

    // --- ССЫЛКИ НА МЕНЕДЖЕРЫ (Назначаются в Инспекторе) ---
    [Header("Менеджеры")]
    [Tooltip("Ссылка на HandManager. Должна быть назначена!")]
    public HandManager handManager;

    [Tooltip("Ссылка на BattleManager. Должна быть назначена!")]
    public BattleManager battleManager;

    [Tooltip("Ссылка на CardManager. Должна быть назначена!")]
    public CardManager cardManager;
    public HealthManager healthManager;
    private void Awake()
    {
        // Установка Singleton
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Используйте, если нужно
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("GameManager: Запуск игры...");
        InitializeGame();
    }

    /// <summary>
    /// Инициализирует все системы игры.
    /// </summary>
    public void InitializeGame()
    {
        // 1. Проверяем, назначены ли все менеджеры в Инспекторе
        if (!VerifyManagers())
        {
            Debug.LogError("GameManager: Не удалось запустить игру из-за отсутствующих ссылок.");
            return;
        }

        Debug.Log("GameManager: Все менеджеры назначены. Запускаем инициализацию...");

        // 2. Инициализация HandManager (БЕЗ ДОБОРА!)
        // Добор стартовой руки теперь полностью обрабатывается BattleManager.StartGame().
        // *********************************************************************************
        // !!! ИСПРАВЛЕНИЕ: УДАЛЕН ВЫЗОВ handManager.DrawStartingHand() !!!
        // *********************************************************************************
        if (handManager != null)
        {
            // Здесь можно выполнить базовую настройку HandManager, но не добор карт.
            // Debug.Log("GameManager: HandManager готов.");
        }


        // 3. Инициализация BattleManager (Начало первого раунда, ресурсов и т.д.)
        if (battleManager != null)
        {
            // BattleManager.StartGame() теперь отвечает за:
            // 1. Сброс здоровья
            // 2. Вызов HandManager.DrawStartingHand()
            // 3. Начало первого хода
            battleManager.StartGame();
            Debug.Log("GameManager: Боевая система инициализирована и игра началась.");
        }

        // 4. Загрузка/инициализация UI (если есть UIManager)
        // ...

        Debug.Log("GameManager: Игра успешно запущена!");
    }

    /// <summary>
    /// Проверяет, что все необходимые ссылки на менеджеры назначены.
    /// </summary>
    private bool VerifyManagers()
    {
        bool success = true;

        if (handManager == null)
        {
            Debug.LogError("GameManager: HandManager не назначен в Инспекторе!");
            success = false;
        }

        if (battleManager == null)
        {
            Debug.LogError("GameManager: BattleManager не назначен в Инспекторе!");
            success = false;
        }

        return success;
    }

    // --- Дополнительные методы управления состоянием игры (пауза, завершение) ---
    public void RestartGame()
    {
        Debug.Log("---  ЗАПУСК РЕСТАРТА ИГРЫ ---");

        // 1. Сброс состояния поля и карт в руке
        if (battleManager != null)
        {
            battleManager.ClearFieldAndHand(); // Очистка поля, руки и сброс ресурсов BattleManager
        }

        // 2. Сброс здоровья
        if (healthManager != null)
        {
            healthManager.InitializeHealth();
        }

        // 3. Сброс колоды (самый важный шаг)
        if (cardManager != null)
        {
            cardManager.ResetDeckAndShuffle();
        }
        else
        {
            Debug.LogError("CardManager не назначен! Невозможно сбросить колоду.");
            return;
        }

        // 4. Перезапуск игры
        InitializeGame(); // Вызовет BattleManager.StartGame()

        Debug.Log("---  РЕСТАРТ ЗАВЕРШЕН ---");
    }
}