using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CosmicBatter.Editor
{
    public static class CosmicBatterPowerupSetup
    {
        [MenuItem("Tools/Cosmic Batter/Update Items And Enemies")]
        public static void UpgradeExisting()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/CosmicBatter.unity");
            var game=Object.FindFirstObjectByType<GameManager>();
            if (!game) throw new System.Exception("Missing existing GameManager.");
            Configure(game);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("COSMIC_POWERUP_UPGRADE_OK");
        }
        public static void Configure(GameManager game)
        {
            // Existing player, UI layout and original art stay in this scene.
            game.roundSeconds=60;
            SetItem("Milk",FallingObject.TouchEffect.Score,10);
            SetItem("Sugar",FallingObject.TouchEffect.HeavyFire,0);
            SetItem("Egg",FallingObject.TouchEffect.SpeedBoost,0);
            game.spawner.ticketPrefab=Enemy("SpeedingTicket",false);
            game.spawner.pigeonPrefab=Enemy("EvilPigeon",true);
            game.spawner.initialInterval=.7f; game.spawner.minimumInterval=.55f;
            game.spawner.fallSpeed=new Vector2(2,3); game.spawner.collectibleChance=.9f;
            game.spawner.maxEnemies=22;
            game.spawner.stageIntervals=new[] {1.7f,1.4f,1.1f,.85f};
            game.player.speedBonus=.3f; game.player.speedDuration=5;
            game.player.slowPenalty=.4f; game.player.slowDuration=3;
            game.player.heavyDuration=2; game.player.pigeonCollisionCooldown=1.5f;
            game.player.GetComponent<Rigidbody2D>().interpolation=RigidbodyInterpolation2D.Interpolate;
            game.ui.speedText=Indicator(game.ui,"Speed effect",new Vector2(-365,252),new Color(.5f,1,.6f));
            game.ui.heavyText=Indicator(game.ui,"Heavy effect",new Vector2(0,252),new Color(1,.8f,.3f));
            game.ui.slowText=Indicator(game.ui,"Slow effect",new Vector2(365,252),new Color(.5f,.8f,1));
            foreach(var label in game.ui.GetComponentsInChildren<Text>(true))
            {
                if(label.name=="Instructions") label.text="WASD / Arrow Keys: Move     SPACE: Shoot\n\nMilk: +10   Egg: Speed +30% / 5s\nSugar: Triple rapid fire / 2s\n\nShoot tickets +10 and pigeons +20. Dodge their attacks!";
                if(label.name=="Legend") label.text="MILK +10   |   EGG: SPEED   |   SUGAR: TRIPLE FIRE   |   TICKET: SLOW   |   PIGEON: -50";
            }
            EditorUtility.SetDirty(game); EditorUtility.SetDirty(game.player); EditorUtility.SetDirty(game.spawner); EditorUtility.SetDirty(game.ui);
        }
        static Text Indicator(GameUI ui,string name,Vector2 position,Color color)
        {
            var existing=ui.hud.transform.Find(name);
            if(existing) return existing.GetComponent<Text>();
            var obj=new GameObject(name,typeof(RectTransform),typeof(Text)); obj.transform.SetParent(ui.hud.transform,false);
            var rect=obj.GetComponent<RectTransform>(); rect.sizeDelta=new Vector2(350,42); rect.anchoredPosition=position;
            var text=obj.GetComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=21;
            text.alignment=TextAnchor.MiddleCenter; text.color=color; text.raycastTarget=false; obj.SetActive(false); return text;
        }
        static void SetItem(string name,FallingObject.TouchEffect effect,int points)
        {
            string path="Assets/Prefabs/"+name+".prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try { var item=root.GetComponent<FallingObject>(); item.touchEffect=effect; item.points=points; PrefabUtility.SaveAsPrefabAsset(root,path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static FallingObject Enemy(string name,bool pigeon)
        {
            string artPath="Assets/Art/"+name+".png";
            AssetDatabase.ImportAsset(artPath);
            var importer=(TextureImporter)AssetImporter.GetAtPath(artPath);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.maxTextureSize=2048;
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            importer.spritePixelsPerUnit=Mathf.Max(width,height)/(pigeon ? 1.7f : 1.9f); importer.SaveAndReimport();
            string path="Assets/Prefabs/"+name+".prefab";
            var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var root=PrefabUtility.LoadPrefabContents(existing ? path : "Assets/Prefabs/DonutBird.prefab");
            try
            {
                root.name=name; root.transform.localScale=Vector3.one;
                root.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
                var item=root.GetComponent<FallingObject>(); item.isEnemy=true; item.blocksBullets=false;
                item.killPoints=pigeon ? 20 : 10; item.points=pigeon ? -50 : 0;
                item.touchEffect=pigeon ? FallingObject.TouchEffect.PigeonPenalty : FallingObject.TouchEffect.TicketSlow;
                item.movement=pigeon ? FallingObject.Movement.Dash : FallingObject.Movement.Follow;
                item.followSpeed=pigeon ? 1.7f : 1.0f; item.dashSpeed=5.5f; item.dashDuration=.35f; item.dashInterval=2.8f;
                var collider=root.GetComponent<CircleCollider2D>(); collider.radius=pigeon ? .44f : .55f; collider.isTrigger=true;
                var body=root.GetComponent<Rigidbody2D>(); body.bodyType=RigidbodyType2D.Kinematic; body.gravityScale=0; body.useFullKinematicContacts=true; body.interpolation=RigidbodyInterpolation2D.Interpolate;
                return PrefabUtility.SaveAsPrefabAsset(root,path).GetComponent<FallingObject>();
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
