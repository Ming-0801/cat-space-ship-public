using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CosmicBatter
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        public GameManager game;
        [Min(0)] public float moveSpeed = 9;
        [Min(0)] public float damageCooldown = .65f;
        public float halfWidth = .6f;
        [Header("Shooting")]
        public Bullet bulletPrefab;
        [Min(.04f)] public float fireInterval = .12f;
        public Vector2 muzzleOffset = new Vector2(0,.65f);
        [Header("Temporary effects - refresh, never stack")]
        public float speedBonus = .3f, speedDuration = 5;
        public float slowPenalty = .4f, slowDuration = 3;
        public float heavyDuration = 2, heavyFireInterval = .065f, spreadAngle = 16;
        public float pigeonCollisionCooldown = 1.5f;
        float nextShot, protectedUntil, pigeonProtectedUntil;
        float speedUntil, slowUntil, heavyUntil;
        Vector2 input;
        Rigidbody2D body;
        SpriteRenderer art;
        public float SpeedRemaining => Mathf.Max(0, speedUntil - Time.time);
        public float SlowRemaining => Mathf.Max(0, slowUntil - Time.time);
        public float HeavyRemaining => Mathf.Max(0, heavyUntil - Time.time);
        public bool HeavyActive => HeavyRemaining > 0;
        public float CurrentMoveSpeed => moveSpeed * (SpeedRemaining > 0 ? 1 + speedBonus : 1) * (SlowRemaining > 0 ? 1 - slowPenalty : 1);
        public Rect MovementBounds
        {
            get
            {
                var camera = Camera.main;
                var center = (Vector2)camera.transform.position;
                float width = Mathf.Max(.1f, camera.orthographicSize * camera.aspect - halfWidth);
                return Rect.MinMaxRect(center.x-width,center.y-camera.orthographicSize+.65f,center.x+width,center.y+camera.orthographicSize-1);
            }
        }
        void Awake() { body = GetComponent<Rigidbody2D>(); art = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            input = Vector2.zero;
            bool shooting = false;
            if (game.Playing)
            {
#if ENABLE_INPUT_SYSTEM
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1;
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1;
                    if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1;
                    if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1;
                    shooting = keyboard.spaceKey.isPressed;
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
                shooting = Input.GetKey(KeyCode.Space);
#endif
            }
            input = Vector2.ClampMagnitude(input,1);
            if (shooting) TryShoot();
            Color tint = SlowRemaining > 0 ? new Color(.5f,.7f,1) : SpeedRemaining > 0 ? new Color(.6f,1,.6f) : HeavyActive ? new Color(1,.8f,.45f) : Color.white;
            art.color = Time.time < protectedUntil ? Color.Lerp(tint,new Color(1,.2f,.3f),.5f+.5f*Mathf.Sin(Time.time*40)) : tint;
        }
        public bool TryShoot()
        {
            if (!game.Playing || !bulletPrefab || Time.time < nextShot) return false;
            nextShot = Time.time + Mathf.Max(.04f,HeavyActive ? heavyFireInterval : fireInterval);
            Fire(0);
            if (HeavyActive) { Fire(-spreadAngle); Fire(spreadAngle); }
            return true;
        }
        void Fire(float angle)
        {
            var bullet = Instantiate(bulletPrefab,transform.position+(Vector3)muzzleOffset,Quaternion.Euler(0,0,angle));
            bullet.Initialize(game);
        }
        void FixedUpdate()
        {
            if (!game.Playing) return;
            var bounds = MovementBounds;
            Vector2 position = body.position + input * CurrentMoveSpeed * Time.fixedDeltaTime;
            body.MovePosition(new Vector2(Mathf.Clamp(position.x,bounds.xMin,bounds.xMax),Mathf.Clamp(position.y,bounds.yMin,bounds.yMax)));
        }
        public void ApplySpeedBoost() { speedUntil=Time.time+speedDuration; ScoreEffect.Burst(transform.position,Color.green); }
        public void ApplySlow() { slowUntil=Time.time+slowDuration; ScoreEffect.Burst(transform.position,new Color(.4f,.7f,1)); }
        public void ApplyHeavyFire() { heavyUntil=Time.time+heavyDuration; nextShot=Mathf.Min(nextShot,Time.time); ScoreEffect.Burst(transform.position,new Color(1,.7f,.2f)); }
        public bool TryHit()
        {
            if (Time.time < protectedUntil) return false;
            protectedUntil = Time.time + damageCooldown;
            return true;
        }
        public bool TryPigeonHit()
        {
            if (Time.time < pigeonProtectedUntil) return false;
            pigeonProtectedUntil=Time.time+pigeonCollisionCooldown;
            protectedUntil=Mathf.Max(protectedUntil,Time.time+.25f);
            return true;
        }
        public void ClearEffects() { speedUntil=slowUntil=heavyUntil=protectedUntil=pigeonProtectedUntil=nextShot=0; }
        public void ResetPlayer()
        {
            if (!body) body = GetComponent<Rigidbody2D>();
            if (!art) art = GetComponent<SpriteRenderer>();
            input=Vector2.zero; ClearEffects(); body.position=new Vector2(0,-3.7f); art.color=Color.white;
        }
    }
}
