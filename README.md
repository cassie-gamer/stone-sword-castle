# Stone Sword Castle

You wake up in a giant castle. There's a chest... inside is a STONE SWORD! Fight through 5 stages and defeat the boss (25 hits!) to win. VICTORY!

## The 5 stages

- **Wake up room**: open the chest (walk up, press **E**) to get the stone sword
- **Stage 1**: 3 masked mice attack!
- **Stage 2**: 3 snakes slither in!
- **Stage 3**: 1 huge tiger!
- **Stage 4**: 1 bear blocks your path!
- **Stage 5**: THE BOSS — takes **25 sword hits**! Watch out, it charges!

Defeat the boss and the screen says **VICTORY!**

## Controls

- Move: **WASD** / arrow keys
- Swing sword: **SPACE**
- Open chest: **E** (stand close to the chest)

## Setup (Windows, Unity)

1. Install Unity Hub + latest LTS with Windows Build Support
2. New Project → **3D (Core)**, name it `StoneSwordCastle`
3. Copy all `.cs` files into `Assets/Scripts/`
4. Tag the player as `Player` (Inspector → Tag)
5. Player needs: `CastlePlayer` + `StoneSword` + `PlayerHealth` + Rigidbody + Collider

### Scene building

**Wake-up room:**
- Floor plane, 4 walls (cubes), a bed (small cube) where you wake up
- Treasure chest: box + lid (separate cube as `lid`) + tiny sword model inside
  - Add `TreasureChest`, drag lid + sword model into its slots
  - Chest needs a trigger Collider
- Empty `SpawnPoint` at the bed, drag into PlayerHealth

**Stage groups (the key part!):**
- Create 5 empty GameObjects: `Stage1` … `Stage5`. Set them all **inactive**.
- Stage1: 3 mice (small spheres/capsules) each with `Enemy` (HP 2, speed 3.5)
- Stage2: 3 snakes (long thin capsules) each with `Enemy` (HP 3, speed 3)
- Stage3: 1 tiger (big orange box) with `Enemy` (HP 6, speed 4.5)
- Stage4: 1 bear (big brown box) with `Enemy` (HP 10, speed 3.5)
- Stage5: 1 boss (huge dark-red box) with `Enemy` (HP **25**, speed 4) + `BossCharge`
- All enemies need trigger Colliders
- Drag Stage1..Stage5 into StageManager's **Stage Groups** array in order

**Managers & UI:**
- Empty `StageManager` + `StageManager` script
- Canvas: `MessageText` (story messages), `StageText` (Stage X / 5), `HeartsText`, `VictoryText` ("VICTORY!", inactive)
- Wire the UI slots in the Inspector

**Camera:** position (0, 12, -10), rotation X 50 — top-down view of the room.

Press Play! Open the chest, then survive all 5 stages!

## Scripts

- `CastlePlayer.cs` — WASD movement, SPACE to swing (needs the sword first)
- `StoneSword.cs` — arc attack in front of the player, 0.5s cooldown
- `TreasureChest.cs` — press E nearby: lid pops, you get the stone sword
- `Enemy.cs` — chases and attacks; tune HP/speed per animal (mice 2HP → boss 25HP)
- `BossCharge.cs` — boss's charging attack every few seconds
- `PlayerHealth.cs` — hearts; lose all and you wake up back at the start
- `StageManager.cs` — runs the 5 stages, counts enemies, shows VICTORY!
- `TigerSmartAI.cs` — **optional!** gives the stage-3 tiger its own AI brain (see below)

## Smart AI (Laya) — optional ✨

The stage-3 tiger can **think for itself**! Instead of always chasing you, it asks a free AI brain called **Laya** (running on your own PC, no internet needed): *"Should I chase, circle around, or retreat?"* — and then does what the AI picks. A tiger that gets scared and runs away when it's almost beaten!

**You don't need this to play.** If Laya isn't running, the tiger just uses its normal behavior. Nothing breaks.

### Want to try it? (Windows PC)

1. Install Python, then in a terminal run:
   `pip install "laya[serve]"`
2. Start the AI brain (leave this window open while you play):
   `laya-serve`
3. In Unity, add the JSON helper package: **Window → Package Manager → "+" → "Add package by name..."** → type `com.unity.nuget.newtonsoft-json`
4. In your scene, find the stage-3 tiger. **Remove** its `Enemy` script and **add** `TigerSmartAI` instead. Re-enter its stats: HP 6, speed 4.5 (same as before).
   - Bonus: make an empty GameObject called `Den`, place it somewhere safe, and drag it into the tiger's **Den** slot — that's where it retreats to!
5. Press Play! Every second the tiger describes the fight in words and the AI picks its move.

### How it works (the simple version)

- The tiger turns what it sees into **words**: "the hero is close", "the tiger is hurt".
- It sends those words + one question (*chase? circle? retreat?*) to Laya at `http://127.0.0.1:8000/v1/systemone`.
- Laya answers in ~20 milliseconds. If it doesn't answer in 250ms, the tiger shrugs and uses its normal behavior that tick.
- Tip from the Laya guide: describe things in words, not numbers — *"the hero is close"* works way better than *"distance: 3.2"*!

Want the full guide? https://www.guardingpearsoftware.com/blog/how-to-use-laya-for-local-game-ai-in-unity-52316

## Build for Windows

File → Build Settings → Add Open Scenes → PC, Mac & Linux Standalone → Windows x86_64 → Build!
