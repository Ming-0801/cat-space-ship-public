using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace CosmicBatter.Editor
{
    // Batch-only regression check. Never starts during ordinary project editing.
    [InitializeOnLoad]
    public static class CosmicBatterSmokeCheck
    {
        const string SessionKey="CosmicPowerupCheck";
        static int stage;
        static double next,deadline;
        static Keyboard keyboard;
        static Vector3 previous;
        static float dashStart;
        static FallingObject tracked;
        static bool observedDash;
        static CosmicBatterSmokeCheck() { EditorApplication.update+=Tick; }
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/CosmicBatter.unity");
            SessionState.SetBool(SessionKey,true); EditorApplication.EnterPlaymode();
        }
        static void Check(bool value,string message) { if(!value) throw new Exception(message); }
        static FallingObject Spawn(GameManager g,FallingObject prefab,Vector3 position)
        {
            var item=UnityEngine.Object.Instantiate(prefab,position,Quaternion.identity);
            item.Initialize(g,.01f); return item;
        }
        static void Press(params UnityEngine.InputSystem.Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); }
        static void Tick()
        {
            if(!SessionState.GetBool(SessionKey,false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            double now=EditorApplication.timeSinceStartup;
            if(tracked && tracked.IsDashing) observedDash=true;
            if(deadline==0) deadline=now+90;
            if(now<next) return;
            try
            {
                Check(now<deadline,"Test timed out");
                var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
                Check(g && g.player && g.ui && g.spawner,"Missing scene references");
                var p=g.player;
                switch(stage)
                {
                    case 0:
                        Check(!g.Playing && !p.TryShoot(),"Shooting before start");
                        Check(g.roundSeconds==60,"Round is not 60 seconds");
                        g.ui.startButton.onClick.Invoke(); g.spawner.enabled=false;
                        Spawn(g,g.spawner.prefabs[0],p.transform.position); next=now+.2; break;
                    case 1:
                        Check(g.Score==10,"Milk must give +10 exactly once");
                        Spawn(g,g.spawner.prefabs[2],p.transform.position); next=now+.2; break;
                    case 2:
                        Check(g.Score==10 && p.SpeedRemaining>4.5f && Mathf.Abs(p.CurrentMoveSpeed-p.moveSpeed*1.3f)<.01f,"Egg effect incorrect");
                        Spawn(g,g.spawner.prefabs[1],p.transform.position); next=now+.2; break;
                    case 3:
                        Check(g.Score==10 && p.HeavyActive && g.ui.heavyText.gameObject.activeSelf,"Sugar/UI incorrect");
                        Check(p.TryShoot(),"Heavy shot failed");
                        Check(UnityEngine.Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length==3,"Heavy shot must have three bullets");
                        var bullets=UnityEngine.Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);
                        Check(Array.Exists(bullets,b=>b.transform.up.x<-.1f) && Array.Exists(bullets,b=>b.transform.up.x>.1f),"Spread missing");
                        next=now+1.0; break;
                    case 4:
                        Spawn(g,g.spawner.prefabs[1],p.transform.position); Spawn(g,g.spawner.prefabs[2],p.transform.position); next=now+.2; break;
                    case 5:
                        Check(p.HeavyRemaining>1.7f && p.SpeedRemaining>4.7f,"Pickup refresh failed");
                        Check(Mathf.Abs(p.CurrentMoveSpeed-p.moveSpeed*1.3f)<.01f,"Speed bonus stacked");
                        next=now+1.6; break;
                    case 6:
                        Check(p.HeavyActive,"Heavy fire ended too early"); next=now+.5; break;
                    case 7:
                        Check(!p.HeavyActive && !g.ui.heavyText.gameObject.activeSelf,"Heavy fire did not expire after 2 seconds");
                        Check(p.TryShoot(),"Normal shot failed");
                        Check(UnityEngine.Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length==1,"Normal fire not restored");
                        g.BeginRound(); Spawn(g,g.spawner.ticketPrefab,p.transform.position); next=now+.2; break;
                    case 8:
                        Check(g.Score==0 && p.SlowRemaining>2.7f && Mathf.Abs(p.CurrentMoveSpeed-p.moveSpeed*.6f)<.01f,"Ticket slow incorrect");
                        Check(g.ui.slowText.gameObject.activeSelf,"Slow indicator missing");
                        next=now+.8; break;
                    case 9:
                        Spawn(g,g.spawner.ticketPrefab,p.transform.position); p.ApplySpeedBoost(); next=now+.2; break;
                    case 10:
                        Check(p.SlowRemaining>2.7f && Mathf.Abs(p.CurrentMoveSpeed-p.moveSpeed*.78f)<.01f,"Refresh/combined speed effects incorrect");
                        next=now+3.1; break;
                    case 11:
                        Check(p.SlowRemaining==0 && p.SpeedRemaining>0 && Mathf.Abs(p.CurrentMoveSpeed-p.moveSpeed*1.3f)<.01f,"Slow expiry corrupts boost");
                        next=now+2.1; break;
                    case 12:
                        Check(p.SpeedRemaining==0 && Mathf.Abs(p.CurrentMoveSpeed-p.moveSpeed)<.01f,"Speed expiry failed");
                        g.BeginRound();
                        for(int i=0;i<10;i++) g.Collect(g.spawner.prefabs[0]);
                        for(int i=0;i<2;i++) { var bird=Spawn(g,g.spawner.pigeonPrefab,p.transform.position); bird.followSpeed=0; bird.dashInterval=100; }
                        next=now+.2; break;
                    case 13:
                        Check(g.Score==50,"Simultaneous pigeons bypassed cooldown"); next=now+.8; break;
                    case 14:
                        Check(g.Score==50,"Pigeon cooldown shorter than 1.5s"); next=now+.7; break;
                    case 15:
                        Check(g.Score==0,"Pigeon contact did not repeat after cooldown");
                        g.BeginRound(); tracked=Spawn(g,g.spawner.ticketPrefab,new Vector3(0,-1,0)); p.TryShoot(); next=now+.4; break;
                    case 16:
                        Check(g.Score==10 && !tracked,"Ticket kill must give +10");
                        tracked=Spawn(g,g.spawner.pigeonPrefab,new Vector3(0,-1,0)); p.TryShoot(); next=now+.4; break;
                    case 17:
                        Check(g.Score==30 && !tracked,"Pigeon kill must give +20");
                        g.BeginRound();
                        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                        keyboard=InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent(); previous=p.transform.position;
                        Press(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.D,UnityEngine.InputSystem.Key.Space); next=now+.35; break;
                    case 18:
                        Check(p.transform.position.x>previous.x+.5f && p.transform.position.y>previous.y+.5f,"W/D movement failed");
                        Check(UnityEngine.Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length>=2,"Held Space fire failed");
                        previous=p.transform.position; Press(UnityEngine.InputSystem.Key.LeftArrow,UnityEngine.InputSystem.Key.DownArrow); next=now+.3; break;
                    case 19:
                        Check(p.transform.position.x<previous.x-.5f && p.transform.position.y<previous.y-.5f,"Arrow movement failed");
                        Press(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.D); next=now+2; break;
                    case 20:
                        var bounds=p.MovementBounds; Check(p.transform.position.x<=bounds.xMax+.01f && p.transform.position.y<=bounds.yMax+.01f,"Player escaped bounds");
                        Press(); g.BeginRound(); observedDash=false; tracked=Spawn(g,g.spawner.pigeonPrefab,new Vector3(3,1,0)); previous=tracked.transform.position; dashStart=Time.time; next=now+.5; break;
                    case 21:
                        Check(Vector2.Distance(tracked.transform.position,p.transform.position)<Vector2.Distance(previous,p.transform.position),"Pigeon does not follow");
                        next=now+3; break;
                    case 22:
                        Check(tracked && observedDash && tracked.dashSpeed>tracked.followSpeed && Time.time-dashStart>tracked.dashInterval,"Pigeon dash never executed");
                        g.BeginRound(); p.ApplyHeavyFire(); p.ApplySlow(); p.ApplySpeedBoost(); p.TryShoot(); g.BeginRound();
                        Check(p.SpeedRemaining==0 && p.SlowRemaining==0 && p.HeavyRemaining==0 && UnityEngine.Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length==0,"Restart leaves effects/bullets");
                        Check(g.spawner.WaveInterval(.24f)>g.spawner.WaveInterval(.25f) && g.spawner.WaveInterval(.5f)>g.spawner.WaveInterval(.75f),"15-second progression failed");
                        Check(g.spawner.finalPigeonChance>g.spawner.pigeonChance,"Final pigeon ratio not increased");
                        g.spawner.enabled=true; next=now+2; break;
                    case 23:
                        Check(UnityEngine.Object.FindObjectsByType<FallingObject>(FindObjectsSortMode.None).Length>0,"Spawning failed");
                        Check(FallingObject.ActiveEnemies<=g.spawner.maxEnemies,"Enemy cap exceeded");
                        g.roundSeconds=.3f; g.BeginRound(); p.ApplyHeavyFire(); p.TryShoot(); next=now+.6; break;
                    case 24:
                        Check(!g.Playing && g.ui.resultPanel.activeSelf && g.Remaining==0 && !p.TryShoot(),"Timer/result failed");
                        Check(p.HeavyRemaining==0 && UnityEngine.Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length==0 && FallingObject.ActiveEnemies==0,"End cleanup failed");
                        InputSystem.RemoveDevice(keyboard);
                        Debug.Log("COSMIC_POWERUP_TEST_PASS: pickup triggers, refresh/expiry, independent speed effects, triple spread, pigeon cooldown, both enemy kill rewards, keyboard movement+shooting, bounds, progression, spawning, restart and timeout.");
                        SessionState.SetBool(SessionKey,false); EditorApplication.Exit(0); return;
                }
                stage++;
            }
            catch(Exception ex) { Debug.LogException(ex); SessionState.SetBool(SessionKey,false); EditorApplication.Exit(1); }
        }
    }
}
