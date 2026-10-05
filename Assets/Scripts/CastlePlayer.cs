using UnityEngine;

// You wake up in a giant castle! Move with WASD / arrow keys.
// Press SPACE to swing your sword - but first you must open the chest!
// (Mobile: use the on-screen joystick + Attack button -> AttackButton())
public class CastlePlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool hasSword = false;

    private StoneSword sword;

    void Start()
    {
        sword = GetComponent<StoneSword>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        if (move.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(move.normalized, Vector3.up);

        if (Input.GetKeyDown(KeyCode.Space))
            AttackButton();
    }

    public void AttackButton()
    {
        if (hasSword && sword != null)
            sword.Swing();
    }
}
