using UnityEngine;

namespace CosmicBatter
{
    // Sweeps the path each physics step so fast bullets cannot skip thin targets.
    public class Bullet : MonoBehaviour
    {
        [Min(1)] public float speed = 18;
        [Min(.01f)] public float hitRadius = .1f;
        GameManager game;
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        public void Initialize(GameManager manager) { game = manager; }
        void FixedUpdate()
        {
            if (!game || !game.Playing) { Destroy(gameObject); return; }
            float distance = speed * Time.fixedDeltaTime;
            var filter = new ContactFilter2D { useTriggers = true };
            int count = Physics2D.CircleCast(transform.position, hitRadius, transform.up, filter, hits, distance);
            for (int i=0;i<count;i++)
            {
                var target = hits[i].collider.GetComponent<FallingObject>();
                if (!target || !target.HitByBullet()) continue;
                gameObject.SetActive(false); Destroy(gameObject); return;
            }
            transform.position += transform.up * distance;
            var camera = Camera.main;
            var delta = transform.position-camera.transform.position;
            if (Mathf.Abs(delta.y)>camera.orthographicSize+1 || Mathf.Abs(delta.x)>camera.orthographicSize*camera.aspect+1) Destroy(gameObject);
        }
    }
}
