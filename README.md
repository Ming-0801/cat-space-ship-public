# Cosmic Batter — Space Cat Arcade Shooter

A fast-paced, 60-second Unity 2D arcade shooter featuring a space cat, collectible ingredients, temporary power-ups, and enemies that chase and dash toward the player.

## Download and play

1. Click **Code → Download ZIP** on GitHub and extract the archive, or clone this repository.
2. In Unity Hub, add the extracted folder containing `Assets`, `Packages`, and `ProjectSettings`.
3. Open the project with **Unity 6000.0.66f2** and wait for the initial import and compilation.
4. Open `Assets/Scenes/CosmicBatter.unity`, press **Play**, then click **LET'S BAKE!**.
5. Click the Game view to focus it. Use **WASD or the arrow keys to move** and **hold SPACE to shoot**.

This repository contains editable Unity project files, not a browser-playable build. The scene and references are already configured; no setup tool is required to play.

After 60 seconds, the results screen displays your score, rank, and a **PLAY AGAIN** button. Restarting clears enemies, bullets, and all temporary effects.

If an open Editor still shows an older scene, save any changes you want to keep to a separate scene before reopening `CosmicBatter`.

## Gameplay

- **Milk:** Grants **+10 points** with floating score text. Spawns regularly.
- **Egg:** Increases movement speed by **30% for 5 seconds**. Another egg refreshes the duration without stacking the bonus. Eggs do not award points.
- **Sugar:** Activates **three-projectile spread fire for 2 seconds** and reduces the firing interval from **0.12 seconds to 0.065 seconds**. Another sugar pickup refreshes the duration without stacking the effect. Sugar does not award points.
- **Speeding Ticket:** Slowly follows the player. Contact reduces movement speed by **40% for 3 seconds**, then the ticket disappears. Further ticket hits refresh the slow without stacking it or deducting points. Shooting a ticket awards **+10 points**.
- **Evil Pigeon:** Follows faster and periodically dashes in a straight line toward the player's position. Dashes last **0.35 seconds**, with a **2.8-second interval** and a small randomized initial delay. Contact deducts **50 points**. A shared **1.5-second pigeon collision cooldown** prevents several pigeons from applying simultaneous penalties. Pigeons remain after contact and can still be shot for **+20 points**.
- **Asteroid:** Contact deducts **30 points**, subject to the existing damage cooldown. Asteroids block bullets and cannot be destroyed.
- Bullets pass through ingredients. The score never drops below zero.

Speed boosts and slows have independent timers. When both are active, their multipliers combine: `1.3 × 0.6 = 0.78` of normal movement speed. Each effect removes only its own modifier when it expires.

The HUD shows remaining effect durations in green for speed, yellow for heavy firepower, and blue for slowing. Player tint changes, small bursts, and floating score numbers provide additional feedback.

## Enemy progression

Enemy spawning increases in four stages:

- **0–15 seconds:** One enemy every 1.7 seconds.
- **15–30 seconds:** One enemy every 1.4 seconds.
- **30–45 seconds:** One enemy every 1.1 seconds.
- **45–60 seconds:** One enemy every 0.85 seconds; pigeon probability increases from 30% to 65%.

At most **22 enemies** can be active. Objects spawn near the top of the playable area, away from the player and existing colliders. A spawn is skipped if no suitable position is available.

## Where to make changes

Most gameplay values can be adjusted in the **Inspector** without editing code.

- **`PlayerController.cs` / Player object:** Movement speed, firing intervals, spread angle, and temporary-effect strengths and durations.
- **`FallingObject.cs` / Item and enemy prefabs:** `Touch Effect` selects the contact effect; `Movement` selects falling, following, or dashing. `Points` controls contact scoring; `Kill Points` controls the reward for shooting an enemy.
- **`ObjectSpawner.cs` / Spawner object:** Four `Stage Intervals`, pigeon probabilities, `Max Enemies`, and ingredient spawn intervals and falling speeds.
- **`Bullet.cs` / `PlayerBullet.prefab`:** Bullet speed, hit radius, and appearance. Collision checks sweep along the bullet's path to avoid skipping targets at high speed.
- **`GameManager.cs`:** Round duration, scoring, ranks, ending the game, and restarting.
- **`GameUI.cs` / Canvas:** Start and results screens, score, timer, and temporary-effect indicators.
- **`ScoreEffect.cs`:** Floating score text and small explosions.

Suggested team split: one person handles the player and bullets, another handles enemies and spawning, and another handles art and UI. Avoid editing the same `.unity` scene simultaneously. Always keep assets and scripts together with their `.meta` files.

This is a public repository: anyone can view and download it. Direct push access requires the owner to add you as a collaborator; otherwise, use a fork and pull request.

## Artwork and prefabs

The supplied enemy artwork is in `Assets/Art/SpeedingTicket.png` and `Assets/Art/EvilPigeon.png`. Image proportions are preserved. The existing `PlayerShip.png`, `DonutBird.png`, and other original artwork remain in the project.

`SpeedingTicket.prefab` and `EvilPigeon.prefab` reuse `FallingObject`, kinematic `Rigidbody2D` components, and trigger colliders. The older `DonutBird` prefab is retained, but the current spawner uses the new enemies.

To replace artwork:

1. Import a PNG and set **Texture Type → Sprite (2D and UI)**.
2. Assign it to the prefab's **Sprite Renderer → Sprite** field.
3. Keep X and Y scaling proportional, and adjust the collider radius to fit the visible artwork.

## Editor tools

- **Tools → Cosmic Batter → Update Items And Enemies:** Applies the item and enemy configuration to the existing scene. It can be run again without duplicating HUD indicators, but it restores this version's default balance settings. Do not rerun it after tuning values unless you intend to reset them.
- **Build Prototype:** The original full rebuild tool. It overwrites the prototype scene and item prefabs. For normal editing, open the existing scene instead.

## Validation

The project compiled successfully in Unity **6000.0.66f2**. Automated Play Mode checks in a separate copy, without graphics, covered:

- Item collision triggers, effect refresh and expiry, and simultaneous speed boosts and slows.
- Three-projectile spread fire and restoration of normal shooting.
- Shared pigeon collision cooldown and both enemy kill rewards.
- Simulated WASD/arrow-key movement, continuous shooting, and movement boundaries.
- Pigeon following and dash activation, stage intervals, and spawning.
- Restart and round-end cleanup.

The round-end test used a shortened duration; the delivered scene remains set to **60 seconds**. The validation copy used locally available Input System and UGUI packages. The original project's package and rendering settings were preserved.

Manual visual review, frame-rate testing, and Web export testing have **not** been completed. Before presenting the game, play a full round in a **16:9 Game view** and check enemy artwork, effect indicators, and the difficulty of the final 15 seconds.

## Web export

1. In Unity Hub, install **Web Build Support** for the matching Editor version.
2. Open **File → Build Profiles → Web**.
3. Add `CosmicBatter` to the scene list and enable only that scene.
4. Switch platform and build.
5. Serve the exported files through an HTTP server or a game-hosting platform; do not open the HTML file directly by double-clicking it.
