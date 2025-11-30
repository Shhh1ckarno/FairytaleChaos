using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    // --- СТАТИЧЕСКИЙ СИНГЛТОН ---
    public static EnemyManager Instance;

    [Header("Настройки Бота")]
    [Tooltip("Задержка перед действием (чтобы не происходило мгновенно)")]
    public float actionDelay = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Начинает ход противника.
    /// </summary>
    public void StartEnemyTurn()
    {
        // Запускаем логику в корутине, чтобы были паузы
        StartCoroutine(EnemyLogicRoutine());
    }

    private IEnumerator EnemyLogicRoutine()
    {
        Debug.Log("--- Ход Противника Начался ---");

        // 1. Небольшая пауза для реалистичности ("бот думает")
        yield return new WaitForSeconds(actionDelay);

        // 2. Получаем слоты врага
        List<SlotController> enemySlots = BattleManager.Instance.enemySlots;

        // Лимит карт, которые бот хочет сыграть за ход (можно сделать умнее, но пока ставим максимум)
        int cardsPlayed = 0;
        int maxCardsPerTurn = 3;

        // 3. Пробегаем по слотам и пытаемся поставить карты
        foreach (SlotController slot in enemySlots)
        {
            // Если мы уже сыграли лимит или слот занят -> пропускаем
            if (cardsPlayed >= maxCardsPerTurn) break;
            if (slot.IsOccupied()) continue;

            // 4. Выбираем случайную карту из базы данных
            CardData randomCardData = GetRandomCardForEnemy();

            if (randomCardData == null) break;

            // *** ПРОВЕРКА ПРЕФАБА ***
            // Используем ваше поле 'prefab' из CardData
            if (randomCardData.prefab == null)
            {
                Debug.LogError($"[EnemyManager] У карты '{randomCardData.displayName}' не назначен Prefab!");
                continue;
            }

            // 5. Проверяем, хватает ли FP (ресурсов)
            // Бот тратит ресурсы так же, как и игрок
            bool canAfford = BattleManager.Instance.TrySpendFearPoints(BattleManager.PlayerType.Enemy, randomCardData.costFear);

            if (canAfford)
            {
                // 6. Спавним карту (используя префаб из данных)
                SpawnEnemyCard(randomCardData, slot);

                cardsPlayed++;

                // Пауза между выкладыванием карт
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log($"--- Противник завершает ход. Сыграно карт: {cardsPlayed} ---");

        // 7. Завершаем ход и передаем управление игроку
        yield return new WaitForSeconds(0.5f);
        BattleManager.Instance.EndTurn(BattleManager.PlayerType.Enemy);
    }

    /// <summary>
    /// Создает карту на поле, используя префаб, указанный в CardData.
    /// </summary>
    private void SpawnEnemyCard(CardData data, SlotController targetSlot)
    {
        // ИНСТАНЦИРУЕМ ПРЕФАБ КОНКРЕТНОЙ КАРТЫ
        // Используем data.prefab, так как у вас поле называется 'prefab'
        GameObject newCardObj = Instantiate(data.prefab);

        // Настраиваем логику
        CardController cardCtrl = newCardObj.GetComponent<CardController>();

        if (cardCtrl != null)
        {
            // 1. Инициализация данных
            cardCtrl.Initialize(data);

            // 2. Помечаем, что это карта врага (для запрета перетаскивания игроком)
            cardCtrl.isPlayerCard = false;

            // 3. Ставим в слот
            cardCtrl.currentSlot = targetSlot;
            targetSlot.SetOccupant(cardCtrl);

            // 4. Регистрируем в BattleManager (чтобы она участвовала в бою)
            BattleManager.Instance.RegisterCardDeployment(cardCtrl, BattleManager.PlayerType.Enemy);
        }
        else
        {
            Debug.LogError($"[EnemyManager] На префабе карты '{data.displayName}' нет скрипта CardController!");
            Destroy(newCardObj);
        }
    }

    private CardData GetRandomCardForEnemy()
    {
        // Берем случайную карту из общего списка карт в CardManager
        if (CardManager.Instance != null && CardManager.Instance.cardList.Count > 0)
        {
            int randomIndex = Random.Range(0, CardManager.Instance.cardList.Count);
            return CardManager.Instance.cardList[randomIndex];
        }

        Debug.LogWarning("[EnemyManager] Список карт в CardManager пуст!");
        return null;
    }
}