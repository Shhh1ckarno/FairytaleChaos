using UnityEngine;
using TMPro;
// using UnityEngine.SceneManagement; // <-- УДАЛЯЕМ, так как рестарт будет в GameManager

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    [Header("Настройки")]
    public int maxHealth = 20;

    [Header("Состояние")]
    public int playerHealth;
    public int enemyHealth;

    [Header("UI Ссылки")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // !!! ИСПРАВЛЕНИЕ: УДАЛЯЕМ ВЫЗОВ InitializeHealth() из Start() !!!
    // Игра теперь запускается только из GameManager.InitializeGame()
    /*
    private void Start()
    {
        InitializeHealth(); 
    }
    */

    public void InitializeHealth()
    {
        playerHealth = maxHealth;
        enemyHealth = maxHealth;
        isGameOver = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateHealthUI();
        Debug.Log("[HM] Здоровье сброшено и инициализировано.");
    }

    public void TakeDamage(BattleManager.PlayerType victim, int damage)
    {
        if (isGameOver) return;

        if (victim == BattleManager.PlayerType.Player)
        {
            playerHealth -= damage;
            Debug.Log($"[Health] Игрок получил {damage} урона. HP: {playerHealth}");
        }
        else
        {
            enemyHealth -= damage;
            Debug.Log($"[Health] Враг получил {damage} урона. HP: {enemyHealth}");
        }

        UpdateHealthUI();
        CheckGameOver();
    }

    private void UpdateHealthUI()
    {
        if (playerHealthText != null) playerHealthText.text = $"PlayerHP: {playerHealth}/{maxHealth}";
        if (enemyHealthText != null) enemyHealthText.text = $"EnemyHP: {enemyHealth}/{maxHealth}";
    }

    // Метод для BattleManager, чтобы проверить состояние
    public bool IsGameOver()
    {
        return isGameOver;
    }

    private void CheckGameOver()
    {
        if (playerHealth <= 0) EndGame(false);
        else if (enemyHealth <= 0) EndGame(true);
    }

    private void EndGame(bool playerWon)
    {
        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (winnerText != null)
            {
                winnerText.text = playerWon ? "ПОБЕДА!" : "ПОРАЖЕНИЕ...";
                winnerText.color = playerWon ? Color.green : Color.red;
            }
        }

        // ОСТАНОВКА ИГРЫ
        if (BattleManager.Instance != null)
        {
            // Отключаем кнопку End Turn и другие взаимодействия
            BattleManager.Instance.DisableInteractions();
        }
    }

    // !!! ИСПРАВЛЕНИЕ: МЕТОД RestartGame() УДАЛЕН. Он будет в GameManager.
}