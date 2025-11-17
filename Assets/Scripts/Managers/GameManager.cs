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
    public HandManager handManager; // Строка 79 (была HandManager не назначен)

    [Tooltip("Ссылка на BattleManager. Должна быть назначена!")]
    public BattleManager battleManager; // Строка 84 (была BattleManager не назначен)

    // Вы можете добавить другие менеджеры, например:
    // public CardManager cardManager; 
    // public UIManager uiManager;

    private void Awake()
    {
        // Установка Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Если вы хотите, чтобы менеджер существовал между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start() // Строка 38
    {
        Debug.Log("GameManager: Запуск игры...");
        InitializeGame();
    }

    /// <summary>
    /// Инициализирует все системы игры.
    /// </summary>
    public void InitializeGame() // Строка 51
    {
        // 1. Проверяем, назначены ли все менеджеры в Инспекторе
        if (!VerifyManagers())
        {
            Debug.LogError("GameManager: Не удалось запустить игру из-за отсутствующих ссылок.");
            return;
        }

        Debug.Log("GameManager: Все менеджеры назначены. Запускаем инициализацию...");

        // 2. Инициализация HandManager (Раздача стартовой руки)
        if (handManager != null)
        {
            // Здесь мы вызываем DrawStartingHand(), который запускает создание карт.
            handManager.DrawStartingHand(); // Строка 59
            Debug.Log("GameManager: Стартовая рука успешно сформирована.");
        }

        // 3. Инициализация BattleManager (Начало первого раунда, ресурсов и т.д.)
        if (battleManager != null)
        {
            battleManager.StartGame();
            Debug.Log("GameManager: Боевая система инициализирована.");
        }

        // 4. Загрузка/инициализация UI (если есть UIManager)
        // ...

        Debug.Log("GameManager: Игра успешно запущена!");
    }

    /// <summary>
    /// Проверяет, что все необходимые ссылки на менеджеры назначены.
    /// </summary>
    private bool VerifyManagers() // Строка 79
    {
        bool success = true;

        if (handManager == null) // Строка 80
        {
            Debug.LogError("GameManager: HandManager не назначен в Инспекторе!");
            success = false;
        }

        if (battleManager == null) // Строка 84
        {
            Debug.LogError("GameManager: BattleManager не назначен в Инспекторе!");
            success = false;
        }

        // Добавьте проверки для других менеджеров здесь...

        return success;
    }

    // --- Дополнительные методы управления состоянием игры (пауза, завершение) ---
}