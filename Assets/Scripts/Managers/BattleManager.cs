using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// Этот класс управляет основными правилами игры, ресурсами и фазами.
public class BattleManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static BattleManager Instance;

    // Перечисление для обозначения игроков
    public enum PlayerType { Player, Enemy }

    // --- FEAR POINTS (FP) ---
    [Header("Очки Страха (FP)")]
    // Максимальный предел FP, который никогда не будет превышен
    public const int FP_CAP = 10;

    [Tooltip("Максимальное количество FP, доступное в текущем раунде.")]
    public int maxFearPoints; // Меняется каждый раунд

    private int currentFearPoints;

    // --- ИГРОВОЙ ЦИКЛ ---
    [Header("Игровой Цикл")]
    [Tooltip("Текущий номер раунда (0 - до начала игры, 1 - первый раунд, и т.д.)")]
    public int currentRound = 0; // Инициализируем с 0

    // --- UI REFERENCES ---
    [Header("Ссылки UI")]
    public TextMeshProUGUI fearPointsText;

    // --- Списки карт на поле ---
    private List<CardController> playerFieldCards = new List<CardController>();
    private List<CardController> enemyFieldCards = new List<CardController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start Game должен вызываться либо тут, либо из GameManager для запуска первого раунда.
        // Мы полагаемся на то, что GameManager.StartGame() вызовет BattleManager.StartGame().
        // Если нет внешнего вызова, раскомментируйте: StartGame();
    }

    // --- МЕТОДЫ УПРАВЛЕНИЯ FP ---

    public bool TrySpendFearPoints(PlayerType player, int cost)
    {
        if (player == PlayerType.Player)
        {
            if (currentFearPoints >= cost)
            {
                currentFearPoints -= cost;
                UpdateFearPointsUI();
                return true;
            }
            Debug.Log($"[BM] Недостаточно FP. Нужно: {cost}, доступно: {currentFearPoints}");
            return false;
        }
        return true;
    }

    public void UpdateFearPointsUI()
    {
        if (fearPointsText != null)
        {
            fearPointsText.text = $"FP: {currentFearPoints}/{maxFearPoints}";
        }
        else
        {
            Debug.LogError("[BM] Поле fearPointsText не назначено в Инспекторе!");
        }
    }

    // --- МЕТОДЫ ИГРОВОГО ЦИКЛА ---

    /// <summary>
    /// Инициализирует игру. Вызывается GameManager (или Start()).
    /// </summary>
    public void StartGame()
    {
        // Устанавливает состояние, чтобы первый EndTurn перевел нас в Раунд 1 с 1 FP.
        currentRound = 0;
        maxFearPoints = 0;
        currentFearPoints = 0;

        // Первый EndTurn фактически инициирует первый ход, когда игрок готов.
        EndTurn();

        Debug.Log("[BM] Игра запущена! Подготовка завершена.");
    }

    /// <summary>
    /// Вызывается кнопкой "End Turn". Завершает текущий ход игрока и начинает новый.
    /// </summary>
    public void EndTurn()
    {
        Debug.Log("--- Игрок завершает ход. Запуск фазы боя... ---");

        // 1. Вызов фазы боя
        ExecuteCombatPhase();

        // 2. Инкремент раунда
        currentRound++;

        // 3. Расчет нового максимального FP
        // FP = min(Текущий Раунд, Максимальный Лимит 10)
        maxFearPoints = Mathf.Min(currentRound, FP_CAP);

        // 4. Восстановление FP
        currentFearPoints = maxFearPoints;

        // 5. Обновление UI
        UpdateFearPointsUI();

        // 6. Добор карты
        if (HandManager.Instance != null)
        {
            HandManager.Instance.DrawCard();
        }

        Debug.Log($"--- Начат Раунд {currentRound}. Доступно FP: {maxFearPoints}. ---");
    }

    private void ExecuteCombatPhase()
    {
        // Здесь будет логика атаки карт.
        Debug.Log("--- ФАЗА БОЯ ЗАВЕРШЕНА. ---");
    }

    // --- МЕТОДЫ УПРАВЛЕНИЯ КАРТАМИ НА ПОЛЕ ---

    public void RegisterCardDeployment(CardController card, PlayerType player)
    {
        if (player == PlayerType.Player)
        {
            playerFieldCards.Add(card);
        }
        else
        {
            enemyFieldCards.Add(card);
        }
        Debug.Log($"Карта {card.data.displayName} выставлена на поле игроком {player}.");
    }

    public void DeregisterCard(CardController card)
    {
        if (playerFieldCards.Contains(card))
        {
            playerFieldCards.Remove(card);
            // ... логика уничтожения объекта
        }
        else if (enemyFieldCards.Contains(card))
        {
            enemyFieldCards.Remove(card);
            // ... логика уничтожения объекта
        }
    }
}