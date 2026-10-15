using UnityEngine;

namespace CosmicBatter
{
    public class ScoreEffect : MonoBehaviour
    {
        float age;
        TextMesh label;
        static Sprite sparkSprite;
        public static void Burst(Vector3 position, Color color)
        {
            if (!sparkSprite)
            {
                var texture = new Texture2D(1,1); texture.SetPixel(0,0,Color.white); texture.Apply();
                sparkSprite = Sprite.Create(texture,new Rect(0,0,1,1),Vector2.one*.5f,1);
            }
            var root = new GameObject("Small explosion"); root.transform.position = position; root.AddComponent<ScoreEffect>();
            for (int i=0;i<8;i++)
            {
                var spark = new GameObject("Spark"); spark.transform.SetParent(root.transform,false);
                spark.transform.localPosition = Quaternion.Euler(0,0,i*45) * Vector3.right * .12f;
                spark.transform.localScale = Vector3.one * .1f;
                var renderer = spark.AddComponent<SpriteRenderer>(); renderer.sprite = sparkSprite; renderer.color = color; renderer.sortingOrder = 20;
            }
        }
        public static void Create(Vector3 position, int points)
        {
            var root = new GameObject("Score feedback");
            root.transform.position = position;
            var effect = root.AddComponent<ScoreEffect>();
            effect.label = root.AddComponent<TextMesh>();
            effect.label.text = (points > 0 ? "+" : "") + points;
            effect.label.fontSize = 60;
            effect.label.characterSize = .08f;
            effect.label.anchor = TextAnchor.MiddleCenter;
            effect.label.color = points > 0 ? new Color(.5f,1,.65f) : new Color(1,.35f,.45f);
            root.GetComponent<MeshRenderer>().sortingOrder = 30;
            var source = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            effect.label.font = source;
            root.GetComponent<MeshRenderer>().sharedMaterial = source.material;
            for (int i = 0; i < 6; i++)
            {
                var spark = new GameObject("Spark"); spark.transform.SetParent(root.transform, false);
                spark.transform.localPosition = Quaternion.Euler(0,0,i*60) * Vector3.right * .2f;
                var text = spark.AddComponent<TextMesh>(); text.text = "+"; text.font = source; text.fontSize = 30;
                text.characterSize = .03f; text.color = effect.label.color;
                spark.GetComponent<MeshRenderer>().sharedMaterial = source.material;
                spark.GetComponent<MeshRenderer>().sortingOrder = 29;
            }
        }
        void Update()
        {
            age += Time.deltaTime; transform.position += Vector3.up * Time.deltaTime;
            foreach (Transform spark in transform) spark.localPosition *= 1 + Time.deltaTime * 3;
            foreach (Transform spark in transform)
            {
                var renderer = spark.GetComponent<SpriteRenderer>();
                if (renderer) { var c = renderer.color; c.a = Mathf.Clamp01(1-age/.4f); renderer.color=c; }
            }
            if (label) { var color = label.color; color.a = 1 - age / .7f; label.color = color; }
            if (age >= .7f) Destroy(gameObject);
        }
    }
}
