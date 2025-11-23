using UnityEngine;
using TMPro; // Required for TextMeshPro

// CardDisplay is responsible ONLY for the visual representation of the card.
public class CardDisplay : MonoBehaviour
{
    // --- UI References (Drag & Drop in Inspector) ---

    [Header("Basic Data")]
    // Using TextMeshPro for 3D text in world space
    public TextMeshPro nameText;
    public TextMeshPro descriptionText;

    // Artwork can be SpriteRenderer (3D) or Image (UI Canvas)
    public SpriteRenderer artworkImage;

    [Header("Stats")]
    public TextMeshPro attackText;
    public TextMeshPro hpText;
    public TextMeshPro costText;

    [Header("Status Indicators")]
    // NEW: Text element to show buffs/debuffs (e.g., "BLOCK", "ON FIRE")
    public TextMeshPro statusText;

    // Internal reference to current data
    private CardData currentData;

    // --- DISPLAY METHODS ---

    /// <summary>
    /// Initializes all visual elements of the card (Called by CardController).
    /// </summary>
    public void DisplayCardData(CardData data)
    {
        currentData = data;

        if (data == null)
        {
            Debug.LogError("Attempted to display null CardData.");
            return;
        }

        // --- Basic Data ---
        if (nameText != null)
            nameText.text = data.displayName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (artworkImage != null)
            artworkImage.sprite = data.artwork;

        // --- Stats ---
        if (attackText != null)
            attackText.text = data.attack.ToString();

        if (costText != null)
            costText.text = data.costFear.ToString();

        // Initialize HP using maxHP
        UpdateHPText(data.maxHP);

        // Hide status text initially
        if (statusText != null)
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Dynamically updates the HP text.
    /// </summary>
    public void UpdateHPText(int newHP)
    {
        if (hpText != null)
        {
            hpText.text = newHP.ToString();
        }
    }

    /// <summary>
    /// Dynamically updates the Attack text (e.g., if buffed by Fire).
    /// </summary>
    public void UpdateAttackText(int newAttack)
    {
        if (attackText != null)
        {
            attackText.text = newAttack.ToString();
        }
    }

    /// <summary>
    /// Updates the status indicator text.
    /// </summary>
    /// <param name="status">The text to display (e.g. "BLOCK").</param>
    /// <param name="isActive">Whether to show or hide the text.</param>
    public void UpdateStatusText(string status, bool isActive)
    {
        if (statusText != null)
        {
            statusText.text = status;
            statusText.gameObject.SetActive(isActive);

            // Optional: Color coding
            if (status.Contains("œŒƒ∆Œ√")) // IGNITION
                statusText.color = Color.red;
            else if (status.Contains("«¿ ¿À ¿")) // TEMPERING
                statusText.color = Color.cyan;
            else if (status.Contains("œ≈–≈œÀ¿¬ ¿")) // REBIRTH
                statusText.color = Color.yellow;
            else
                statusText.color = Color.white;
        }
    }
}