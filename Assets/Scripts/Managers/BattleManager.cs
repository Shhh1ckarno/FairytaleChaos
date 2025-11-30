using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq; // Нужно для работы списков

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public enum PlayerType { Player, Enemy }

    [Header("Настройки")]
    public PlayerType currentTurnOwner; // Чей сейчас ход

    // --- FEAR POINTS (FP) ---
    [Header("Очки Страха (FP)")]
    public const int FP_CAP = 10;

    public int maxFearPoints;
    public int currentFearPoints; // FP Игрока
                                  // Для врага можно сделать отдельные переменные, но для простоты используем общую логику пока

    // --- ИГРОВОЙ ЦИКЛ ---
    [Header("Игровой Цикл")]
    public int currentRound = 0;

    // --- UI REFERENCES ---
    [Header("Ссылки UI")]
    public TextMeshProUGUI fearPointsText;
    public Button endTurnButton; // Ссылка на кнопку, чтобы отключать её в ход врага

    // --- Слоты Поля ---
    [Header("Слоты Поля")]
    public List<SlotController> playerSlots;
    public List<SlotController> enemySlots;

    // --- Списки карт на поле ---
    private List<CardController> playerFieldCards = new List<CardController>();
    private List<CardController> enemyFieldCards = new List<CardController>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // StartGame(); // Можно вызывать отсюда или из GameManager
    }

    // ========================================================================================
    //                                  УПРАВЛЕНИЕ ХОДОМ
    // ========================================================================================

    public void StartGame()
    {
        currentRound = 0;
        maxFearPoints = 0;

        // Начинаем первый ход с Игрока
        StartTurn(PlayerType.Player);
        Debug.Log("[BM] Игра запущена!");
    }

    public void StartTurn(PlayerType player)
    {
        currentTurnOwner = player;

        // --- 1. ОБЩАЯ ЛОГИКА НАЧИСЛЕНИЯ РЕСУРСОВ ---

        // Если ходит Игрок, значит начался абсолютно новый раунд
        if (player == PlayerType.Player)
        {
            currentRound++;
        }

        // Рассчитываем Максимум FP на этот ход (зависит от номера раунда)
        maxFearPoints = Mathf.Min(currentRound, FP_CAP);

        // ВАЖНО: Восстанавливаем FP до максимума. 
        // Это делается и для Игрока, и для Врага, чтобы у бота тоже был лимит!
        currentFearPoints = maxFearPoints;

        // ---------------------------------------------

        if (player == PlayerType.Player)
        {
            // --- ХОД ИГРОКА ---
            Debug.Log($"=== РАУНД {currentRound}: ХОД ИГРОКА (FP: {currentFearPoints}) ===");

            UpdateFearPointsUI(); // Обновляем текст в UI

            // Включаем кнопку конца хода
            if (endTurnButton != null) endTurnButton.interactable = true;

            // Добор карты игроком
            if (HandManager.Instance != null) HandManager.Instance.DrawCard();
        }
        else
        {
            // --- ХОД ВРАГА ---
            Debug.Log($"=== РАУНД {currentRound}: ХОД ВРАГА (FP: {currentFearPoints}) ===");

            // Выключаем кнопку конца хода
            if (endTurnButton != null) endTurnButton.interactable = false;

            // Запускаем логику бота
            if (EnemyManager.Instance != null) EnemyManager.Instance.StartEnemyTurn();
        }
    }

    /// <summary>
    /// Этот метод привязывается к UI Кнопке "Конец Хода".
    /// </summary>
    public void OnEndTurnButton()
    {
        if (currentTurnOwner == PlayerType.Player)
        {
            EndTurn(PlayerType.Player);
        }
    }

    /// <summary>
    /// Завершает ход указанного игрока. Вызывается кнопкой (Player) или ботом (Enemy).
    /// </summary>
    public void EndTurn(PlayerType player)
    {
        Debug.Log($"--- {player} завершает ход. Фаза Боя... ---");

        // 1. Вызов фазы боя (Атакует тот, чей ход закончился)
        ExecuteCombatPhase(player);

        // 2. Передача хода
        if (player == PlayerType.Player)
        {
            StartTurn(PlayerType.Enemy);
        }
        else
        {
            StartTurn(PlayerType.Player);
        }
    }

    // ========================================================================================
    //                                УПРАВЛЕНИЕ РЕСУРСАМИ
    // ========================================================================================

    public bool TrySpendFearPoints(PlayerType player, int cost)
    {
        // 1. Проверяем, что тот, кто тратит, владеет ходом
        if (player != currentTurnOwner) return false;

        // 2. Проверяем, хватает ли очков (currentFearPoints общие для текущего хода)
        if (currentFearPoints >= cost)
        {
            currentFearPoints -= cost;

            // Обновляем UI только если тратит Игрок (у врага FP скрыты)
            if (player == PlayerType.Player)
            {
                UpdateFearPointsUI();
            }

            // Для отладки можно вывести в консоль
            if (player == PlayerType.Enemy)
            {
                Debug.Log($"[BM] Враг потратил {cost} FP. Осталось: {currentFearPoints}");
            }

            return true;
        }

        // Если это враг, пишем в лог, почему он не сходил
        if (player == PlayerType.Enemy)
        {
            Debug.Log($"[BM] Врагу не хватает FP на карту. Нужно {cost}, есть {currentFearPoints}");
        }

        return false;
    }

    public void UpdateFearPointsUI()
    {
        if (fearPointsText != null)
        {
            fearPointsText.text = $"FP: {currentFearPoints}/{maxFearPoints}";
        }
    }

    // ========================================================================================
    //                                  ГЛАВНЫЙ ЦИКЛ БОЯ
    // ========================================================================================

    private void ExecuteCombatPhase(PlayerType activePlayer)
    {
        Debug.Log($"--- ФАЗА БОЯ ({activePlayer}) НАЧАТА ---");

        if (activePlayer == PlayerType.Player)
        {
            // Атакуют карты Игрока
            PerformMovementPhase(playerFieldCards);
            PerformSupportPhase(playerFieldCards);
            PerformAttackPhase(playerFieldCards, enemySlots);
        }
        else
        {
            // Атакуют карты Врага (Зеркально)
            PerformMovementPhase(enemyFieldCards);
            PerformSupportPhase(enemyFieldCards);
            PerformAttackPhase(enemyFieldCards, playerSlots);
        }

        Debug.Log("--- ФАЗА БОЯ ЗАВЕРШЕНА. ---");
    }

    // ----------------------------------------------------------------------
    //                           ФАЗА ПЕРЕМЕЩЕНИЯ
    // ----------------------------------------------------------------------

    private void PerformMovementPhase(List<CardController> fieldCards)
    {
        // Создаем копию списка
        List<CardController> cardsToMove = new List<CardController>(fieldCards);

        foreach (CardController card in cardsToMove)
        {
            if (card == null || card.currentSlot == null) continue;

            if (card.data.attackPattern == AttackPattern.SupportOnly ||
                card.data.attackPattern == AttackPattern.TargetThroughLine)
            {
                continue;
            }

            SlotController currentSlot = card.currentSlot;

            if (!currentSlot.isFrontLine)
            {
                SlotController forwardSlot = currentSlot.forwardSlot;
                if (forwardSlot != null && !forwardSlot.IsOccupied())
                {
                    currentSlot.ClearOccupant();
                    forwardSlot.SetOccupant(card);
                }
            }
        }
    }

    // ----------------------------------------------------------------------
    //                           ФАЗА ПОДДЕРЖКИ
    // ----------------------------------------------------------------------

    private void PerformSupportPhase(List<CardController> fieldCards)
    {
        foreach (CardController card in fieldCards)
        {
            if (card == null || card.currentSlot == null) continue;

            if (card.data.attackPattern == AttackPattern.SupportOnly)
            {
                // Для врага "вперед" - это тоже forwardSlot (мы должны были настроить их зеркально)
                SlotController forwardSlot = card.currentSlot.forwardSlot;

                if (forwardSlot != null && forwardSlot.IsOccupied())
                {
                    CardController target = forwardSlot.GetOccupant();
                    if (target == null) continue;

                    Debug.Log($"Саппорт {card.data.displayName} баффает {target.data.displayName}.");

                    if (target.data.displayName == "Мальвина") target.ApplyBlock();
                    else if (target.data.displayName == "Винни-Пух")
                    {
                        target.Heal(card.data.supportHealAmount);
                        target.RemoveAllDebuffs();
                    }
                    else if (target.data.displayName == "Железный Дровосек") target.ApplyRebirth();
                    else if (target.data.displayName == "Буратино") target.ApplyOnFire(2);
                }
            }
        }
    }

    // ----------------------------------------------------------------------
    //                           ФАЗА АТАКИ
    // ----------------------------------------------------------------------

    private void PerformAttackPhase(List<CardController> fieldCards, List<SlotController> opponentSlots)
    {
        List<CardController> attackers = new List<CardController>(fieldCards);

        foreach (CardController attackingCard in attackers)
        {
            if (attackingCard == null || attackingCard.currentSlot == null) continue;
            if (attackingCard.data.attackPattern == AttackPattern.SupportOnly) continue;

            CardController target = FindTarget(attackingCard, attackingCard.currentSlot, opponentSlots);

            if (target != null)
            {
                int damage = attackingCard.GetTotalAttack();
                Debug.Log($"{attackingCard.data.displayName} атакует {target.data.displayName}");
                target.TakeDamage(damage);
            }
            else
            {
                Debug.Log($"{attackingCard.data.displayName} атакует базу.");
                // HealthManager.Instance.TakeDamage(...)
            }
        }
    }

    private CardController FindTarget(CardController attacker, SlotController currentSlot, List<SlotController> opponentSlots)
    {
        AttackPattern pattern = attacker.data.attackPattern;
        SlotController targetSlot = currentSlot.opponentSlot;

        if (pattern == AttackPattern.TargetThroughLine)
        {
            bool targetFrontLine = !currentSlot.isFrontLine;
            var targetLineSlots = opponentSlots.FindAll(s => s.isFrontLine == targetFrontLine);
            foreach (SlotController slot in targetLineSlots)
            {
                CardController target = slot.GetOccupant();
                if (target != null) return target;
            }
            return null;
        }

        if (targetSlot != null)
        {
            CardController target = targetSlot.GetOccupant();
            if (target != null) return target;
        }

        if (pattern == AttackPattern.FlexibleFront)
        {
            // Логика Буратино для поиска соседей
            // Примечание: Это работает для игрока. Для врага нужно убедиться, что enemySlots упорядочены так же.
            List<SlotController> mySideSlots = (attacker.isPlayerCard) ? playerSlots : enemySlots;
            int currentIndex = mySideSlots.IndexOf(currentSlot);

            if (currentIndex != -1)
            {
                int[] adjacentIndices = { currentIndex - 1, currentIndex + 1 };
                foreach (int adjIndex in adjacentIndices)
                {
                    if (adjIndex >= 0 && adjIndex < opponentSlots.Count)
                    {
                        SlotController potentialTargetSlot = opponentSlots[adjIndex];
                        if (!potentialTargetSlot.isFrontLine)
                        {
                            CardController target = potentialTargetSlot.GetOccupant();
                            if (target != null) return target;
                        }
                    }
                }
            }
        }

        return null;
    }

    // ----------------------------------------------------------------------
    //                           РЕГИСТРАЦИЯ КАРТ
    // ----------------------------------------------------------------------

    public void RegisterCardDeployment(CardController card, PlayerType player)
    {
        // Добавляем пометку карте, чья она (полезно для логики)
        card.isPlayerCard = (player == PlayerType.Player);

        if (player == PlayerType.Player) playerFieldCards.Add(card);
        else enemyFieldCards.Add(card);
    }

    public void DeregisterCard(CardController card)
    {
        if (playerFieldCards.Contains(card)) playerFieldCards.Remove(card);
        else if (enemyFieldCards.Contains(card)) enemyFieldCards.Remove(card);
    }
}