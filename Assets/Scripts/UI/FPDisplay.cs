using UnityEngine;
using TMPro;

public class FPDisplay : MonoBehaviour // <-- Открытие класса {1
{
    public static FPDisplay Instance;

    // Ссылка, которую нужно назначить в Инспекторе
    public TextMeshProUGUI playerFPText;

    private void Awake() // <-- Открытие Awake {2
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    } // <-- Закрытие Awake }2

    // Метод, вызываемый BattleManager для обновления
    public void UpdatePlayerFP(int currentFP) // <-- Открытие UpdatePlayerFP {3
    {
        if (playerFPText != null)
        {
            // Форматирование для отображения
            playerFPText.text = $"FP: {currentFP}";
        }
    } // <-- Закрытие UpdatePlayerFP }3

} // <-- Закрытие класса FPDisplay }1 (Это должна быть строка 30 или около того)