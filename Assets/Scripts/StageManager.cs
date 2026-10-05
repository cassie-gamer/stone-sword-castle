using UnityEngine;
using UnityEngine.UI;

// Runs the 5 stages of the giant castle!
//   Wake up room: open the chest to get the stone sword
//   Stage 1: 3 masked mice! | Stage 2: 3 snakes!
//   Stage 3: 1 tiger! | Stage 4: 1 bear! | Stage 5: THE BOSS (25 hits!)
// HOW TO SET UP:
//  1. Make 5 empty GameObjects named Stage1..Stage5. Put each stage's enemies
//     inside its group (3 mice in Stage1, 3 snakes in Stage2, tiger in Stage3,
//     bear in Stage4, boss in Stage5). Set ALL groups INACTIVE.
//  2. Drag the 5 groups into the Stage Groups array below, in order.
//  3. The boss needs Enemy (maxHP 25) + BossCharge.
public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("UI")]
    public Text messageText;
    public Text stageText;
    public GameObject victoryText; // big "VICTORY!" text, starts INACTIVE

    [Header("Drag Stage1..Stage5 groups here in order")]
    public GameObject[] stageGroups = new GameObject[5];

    // 0 = wake-up room, 1-5 = stages
    private int stage = 0;
    private int enemiesLeft = 0;
    private bool chestDone = false;

    void Awake() { Instance = this; }

    void Start()
    {
        foreach (var g in stageGroups)
            if (g != null) g.SetActive(false);

        ShowMessage("You wake up in a giant castle... What's in that chest? (Walk up and press E)");
        UpdateStageUI();
    }

    // Called by TreasureChest
    public void ChestOpened()
    {
        if (chestDone) return;
        chestDone = true;
        ShowMessage("You got the STONE SWORD! Get ready...");
        NextStage();
    }

    // Called by Enemy when it dies
    public void EnemyDefeated(Enemy e)
    {
        enemiesLeft--;
        if (enemiesLeft <= 0)
        {
            if (stage >= 5)
                WinGame();
            else
            {
                ShowMessage("Stage clear! ...");
                NextStage();
            }
        }
    }

    void NextStage()
    {
        stage++;
        UpdateStageUI();

        if (stage - 1 < stageGroups.Length && stageGroups[stage - 1] != null)
        {
            var group = stageGroups[stage - 1];
            group.SetActive(true);
            enemiesLeft = group.GetComponentsInChildren<Enemy>().Length;
        }

        switch (stage)
        {
            case 1: ShowMessage("STAGE 1: 3 masked mice attack!"); break;
            case 2: ShowMessage("STAGE 2: 3 snakes slither in!"); break;
            case 3: ShowMessage("STAGE 3: A tiger! Just one... but it's HUGE!"); break;
            case 4: ShowMessage("STAGE 4: A bear blocks your path!"); break;
            case 5: ShowMessage("FINAL STAGE: THE BOSS! It takes 25 hits! Be brave!"); break;
        }
    }

    void WinGame()
    {
        ShowMessage("VICTORY!");
        if (victoryText != null) victoryText.SetActive(true);
        Debug.Log("VICTORY! The boss is defeated!");
    }

    public void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
        Debug.Log(msg);
    }

    void UpdateStageUI()
    {
        if (stageText != null)
            stageText.text = stage == 0 ? "Wake up!" : "Stage " + stage + " / 5";
    }
}
