using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Для перезагрузки сцены

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
    public GameObject gameOverPanel; // Панель конца игры
    public TextMeshProUGUI winnerText; // Текст "Победа/Поражение"

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeHealth();
    }

    public void InitializeHealth()
    {
        playerHealth = maxHealth;
        enemyHealth = maxHealth;
        isGameOver = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateHealthUI();
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
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}