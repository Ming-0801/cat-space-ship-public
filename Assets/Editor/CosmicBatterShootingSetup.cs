using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace CosmicBatter.Editor
{
    public static class CosmicBatterShootingSetup
    {
        [MenuItem("Tools/Cosmic Batter/Upgrade Existing Scene - Shooting")]
        public static void UpgradeExisting()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/CosmicBatter.unity");
            var game=Object.FindFirstObjectByType<GameManager>();
            if (!game) throw new System.Exception("CosmicBatter scene needs a GameManager.");
            Configure(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("COSMIC_SHOOTING_UPGRADE_OK");
        }
        public static void Configure(GameManager game)
        {
            const string path="Assets/Prefabs/PlayerBullet.prefab";
            var bullet=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!bullet)
            {
                var obj=new GameObject("PlayerBullet");
                var renderer=obj.AddComponent<SpriteRenderer>();
                renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Star.png");
                renderer.color=new Color(.35f,1,1); renderer.sortingOrder=12;
                obj.transform.localScale=new Vector3(.25f,.65f,1);
                obj.AddComponent<Bullet>();
                bullet=PrefabUtility.SaveAsPrefabAsset(obj,path); Object.DestroyImmediate(obj);
            }
            game.player.bulletPrefab=bullet.GetComponent<Bullet>();
            EditorUtility.SetDirty(game.player);
            foreach (string name in new[] {"DonutBird","Asteroid"})
            {
                string prefabPath="Assets/Prefabs/"+name+".prefab";
                var root=PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    var item=root.GetComponent<FallingObject>();
                    item.isEnemy=name=="DonutBird"; item.blocksBullets=name=="Asteroid";
                    PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            foreach (var text in game.ui.GetComponentsInChildren<Text>(true))
            {
                if (text.name=="Instructions") text.text="A / D or Arrow Keys to move\nHold SPACE to shoot upward!\n\nShoot birds for points. Collect ingredients.\nAvoid birds and rocks. Survive 60 seconds!";
                if (text.name=="Legend") text.text="HOLD SPACE: SHOOT   |   BIRD KILL +25   |   COLLECT INGREDIENTS   |   DODGE HAZARDS";
            }
            if (System.IO.File.Exists("Assets/Art/SpeedingTicket.png") && System.IO.File.Exists("Assets/Art/EvilPigeon.png"))
                CosmicBatterPowerupSetup.Configure(game);
        }
    }
}
