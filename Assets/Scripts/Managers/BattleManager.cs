using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static BattleManager Instance;

    // Перечисление для обозначения игроков
    public enum PlayerType { Player, Enemy }

    [Header("Настройки")]
    public PlayerType currentTurnOwner; // Чей сейчас ход

    // --- FEAR POINTS (FP) ---
    [Header("Очки Страха (FP)")]
    public const int FP_CAP = 10;

    public int maxFearPoints;
    public int currentFearPoints;

    // --- ИГРОВОЙ ЦИКЛ ---
    [Header("Игровой Цикл")]
    public int currentRound = 0;

    // --- UI REFERENCES ---
    [Header("Ссылки UI")]
    public TextMeshProUGUI fearPointsText;
    public Button endTurnButton;

    // --- Слоты Поля ---
    [Header("Слоты Поля")]
    [Tooltip("Список слотов игрока")]
    public List<SlotController> playerSlots;

    [Tooltip("Список слотов противника")]
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
        // StartGame(); 
    }

    // ========================================================================================
    //                                  УПРАВЛЕНИЕ ХОДОМ
    // ========================================================================================

    public void StartGame()
    {
        currentRound = 0;
        maxFearPoints = 0;

        // Сброс здоровья через менеджер
        if (HealthManager.Instance != null) HealthManager.Instance.InitializeHealth();

        StartTurn(PlayerType.Player);
        Debug.Log("[BM] Игра запущена!");
    }

    public void StartTurn(PlayerType player)
    {
        currentTurnOwner = player;

        if (player == PlayerType.Player)
        {
            currentRound++;
            maxFearPoints = Mathf.Min(currentRound, FP_CAP);
            currentFearPoints = maxFearPoints;
            UpdateFearPointsUI();

            Debug.Log($"=== РАУНД {currentRound}: ХОД ИГРОКА ===");
            if (endTurnButton != null) endTurnButton.interactable = true;
            if (HandManager.Instance != null) HandManager.Instance.DrawCard();
        }
        else
        {
            Debug.Log($"=== РАУНД {currentRound}: ХОД ВРАГА ===");
            if (endTurnButton != null) endTurnButton.interactable = false;
            if (EnemyManager.Instance != null) EnemyManager.Instance.StartEnemyTurn();
        }
    }

    public void OnEndTurnButton()
    {
        if (currentTurnOwner == PlayerType.Player)
        {
            EndTurn(PlayerType.Player);
        }
    }

    public void EndTurn(PlayerType player)
    {
        Debug.Log($"--- {player} завершает ход. Фаза Боя... ---");

        // 1. Вызов фазы боя
        ExecuteCombatPhase(player);

        // 2. Передача хода
        if (player == PlayerType.Player) StartTurn(PlayerType.Enemy);
        else StartTurn(PlayerType.Player);
    }

    // ========================================================================================
    //                                УПРАВЛЕНИЕ РЕСУРСАМИ
    // ========================================================================================

    public bool TrySpendFearPoints(PlayerType player, int cost)
    {
        if (player != currentTurnOwner) return false;

        if (currentFearPoints >= cost)
        {
            currentFearPoints -= cost;
            if (player == PlayerType.Player) UpdateFearPointsUI();
            else Debug.Log($"[BM] Враг потратил {cost} FP. Осталось: {currentFearPoints}");
            return true;
        }

        if (player == PlayerType.Enemy) Debug.Log($"[BM] Врагу не хватает FP.");
        return false;
    }

    public void UpdateFearPointsUI()
    {
        if (fearPointsText != null)
            fearPointsText.text = $"FP: {currentFearPoints}/{maxFearPoints}";
    }

    // ========================================================================================
    //                                  ГЛАВНЫЙ ЦИКЛ БОЯ
    // ========================================================================================

    private void ExecuteCombatPhase(PlayerType activePlayer)
    {
        Debug.Log($"--- ФАЗА БОЯ ({activePlayer}) НАЧАТА ---");

        if (activePlayer == PlayerType.Player)
        {
            PerformMovementPhase(playerFieldCards);
            PerformSupportPhase(playerFieldCards);
            PerformAttackPhase(playerFieldCards, enemySlots, PlayerType.Enemy); // Атакуем Врага
        }
        else
        {
            PerformMovementPhase(enemyFieldCards);
            PerformSupportPhase(enemyFieldCards);
            PerformAttackPhase(enemyFieldCards, playerSlots, PlayerType.Player); // Атакуем Игрока
        }

        Debug.Log("--- ФАЗА БОЯ ЗАВЕРШЕНА. ---");
    }

    // ----------------------------------------------------------------------
    //                           ФАЗА ПЕРЕМЕЩЕНИЯ
    // ----------------------------------------------------------------------

    private void PerformMovementPhase(List<CardController> fieldCards)
    {
        List<CardController> cardsToMove = new List<CardController>(fieldCards);

        foreach (CardController card in cardsToMove)
        {
            if (card == null || card.currentSlot == null) continue;

            if (card.data.attackPattern == AttackPattern.SupportOnly ||
                card.data.attackPattern == AttackPattern.TargetThroughLine)
                continue;

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
    //                           ФАЗА АТАКИ (С HEALTH MANAGER)
    // ----------------------------------------------------------------------

    // Добавил аргумент defenderType, чтобы знать, чью базу атаковать
    private void PerformAttackPhase(List<CardController> fieldCards, List<SlotController> opponentSlots, PlayerType defenderType)
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
                // !!! ИНТЕГРАЦИЯ HEALTH MANAGER !!!
                int damage = attackingCard.GetTotalAttack();
                Debug.Log($"{attackingCard.data.displayName} атакует БАЗУ ({defenderType}) на {damage} урона.");

                if (HealthManager.Instance != null)
                {
                    HealthManager.Instance.TakeDamage(defenderType, damage);
                }
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

    // --- РЕГИСТРАЦИЯ КАРТ ---

    public void RegisterCardDeployment(CardController card, PlayerType player)
    {
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