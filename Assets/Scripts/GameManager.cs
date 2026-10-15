using UnityEngine;

namespace CosmicBatter
{
    public class GameManager : MonoBehaviour
    {
        [Min(1)] public float roundSeconds = 60;
        public int rankB = 200, rankA = 400, rankS = 700;
        public PlayerController player;
        public ObjectSpawner spawner;
        public GameUI ui;
        public bool Playing { get; private set; }
        public int Score { get; private set; }
        public float Remaining { get; private set; }
        public string Rank => Score >= rankS ? "S" : Score >= rankA ? "A" : Score >= rankB ? "B" : "C";
        void Start() { Remaining = roundSeconds; ui.ShowStart(); }
        void Update()
        {
            if (!Playing) return;
            Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
            ui.Refresh();
            if (Remaining > 0) return;
            Playing = false;
            ClearObjects();
            player.ClearEffects();
            ui.ShowResult();
        }
        public void BeginRound()
        {
            Playing = false;
            ClearObjects();
            Score = 0;
            Remaining = roundSeconds;
            player.ResetPlayer();
            spawner.ResetSpawner();
            Playing = true;
            ui.ShowGame();
        }
        void ClearObjects()
        {
            foreach (var bullet in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            { bullet.gameObject.SetActive(false); Destroy(bullet.gameObject); }
            foreach (var item in FindObjectsByType<FallingObject>(FindObjectsSortMode.None))
            { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            foreach (var effect in FindObjectsByType<ScoreEffect>(FindObjectsSortMode.None))
                Destroy(effect.gameObject);
        }
        public void Collect(FallingObject item)
        {
            if (!Playing) return;
            switch(item.touchEffect)
            {
                case FallingObject.TouchEffect.SpeedBoost: player.ApplySpeedBoost(); ui.Refresh(); return;
                case FallingObject.TouchEffect.HeavyFire: player.ApplyHeavyFire(); ui.Refresh(); return;
                case FallingObject.TouchEffect.TicketSlow: player.ApplySlow(); ui.Refresh(); return;
                case FallingObject.TouchEffect.PigeonPenalty: if (!player.TryPigeonHit()) return; break;
                default: if(item.points<0 && !player.TryHit()) return; break;
            }
            Score = Mathf.Max(0, Score + item.points);
            ScoreEffect.Create(item.transform.position, item.points);
            ui.Refresh();
        }
        public void EnemyDestroyed(Vector3 position, int reward)
        {
            if (!Playing) return;
            Score += Mathf.Max(0,reward);
            ScoreEffect.Create(position,reward);
            ScoreEffect.Burst(position,new Color(1,.65f,.15f));
            ui.Refresh();
        }
    }
}
