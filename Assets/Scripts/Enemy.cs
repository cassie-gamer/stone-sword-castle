using UnityEngine;

// A castle enemy: masked mouse, snake, tiger, bear - or the BOSS!
// Set the stats in the Inspector for each animal:
//   Masked mouse: HP 2, speed 3.5 | Snake: HP 3, speed 3
//   Tiger: HP 6, speed 4.5 | Bear: HP 10, speed 3.5 | Boss: HP 25, speed 4
// Needs a trigger Collider. The player must be tagged "Player".
public class Enemy : MonoBehaviour
{
    [Header("Enemy stats - tune per animal!")]
    public string enemyName = "Masked Mouse";
    public int maxHP = 2;
    public int damage = 1;
    public float moveSpeed = 3f;
    public float chaseRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.2f;

    protected int hp;
    protected Transform player;
    private float attackTimer = 0f;

    protected virtual void Start()
    {
        hp = maxHP;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    protected virtual void Update()
    {
        if (player == null || hp <= 0) return;
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < chaseRange && dist > attackRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0f;
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        else if (dist <= attackRange && attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            var health = player.GetComponent<PlayerHealth>();
            if (health != null) health.LoseHeart(damage);
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (hp <= 0) return;
        hp -= amount;
        Debug.Log(enemyName + " hit! HP left: " + hp);
        if (hp <= 0) Die();
    }

    protected virtual void Die()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.EnemyDefeated(this);
        Destroy(gameObject);
    }
}
