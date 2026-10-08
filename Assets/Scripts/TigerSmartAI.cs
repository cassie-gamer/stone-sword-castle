using UnityEngine;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ============================================================
// TIGER SMART AI (optional!)
// The stage-3 tiger, but it can THINK for itself!
//
// How it works:
//   Every 1 second the tiger describes the fight IN WORDS
//   ("the hero is close", "I am hurt") and asks Laya — a free,
//   offline AI brain running on your own PC — one question:
//   should I CHASE, CIRCLE, or RETREAT?
//   Then it does what Laya picks!
//
// If Laya is NOT running, nothing breaks: the tiger just uses
// its normal scripted behavior (chase when you're near).
// So this file is 100% optional. The game works fine without it.
//
// To try it (Windows):
//   1. pip install "laya[serve]"   (needs Python)
//   2. Run:  laya-serve            (leave it running!)
//   3. In Unity: Window -> Package Manager -> "+" ->
//      "Add package by name..." -> com.unity.nuget.newtonsoft-json
//   4. On the stage-3 tiger, REMOVE the Enemy script and ADD
//      this TigerSmartAI script instead (re-enter HP 6, speed 4.5).
//   5. Press Play. The tiger now asks its AI brain what to do!
// ============================================================

// Put this on the tiger INSTEAD of the normal Enemy script.
// (It extends Enemy, so damage, HP and dying still work the same.)
public class TigerSmartAI : Enemy
{
    [Header("Smart AI settings")]
    [Tooltip("How often the tiger asks Laya what to do (seconds)")]
    public float thinkEvery = 1.0f;

    [Tooltip("Where the tiger runs to when it retreats (optional - drag an empty GameObject)")]
    public Transform den;

    [Tooltip("Laya's address. Dev service = /v1/systemone. Shipped exe in StreamingAssets = /api/decide")]
    public string layaEndpoint = "http://127.0.0.1:8000/v1/systemone";

    // The tiger's current plan: set by Laya, or by the fallback.
    private enum TigerMode { Chase, Circle, Retreat }
    private TigerMode mode = TigerMode.Chase;

    private float thinkTimer = 0f;
    private bool thinking = false; // don't ask twice at once
    private float myAttackTimer = 0f;
    private LayaClient laya;

    protected override void Start()
    {
        base.Start(); // finds the player, sets HP, like a normal Enemy
        laya = new LayaClient(layaEndpoint);
        enemyName = "Smart Tiger";
        thinkTimer = 0.5f; // think almost right away
    }

