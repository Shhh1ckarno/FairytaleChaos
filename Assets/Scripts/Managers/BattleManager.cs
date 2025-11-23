using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
// Предполагается, что CardData, CardController и AttackPattern доступны

public class BattleManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static BattleManager Instance;

    // Перечисление для обозначения игроков
    public enum PlayerType { Player, Enemy }

    // --- FEAR POINTS (FP) ---
    [Header("Очки Страха (FP)")]
    public const int FP_CAP = 10;

    public int maxFearPoints;
    private int currentFearPoints;

    // --- ИГРОВОЙ ЦИКЛ ---
    [Header("Игровой Цикл")]
    public int currentRound = 0;

    // --- UI REFERENCES ---
    [Header("Ссылки UI")]
    public TextMeshProUGUI fearPointsText;

    // --- Слоты Поля (КРИТИЧНО) ---
    [Header("Слоты Поля")]
    [Tooltip("Список слотов игрока, упорядоченный для логики Буратино.")]
    public List<SlotController> playerSlots;

    [Tooltip("Список слотов противника, упорядоченный для логики Буратино.")]
    public List<SlotController> enemySlots;

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
        // StartGame(); // Может вызываться внешним GameManager
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

    public void StartGame()
    {
        currentRound = 0;
        maxFearPoints = 0;
        currentFearPoints = 0;
        EndTurn();
        Debug.Log("[BM] Игра запущена! Подготовка завершена.");
    }

    public void EndTurn()
    {
        Debug.Log("--- Игрок завершает ход. Запуск фазы боя... ---");

        // 1. Вызов фазы боя
        ExecuteCombatPhase();

        // 2. Инкремент раунда и восстановление FP
        currentRound++;
        maxFearPoints = Mathf.Min(currentRound, FP_CAP);
        currentFearPoints = maxFearPoints;

        // 3. Обновление UI и добор карты
        UpdateFearPointsUI();
        if (HandManager.Instance != null)
        {
            HandManager.Instance.DrawCard();
        }

        Debug.Log($"--- Начат Раунд {currentRound}. Доступно FP: {maxFearPoints}. ---");
    }

    // ----------------------------------------------------------------------
    //                           ГЛАВНЫЙ ЦИКЛ БОЯ
    // ----------------------------------------------------------------------

    private void ExecuteCombatPhase()
    {
        Debug.Log("--- ФАЗА БОЯ НАЧАТА ---");

        // 1. ФАЗА ПЕРЕМЕЩЕНИЯ 
        PerformMovementPhase(playerFieldCards);

        // 2. ФАЗА ПОДДЕРЖКИ (Кальцифер)
        PerformSupportPhase(playerFieldCards);

        // 3. ФАЗА АТАКИ (Сложная логика)
        PerformAttackPhase(playerFieldCards, enemySlots);

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

            // Кальцифер (SupportOnly) и Мальвина (TargetThroughLine) не двигаются
            if (card.data.attackPattern == AttackPattern.SupportOnly ||
                card.data.attackPattern == AttackPattern.TargetThroughLine)
            {
                continue;
            }

            SlotController currentSlot = card.currentSlot;

            // Проверяем: Карта стоит на задней линии?
            if (!currentSlot.isFrontLine)
            {
                SlotController forwardSlot = currentSlot.forwardSlot;

                if (forwardSlot != null && !forwardSlot.IsOccupied())
                {
                    // Двигаемся вперед!
                    Debug.Log($"Карта {card.data.displayName} переместилась вперед.");

                    // Обновление ссылок
                    currentSlot.ClearOccupant();
                    forwardSlot.SetOccupant(card);
                }
            }
        }
    }


    // ----------------------------------------------------------------------
    //                           ФАЗА ПОДДЕРЖКИ (ИСПРАВЛЕНО)
    // ----------------------------------------------------------------------

    private void PerformSupportPhase(List<CardController> fieldCards)
    {
        foreach (CardController card in fieldCards)
        {
            if (card == null || card.currentSlot == null) continue;

            // Ищем только Кальцифера (SupportOnly)
            if (card.data.attackPattern == AttackPattern.SupportOnly)
            {
                // Кальцифер поддерживает слот прямо перед собой
                SlotController forwardSlot = card.currentSlot.forwardSlot;

                if (forwardSlot != null && forwardSlot.IsOccupied())
                {
                    CardController target = forwardSlot.GetOccupant();
                    if (target == null) continue;

                    Debug.Log($"Кальцифер поддерживает {target.data.displayName}.");

                    // --- ЛОГИКА ПРИМЕНЕНИЯ БАФФОВ ---
                    // ВАЖНО: displayName должен точно совпадать с тем, что написано в CardData!

                    // 1. Закалить Мальвину (Block)
                    if (target.data.displayName == "Мальвина")
                    {
                        target.ApplyBlock();
                    }
                    // 2. Полечить Винни-Пуха и снять дебаффы
                    else if (target.data.displayName == "Винни-Пух")
                    {
                        // Используем supportHealAmount из данных Кальцифера
                        target.Heal(card.data.supportHealAmount);
                        target.RemoveAllDebuffs();
                    }
                    // 3. Переплавить Дровосека (Перерождение)
                    else if (target.data.displayName == "Железный Дровосек")
                    {
                        target.ApplyRebirth();
                    }
                    // 4. Поджечь Буратино (Бонус к урону)
                    else if (target.data.displayName == "Буратино")
                    {
                        target.ApplyOnFire(2); // Даем +2 к атаке
                    }
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

            // Кальцифер (SupportOnly) не атакует
            if (attackingCard.data.attackPattern == AttackPattern.SupportOnly) continue;

            CardController target = FindTarget(attackingCard, attackingCard.currentSlot, opponentSlots);

            if (target != null)
            {
                // Цель найдена!
                // Используем GetTotalAttack(), чтобы учесть бонус от поджога
                int damage = attackingCard.GetTotalAttack();

                Debug.Log($"Карта {attackingCard.data.displayName} ({damage} ATK) атакует {target.data.displayName}");
                target.TakeDamage(damage);
            }
            else
            {
                // Атака по главному герою (базе)
                Debug.Log($"Карта {attackingCard.data.displayName} атакует базу противника.");
                // HealthManager.Instance.EnemyTakeDamage(attackingCard.GetTotalAttack()); 
            }
        }
    }

    /// <summary>
    /// Определяет цель атаки на основе паттерна.
    /// </summary>
    private CardController FindTarget(CardController attacker, SlotController currentSlot, List<SlotController> opponentSlots)
    {
        AttackPattern pattern = attacker.data.attackPattern;
        SlotController targetSlot = currentSlot.opponentSlot; // Слот прямо напротив

        // 1. Проверяем TARGET_THROUGH_LINE (Мальвина)
        if (pattern == AttackPattern.TargetThroughLine)
        {
            List<SlotController> targetLineSlots = new List<SlotController>();

            // Если карта находится на ПЕРЕДНЕЙ линии, она бьет на заднюю линию противника.
            // Если карта находится на ЗАДНЕЙ линии, она бьет на переднюю линию противника.
            bool targetFrontLine = !currentSlot.isFrontLine;

            targetLineSlots = opponentSlots.FindAll(s => s.isFrontLine == targetFrontLine);

            // Мальвина бьет по первому доступному (неважно где стоит)
            foreach (SlotController slot in targetLineSlots)
            {
                CardController target = slot.GetOccupant();
                if (target != null) return target;
            }

            return null;
        }

        // --- Дровосек, Винни-Пух, Буратино (ПРИОРИТЕТ 1: ПРЯМО НАПРОТИВ) ---
        if (targetSlot != null)
        {
            CardController target = targetSlot.GetOccupant();
            if (target != null)
            {
                return target; // Цель найдена!
            }
        }

        // --- ПРИОРИТЕТ 2: ГИБКИЙ УДАР (Только для FlexibleFront / Буратино) ---
        if (pattern == AttackPattern.FlexibleFront)
        {
            // Буратино: Прямая линия пуста. Атакует заднюю линию на другой дорожке.

            int currentIndex = playerSlots.IndexOf(currentSlot);

            if (currentIndex != -1)
            {
                // Ищем цели на соседних дорожках противника
                int[] adjacentIndices = { currentIndex - 1, currentIndex + 1 };

                foreach (int adjIndex in adjacentIndices)
                {
                    if (adjIndex >= 0 && adjIndex < opponentSlots.Count)
                    {
                        SlotController potentialTargetSlot = opponentSlots[adjIndex];

                        // Проверяем, что это слот ЗАДНЕЙ линии противника
                        if (potentialTargetSlot.isFrontLine == false)
                        {
                            CardController target = potentialTargetSlot.GetOccupant();
                            if (target != null)
                            {
                                return target;
                            }
                        }
                    }
                }
            }
        }

        return null;
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
        }
        else if (enemyFieldCards.Contains(card))
        {
            enemyFieldCards.Remove(card);
        }
        Debug.Log($"Карта {card.data.displayName} удалена с поля.");
    }
}