using UnityEngine;

namespace CosmicBatter
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class FallingObject : MonoBehaviour
    {
        public enum TouchEffect { Score, SpeedBoost, HeavyFire, TicketSlow, PigeonPenalty }
        public enum Movement { Falling, Follow, Dash }
        public TouchEffect touchEffect;
        public Movement movement;
        [Header("Simple enemy movement")]
        public float followSpeed=1.1f, dashSpeed=5.5f, dashInterval=2.8f, dashDuration=.35f;
        float nextDash, dashUntil;
        Vector2 dashDirection;
        public bool IsDashing => movement==Movement.Dash && Time.time<dashUntil;
        public int points = 10;
        [Tooltip("Birds: one bullet destroys them. Ingredients ignore bullets.")]
        public bool isEnemy;
        [Min(0)] public int killPoints = 25;
        public bool blocksBullets;
        public static int ActiveEnemies { get; private set; }
        [Min(.1f)] public float speedMultiplier = 1;
        GameManager game;
        Rigidbody2D body;
        float speed;
        bool consumed;
        void OnEnable() { if (isEnemy) ActiveEnemies++; }
        void OnDisable() { if (isEnemy) ActiveEnemies = Mathf.Max(0, ActiveEnemies - 1); }
        public bool HitByBullet()
        {
            if (consumed || !game || !game.Playing || (!isEnemy && !blocksBullets)) return false;
            if (isEnemy)
            {
                consumed = true;
                game.EnemyDestroyed(transform.position, killPoints);
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            else ScoreEffect.Burst(transform.position, new Color(.65f,.8f,1));
            return true;
        }
        public void Initialize(GameManager manager, float fallSpeed)
        { game = manager; speed = fallSpeed * speedMultiplier; body = GetComponent<Rigidbody2D>(); nextDash=Time.time+dashInterval+Random.Range(0,.5f); }
        void FixedUpdate()
        {
            if (!game || !game.Playing) return;
            Vector2 velocity=Vector2.down*speed;
            if (movement!=Movement.Falling)
            {
                Vector2 toward=(Vector2)game.player.transform.position-body.position;
                velocity=toward.normalized*followSpeed;
                if (movement==Movement.Dash)
                {
                    if (Time.time>=nextDash)
                    {
                        dashDirection=toward.normalized; dashUntil=Time.time+dashDuration; nextDash=Time.time+dashInterval;
                        ScoreEffect.Burst(transform.position,new Color(1,.4f,.6f));
                    }
                    if (Time.time<dashUntil) velocity=dashDirection*dashSpeed;
                }
            }
            Vector2 position=body.position+velocity*Time.fixedDeltaTime;
            if (movement!=Movement.Falling)
            {
                var bounds=game.player.MovementBounds;
                position=new Vector2(Mathf.Clamp(position.x,bounds.xMin,bounds.xMax),Mathf.Clamp(position.y,bounds.yMin,bounds.yMax));
            }
            body.MovePosition(position);
            if (body.position.y < Camera.main.transform.position.y-Camera.main.orthographicSize-1) Destroy(gameObject);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || !game || !other.GetComponent<PlayerController>()) return;
            if (touchEffect==TouchEffect.PigeonPenalty) { game.Collect(this); return; }
            consumed = true;
            game.Collect(this);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        void OnTriggerStay2D(Collider2D other)
        {
            if (!consumed && game && touchEffect==TouchEffect.PigeonPenalty && other.GetComponent<PlayerController>()) game.Collect(this);
        }
    }
}
