using UnityEngine;

// The hero's STONE SWORD! Swings in an arc and hurts enemies in front.
// Add to the player. It only works after the chest is opened (hasSword = true).
public class StoneSword : MonoBehaviour
{
    [Header("Sword")]
    public int damage = 1;        // the boss needs 25 hits!
    public float range = 2.5f;
    public float cooldown = 0.5f; // seconds between swings

    private float cooldownLeft = 0f;

    void Update()
    {
        if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;
    }

    public void Swing()
    {
        if (cooldownLeft > 0f) return;
        cooldownLeft = cooldown;

        // Hit every enemy in front of the player
        Vector3 center = transform.position + transform.forward * (range * 0.6f);
        var hits = Physics.OverlapSphere(center, range);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        Debug.Log("Stone sword swing!");
    }
}
