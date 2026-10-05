using UnityEngine;
using UnityEngine.UI;

// Your hearts! Enemies knock them away.
// Lose them all and you wake up back at the start of the castle (but keep your sword!).
public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 5;
    public Text heartsText;

    // Drag an empty GameObject placed at your wake-up spot into spawnPoint
    public Transform spawnPoint;

    private int hearts;

    void Start()
    {
        hearts = maxHearts;
        UpdateUI();
    }

    public void LoseHeart(int amount)
    {
        hearts -= amount;
        UpdateUI();

        if (hearts <= 0)
        {
            hearts = maxHearts;
            UpdateUI();
            if (spawnPoint != null)
                transform.position = spawnPoint.position;
            if (StageManager.Instance != null)
                StageManager.Instance.ShowMessage("Ouch! You woke up back at the start. Grab your sword and try again, hero!");
        }
    }

    public void AddHeart(int amount)
    {
        hearts = Mathf.Min(maxHearts, hearts + amount);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (heartsText != null)
        {
            string h = "";
            for (int i = 0; i < hearts; i++) h += "♥";
            heartsText.text = h;
        }
    }
}
