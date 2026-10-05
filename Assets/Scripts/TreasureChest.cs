using UnityEngine;

// The chest you see when you wake up! Walk close and press E to open it.
// Inside: a STONE SWORD!
// The chest needs a trigger Collider. Tag the player as "Player".
// (Mobile: call OpenButton() from an on-screen "Open" button.)
public class TreasureChest : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject lid;         // the lid will pop open!
    public GameObject swordVisual; // little sword model inside the chest

    private bool opened = false;
    private bool playerNear = false;

    void Update()
    {
        if (!opened && playerNear && Input.GetKeyDown(KeyCode.E))
            OpenChest();
    }

    public void OpenButton()
    {
        if (!opened && playerNear) OpenChest();
    }

    void OpenChest()
    {
        opened = true;

        if (lid != null)
            lid.transform.rotation = Quaternion.Euler(-110f, 0f, 0f); // lid flies open!

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var player = playerObj.GetComponent<CastlePlayer>();
            if (player != null) player.hasSword = true;
            if (swordVisual != null) swordVisual.SetActive(false); // the sword is yours now!
        }

        if (StageManager.Instance != null)
            StageManager.Instance.ChestOpened();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerNear = false;
    }
}