    protected override void Update()
    {
        if (player == null || hp <= 0) return;
        if (myAttackTimer > 0f) myAttackTimer -= Time.deltaTime;

        // Time to ask the AI brain again?
        thinkTimer -= Time.deltaTime;
        if (thinkTimer <= 0f && !thinking)
        {
            thinkTimer = thinkEvery;
            Think(); // fire-and-forget: the tiger keeps moving while it thinks
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // --- Move based on the current plan ---
        if (mode == TigerMode.Chase)
        {
            // Run straight at the hero!
            MoveToward(player.position);
        }
        else if (mode == TigerMode.Circle)
        {
            // Strafe around the hero, waiting for a good moment.
            Vector3 toHero = (player.position - transform.position).normalized;
            toHero.y = 0f;
            Vector3 side = Vector3.Cross(Vector3.up, toHero); // perpendicular!
            Vector3 dir = (side * 0.8f + toHero * 0.3f).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        else // Retreat
        {
            // Back away! Toward the den if one is set, else just away from the hero.
            Vector3 away = (transform.position - player.position).normalized;
            away.y = 0f;
            Vector3 target = (den != null) ? den.position : transform.position + away * 5f;
            MoveToward(target);
        }

        // --- Attack, exactly like a normal Enemy ---
        if (dist <= attackRange && myAttackTimer <= 0f)
        {
            myAttackTimer = attackCooldown;
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.LoseHeart(damage);
        }
    }

    void MoveToward(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0f;
        transform.position += dir * moveSpeed * Time.deltaTime;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    // Ask Laya: "what should the tiger do right now?"
    // IMPORTANT: the situation is described in WORDS, not numbers.
    // (Laya is bad with numbers - "the hero is close" works, "dist: 3.2" doesn't!)
    async void Think()
    {
        thinking = true;
        try
        {
            float dist = Vector3.Distance(transform.position, player.position);

            var state = new Dictionary<string, object>
            {
                ["hero_distance"] = dist < 4f ? "the hero is very close" :
                                    dist < chaseRange ? "the hero is close" : "the hero is far away",
                ["tiger_health"] = hp * 2 > maxHP ? "the tiger is strong and healthy" :
                                   hp > 1 ? "the tiger is hurt" : "the tiger is almost beaten",
                ["hero_weapon"] = "the hero carries a stone sword"
            };

            var questions = new Dictionary<string, object>
            {
                ["move"] = LayaQ.Choice(
                    "What should the tiger do right now?",
                    new Dictionary<string, string>
                    {
                        ["chase"] = "run straight at the hero",
                        ["circle"] = "circle around the hero and wait for a good moment",
                        ["retreat"] = "back away from the hero toward safety"
                    })
            };

            JObject answers = await laya.AskAsync(state, questions, 250);

            if (answers != null)
            {
                // Laya answered! Do what it picked.
                string pick = (string)answers["move"]?["choice"];
                if (pick == "circle") mode = TigerMode.Circle;
                else if (pick == "retreat") mode = TigerMode.Retreat;
                else mode = TigerMode.Chase; // "chase" or anything unexpected
            }
            else
            {
                // Laya didn't answer (service not running?) -> normal scripted fallback:
                // chase when the hero is near, circle/wander otherwise.
                mode = dist < chaseRange ? TigerMode.Chase : TigerMode.Circle;
            }
        }
        finally
        {
            thinking = false;
        }
    }
}

// ============================================================
// LayaQ: tiny helpers that build Laya's typed questions.
// Plain Dictionaries on purpose - safe for IL2CPP builds.
// (Pattern from the Laya + Unity guide.)
// ============================================================
public static class LayaQ
{
    // "Pick one of these options."
    public static Dictionary<string, object> Choice(string instructions, IReadOnlyDictionary<string, string> options)
    {
        return new Dictionary<string, object>
        {
            ["type"] = "choice",
            ["instructions"] = instructions,
            ["criteria"] = options
        };
    }

    // "Rate the situation on this scale, low to high."
    public static Dictionary<string, object> Score(string instructions, params string[] levelsLowToHigh)
    {
        return new Dictionary<string, object>
        {
            ["type"] = "score",
            ["instructions"] = instructions,
            ["criteria"] = levelsLowToHigh
        };
    }

    // "Is this statement true? Answer with a yes/no probability."
    public static Dictionary<string, object> Noul(string statement, string ifTrue, string ifFalse)
    {
        return new Dictionary<string, object>
        {
            ["type"] = "noul",
            ["instructions"] = statement,
            ["criteria"] = new Dictionary<string, string> { ["true"] = ifTrue, ["false"] = ifFalse }
        };
    }
}

// ============================================================
// LayaClient: talks to the Laya AI brain over HTTP.
// One call, short timeout. Returns null on ANY failure -
// and the tiger treats null as "use normal behavior this time".
// ============================================================
public sealed class LayaClient
{
    private static readonly HttpClient Http = new HttpClient();
    private readonly string _endpoint;

    public LayaClient(string endpoint = "http://127.0.0.1:8000/v1/systemone")
    {
        _endpoint = endpoint;
    }

    // Sends {state, questions} to Laya.
    // Returns the "answers" object, or null if Laya didn't answer in time.
    public async Task<JObject> AskAsync(object state, Dictionary<string, object> questions, int timeoutMs = 250)
    {
        JObject body = new JObject
        {
            ["state"] = JToken.FromObject(state),
            ["questions"] = JToken.FromObject(questions)
        };

        using (var cts = new CancellationTokenSource(timeoutMs))
        using (var content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json"))
        {
            try
            {
                using (var resp = await Http.PostAsync(_endpoint, content, cts.Token))
                {
                    if (!resp.IsSuccessStatusCode) return null;
                    string json = await resp.Content.ReadAsStringAsync();
                    JObject root = JObject.Parse(json);
                    return root["answers"] as JObject;
                }
            }
            catch
            {
                return null; // timeout, no service, bad JSON... -> scripted fallback
            }
        }
    }
}
