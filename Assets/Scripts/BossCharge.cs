using UnityEngine;
using System.Collections;

// The FINAL BOSS! Add this plus the Enemy script (set Enemy maxHP = 25).
// Every few seconds the boss CHARGES at you - run away!
public class BossCharge : MonoBehaviour
{
    [Header("Charge attack")]
    public float chargeSpeed = 9f;
    public float chargeTime = 1f;
    public float chargeCooldown = 5f;

    private float timer;
    private bool charging = false;
    private Vector3 chargeDir;
    private Transform player;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        timer = chargeCooldown;
    }

    void Update()
    {
        if (player == null) return;
        timer -= Time.deltaTime;

        if (!charging && timer <= 0f)
        {
            charging = true;
            chargeDir = (player.position - transform.position).normalized;
            chargeDir.y = 0f;
            timer = chargeCooldown;
            StartCoroutine(StopCharge());
            Debug.Log("Boss is CHARGING!");
        }

        if (charging)
            transform.position += chargeDir * chargeSpeed * Time.deltaTime;
    }

    IEnumerator StopCharge()
    {
        yield return new WaitForSeconds(chargeTime);
        charging = false;
    }
}
