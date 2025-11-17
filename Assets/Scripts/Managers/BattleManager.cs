using System.Collections.Generic;
using System.Linq; // Необходим для LINQ в GetCombatOrder
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // *** ПАТТЕРН SINGLETON ***
    public static BattleManager Instance;

    // Enum для определения, чей ход / кому принадлежит ресурс
    public enum PlayerType { Player, Enemy };

    // --- Ресурсы ---
    [Header("Очки Страха (FP)")]
    [Tooltip("Текущее количество Очков Страха у игрока")]
    public int playerFearPoints;
    [Tooltip("Текущее количество Очков Страха у врага")]
    public int enemyFearPoints;

    // --- Раунды и Инициатива ---
    [Header("Раунды и Инициатива")]
    [Tooltip("Текущий номер раунда")]
    public int currentRound = 0;
    [Tooltip("Игрок, который ходит первым в этом раунде")]
    public PlayerType initiativeHolder;

    // *** НОВОЕ ПОЛЕ: СПИСОК УЧАСТНИКОВ БОЯ ***
    // Содержит все выставленные на поле карты (для сортировки в CombatPhase)
    private List<CardController> deployedCards = new List<CardController>();

    private void Awake()
    {
        // Установка Singleton
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
        // Предполагается, что GameManager вызывает StartGame()
        // StartGame(); 
    }

    public void StartGame()
    {
        // Инициализация FP: В начале игры игрок получает 1 ОС.
        playerFearPoints = 1;
        enemyFearPoints = 1;

        // Обновление UI FP (если есть)
        // if (FPDisplay.Instance != null)
        //     FPDisplay.Instance.UpdatePlayerFP(playerFearPoints);

        StartRound();
    }

    // Запускает новый раунд, обновляя FP и Инициативу
    public void StartRound()
    {
        currentRound++;

        // Механика FP: В начале каждого нового раунда FP увеличивается на 1.
        int maxFearPoints = currentRound;

        playerFearPoints = maxFearPoints;
        enemyFearPoints = maxFearPoints;

        // Обновление UI FP
        // if (FPDisplay.Instance != null)
        //     FPDisplay.Instance.UpdatePlayerFP(playerFearPoints);

        Debug.Log($"--- Начат Раунд {currentRound} ---");
        Debug.Log($"FP игрока/врага восстановлено до: {playerFearPoints}");

        DetermineInitiative();

        // Здесь можно добавить вызов HandManager.Instance.DrawCard()
        // PlayerController.Instance.DrawCard();
    }

    private void DetermineInitiative()
    {
        // Механика Инициативы: Чередование ходов
        if (currentRound % 2 != 0) // Нечетные раунды
        {
            initiativeHolder = PlayerType.Player;
        }
        else // Четные раунды
        {
            initiativeHolder = PlayerType.Enemy;
        }

        Debug.Log($"Инициатива (ходит первым) в Раунде {currentRound} принадлежит: {initiativeHolder}");
    }

    // --- УПРАВЛЕНИЕ РЕСУРСАМИ (FP) ---

    // Вызывается из CardController для проверки и расхода FP
    public bool TrySpendFearPoints(PlayerType player, int cost)
    {
        int currentFP = (player == PlayerType.Player) ? playerFearPoints : enemyFearPoints;

        if (currentFP >= cost)
        {
            if (player == PlayerType.Player)
            {
                playerFearPoints -= cost;
                // Обновление UI FP
                // if (FPDisplay.Instance != null)
                //     FPDisplay.Instance.UpdatePlayerFP(playerFearPoints);
                Debug.Log($"Игрок потратил {cost} ОС. Осталось: {playerFearPoints}");
            }
            else
            {
                enemyFearPoints -= cost;
            }
            return true;
        }
        return false;
    }

    // --- УПРАВЛЕНИЕ КАРТАМИ НА ПОЛЕ (РЕШЕНИЕ ОШИБКИ CS1061) ---

    /// <summary>
    /// Регистрирует карту, выставленную на поле, для участия в фазе боя.
    /// Вызывается из CardController.OnEndDrag после успешного розыгрыша.
    /// </summary>
    public void RegisterCardDeployment(CardController card, PlayerType player)
    {
        if (!deployedCards.Contains(card))
        {
            deployedCards.Add(card);
            // Если вы не передаете player, то вам нужно определить его через slot.isEnemySlot
            Debug.Log($"Карта {card.data.displayName} зарегистрирована для боя.");
        }
    }

    /// <summary>
    /// Удаляет карту из списка при ее уничтожении. Вызывается CardController.Die().
    /// </summary>
    public void DeregisterCard(CardController card)
    {
        if (deployedCards.Remove(card))
        {
            Debug.Log($"Карта {card.data.displayName} удалена из списка боя.");
        }
    }

    // --- ФАЗА БОЯ ---

    /// <summary>
    /// Сортирует карты по инициативе класса, затем по инициативе раунда.
    /// </summary>
    private List<CardController> GetCombatOrder()
    {
        var combatants = deployedCards
            .Where(c => c != null && c.currentHP > 0)
            .ToList();

        // Сортировка по ТЗ:
        // 1. По Инициативе Класса (CardClass Enum value)
        // 2. По Инициативе Раунда (initiativeHolder)
        var sortedOrder = combatants
            .OrderBy(c => c.data.cardClass) // Сортировка по CardClass (0 - highest, 4 - lowest)
            .ThenBy(c => GetPlayerType(c) == initiativeHolder ? 0 : 1) // Tie-breaker: ходивший первым атакует первым
            .ToList();

        return sortedOrder;
    }

    // Вспомогательный метод для определения игрока по карте
    private PlayerType GetPlayerType(CardController card)
    {
        // Предполагаем, что SlotController знает, чья это сторона
        return card.currentSlot != null && card.currentSlot.isEnemySlot ? PlayerType.Enemy : PlayerType.Player;
    }

    // *** ФУНКЦИЯ-ОБЕРТКА ДЛЯ КНОПКИ UI ***
    public void EndTurnPlayer()
    {
        EndTurn(PlayerType.Player);
    }

    public void EndTurn(PlayerType player)
    {
        Debug.Log($"{player} закончил свою фазу розыгрыша.");

        // После того, как оба игрока закончили выставление:
        // (Для MVP мы сразу переходим к бою после хода игрока)
        StartCombatPhase();
    }

    public void StartCombatPhase()
    {
        Debug.Log("--- ФАЗА БОЯ (ЗАПУЩЕНА) ---");

        List<CardController> order = GetCombatOrder();

        foreach (var attacker in order)
        {
            // --- СЮДА ПОЙДЕТ ЛОГИКА PROCESSATTACK(attacker) ---
            Debug.Log($"Атакует: {attacker.data.displayName} ({attacker.data.cardClass})");
        }

        // Очистка мертвых карт и переход к следующему раунду
        deployedCards.RemoveAll(c => c.currentHP <= 0);

        StartRound();
    }

    // public void EndGame(PlayerController loser) { ... }
}