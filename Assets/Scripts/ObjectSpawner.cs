using UnityEngine;

namespace CosmicBatter
{
    public class ObjectSpawner : MonoBehaviour
    {
        public GameManager game;
        [Tooltip("Milk, Sugar, Egg, original Bird (kept for editing), Asteroid")]
        public FallingObject[] prefabs;
        public FallingObject ticketPrefab, pigeonPrefab;
        [Header("Ingredients and occasional rocks")]
        [Min(.1f)] public float initialInterval=.7f, minimumInterval=.55f;
        [Range(0,1)] public float collectibleChance=.9f;
        public Vector2 fallSpeed=new Vector2(2,3);
        [Header("Enemy stages: 0, 15, 30, 45 seconds")]
        public float[] stageIntervals={1.7f,1.4f,1.1f,.85f};
        [Range(0,1)] public float pigeonChance=.3f, finalPigeonChance=.65f;
        [Min(1)] public int maxEnemies=22;
        [Min(.5f)] public float safeSpawnDistance=2.4f;
        float countdown, waveCountdown;
        public int CurrentStage => Mathf.Clamp(Mathf.FloorToInt((game.roundSeconds-game.Remaining)/15f),0,3);
        public float WaveInterval(float progress)
        {
            int stage=Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(progress)*4),0,3);
            return stageIntervals!=null && stageIntervals.Length>stage ? Mathf.Max(.2f,stageIntervals[stage]) : 1;
        }
        public int WaveSize(float progress) => 1;
        public void ResetSpawner() { countdown=.3f; waveCountdown=1; }
        void Update()
        {
            if (!game.Playing) return;
            countdown-=Time.deltaTime; waveCountdown-=Time.deltaTime;
            if (countdown<=0)
            {
                countdown=Mathf.Lerp(initialInterval,minimumInterval,CurrentStage/3f);
                // Half of ingredient rolls are milk, ensuring regular score pickups.
                float roll=Random.value;
                int index=Random.value>collectibleChance ? 4 : roll<.5f ? 0 : roll<.75f ? 1 : 2;
                if (prefabs!=null && prefabs.Length>index) Spawn(prefabs[index],Random.Range(fallSpeed.x,fallSpeed.y));
            }
            if (waveCountdown<=0)
            {
                waveCountdown=stageIntervals!=null && stageIntervals.Length>CurrentStage ? Mathf.Max(.2f,stageIntervals[CurrentStage]) : 1;
                if (FallingObject.ActiveEnemies<maxEnemies)
                    Spawn(Random.value<(CurrentStage==3 ? finalPigeonChance : pigeonChance) ? pigeonPrefab : ticketPrefab,1);
            }
        }
        void Spawn(FallingObject prefab,float speed)
        {
            if (!prefab) return;
            var bounds=game.player.MovementBounds;
            float y=bounds.yMax-.35f;
            for(int attempt=0;attempt<12;attempt++)
            {
                var position=new Vector2(Random.Range(bounds.xMin+.4f,bounds.xMax-.4f),y);
                if (Vector2.Distance(position,game.player.transform.position)<safeSpawnDistance) continue;
                if (Physics2D.OverlapCircle(position,.75f)) continue;
                var item=Instantiate(prefab,position,Quaternion.identity,transform);
                item.Initialize(game,speed);
                return;
            }
            // A crowded spawn row skips one spawn instead of overlapping a player/enemy.
        }
    }
}
