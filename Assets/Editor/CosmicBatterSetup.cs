using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CosmicBatter.Editor
{
    public static class CosmicBatterSetup
    {
        const string ScenePath = "Assets/Scenes/CosmicBatter.unity";
        static Font font;
        [MenuItem("Tools/Cosmic Batter/Build Prototype")]
        public static void BuildPrototype()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string folder in new[] { "Art", "Scenes", "Prefabs" }) Directory.CreateDirectory("Assets/" + folder);
            AssetDatabase.Refresh();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0,0,-10);
            camera.orthographic = true; camera.orthographicSize = 5; camera.backgroundColor = new Color(.025f,.025f,.09f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<AudioListener>();
            var starSprite = Placeholder("Star", Color.white, 0);
            var stars = new GameObject("Background stars (editable)");
            var random = new System.Random(42);
            for (int i=0;i<90;i++)
            {
                var star = new GameObject("Star").AddComponent<SpriteRenderer>(); star.transform.SetParent(stars.transform);
                star.sprite = starSprite; star.sortingOrder = -10;
                star.color = new Color(.45f,.55f,.85f,(float)random.NextDouble()*.6f+.2f);
                star.transform.position = new Vector3((float)random.NextDouble()*19-9.5f,(float)random.NextDouble()*10-5,1);
                star.transform.localScale = Vector3.one * (.025f+(float)random.NextDouble()*.035f);
            }
            var game = new GameObject("GameManager - round and ranks").AddComponent<GameManager>();
            var player = new GameObject("Player - Green Cat").AddComponent<PlayerController>();
            player.game = game; player.transform.position = new Vector3(0,-3.7f,0);
            var art = player.GetComponent<SpriteRenderer>(); art.sprite = LoadArt("PlayerShip", 1.9f) ?? Placeholder("PlayerPlaceholder", Color.green, 0); art.sortingOrder = 10;
            var body = player.GetComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0;
            var hit = player.gameObject.AddComponent<BoxCollider2D>(); hit.size = new Vector2(1.05f,.75f); hit.isTrigger = true;
            game.player = player;
            string[] names = { "Milk", "Sugar", "Egg", "DonutBird", "Asteroid" };
            int[] points = {10,15,20,-20,-30};
            Color[] colors = {new Color(.55f,.85f,1),new Color(1,.55f,.85f),new Color(1,.9f,.6f),new Color(.65f,.4f,1),new Color(.55f,.6f,.7f)};
            var prefabs = new FallingObject[5];
            for (int i=0;i<5;i++)
            {
                var obj = new GameObject(names[i]);
                var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = LoadArt(names[i],i==3 ? 1.1f : .65f) ?? Placeholder(names[i]+"Placeholder",colors[i], i==0 || i==1 ? 1 : 0);
                renderer.sortingOrder = 5;
                var rb = obj.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0;
                rb.useFullKinematicContacts = true;
                var collider = obj.AddComponent<CircleCollider2D>(); collider.radius = i==3 ? .43f : .3f; collider.isTrigger = true;
                var falling = obj.AddComponent<FallingObject>(); falling.points = points[i];
                if (i!=3)
                {
                    var label = new GameObject("Label - placeholder only").AddComponent<TextMesh>(); label.transform.SetParent(obj.transform,false);
                    label.text = i==0 ? "M" : i==1 ? "S" : i==2 ? "E" : "X";
                    label.anchor = TextAnchor.MiddleCenter; label.font = font; label.fontSize=40; label.characterSize=.075f; label.color = new Color(.1f,.1f,.2f);
                    label.GetComponent<MeshRenderer>().sharedMaterial = font.material; label.GetComponent<MeshRenderer>().sortingOrder=6;
                }
                prefabs[i] = PrefabUtility.SaveAsPrefabAsset(obj,"Assets/Prefabs/"+names[i]+".prefab").GetComponent<FallingObject>();
                Object.DestroyImmediate(obj);
            }
            var spawner = new GameObject("Spawner - density and speed").AddComponent<ObjectSpawner>(); spawner.game = game; spawner.prefabs = prefabs; game.spawner = spawner;
            MakeUI(game);
            CosmicBatterShootingSetup.Configure(game);
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            EditorSceneManager.SaveScene(scene, ScenePath);
            var existing = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            existing.RemoveAll(s => s.path == ScenePath); existing.Insert(0,new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes = existing.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Cosmic Batter ready: open Assets/Scenes/CosmicBatter.unity and press Play.");
        }
        static Sprite LoadArt(string name, float worldSize)
        {
            string path = "Assets/Art/"+name+".png";
            if (!File.Exists(path)) return null;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);
            importer.spritePixelsPerUnit = Mathf.Max(w,h)/worldSize; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static Sprite Placeholder(string name,Color color,int shape)
        {
            string path="Assets/Art/"+name+".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(64,64);
                for(int y=0;y<64;y++) for(int x=0;x<64;x++)
                {
                    bool inside = shape==1 ? x>6 && x<58 && y>4 && y<60 : Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))<29;
                    texture.SetPixel(x,y,inside ? color : Color.clear);
                }
                texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
            }
            return LoadArt(name,.65f);
        }
        static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos)
        {
            var r = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); r.sizeDelta=size; r.anchoredPosition=pos; return r;
        }
        static Text Label(string name,Transform parent,string value,Vector2 size,Vector2 pos,int fontSize)
        {
            var text=Rect(name,parent,size,pos).gameObject.AddComponent<Text>(); text.font=font; text.text=value; text.fontSize=fontSize; text.alignment=TextAnchor.MiddleCenter; text.color=Color.white; text.raycastTarget=false; return text;
        }
        static GameObject Panel(string name,Transform parent)
        {
            var r=Rect(name,parent,new Vector2(920,560),Vector2.zero); var image=r.gameObject.AddComponent<Image>(); image.color=new Color(.055f,.06f,.17f,.96f); return r.gameObject;
        }
        static Button Button(Transform parent,string label)
        {
            var r=Rect(label,parent,new Vector2(280,64),new Vector2(0,-190)); var image=r.gameObject.AddComponent<Image>(); image.color=new Color(.27f,.8f,.63f);
            var button=r.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            var text=Label("Label",r,label,new Vector2(280,64),Vector2.zero,26); text.color=new Color(.025f,.07f,.12f); return button;
        }
        static void MakeUI(GameManager game)
        {
            var canvas=new GameObject("UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scale=canvas.GetComponent<CanvasScaler>(); scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scale.referenceResolution=new Vector2(1280,720); scale.matchWidthOrHeight=.5f;
            var ui=canvas.AddComponent<GameUI>(); ui.game=game; game.ui=ui;
            ui.hud=Rect("HUD",canvas.transform,new Vector2(1280,720),Vector2.zero).gameObject;
            ui.scoreText=Label("Score",ui.hud.transform,"",new Vector2(300,60),new Vector2(-450,305),30);
            ui.timerText=Label("Time",ui.hud.transform,"",new Vector2(200,60),new Vector2(0,305),34);
            Label("Legend",ui.hud.transform,"MILK +10     SUGAR +15     EGG +20     BIRD -20     ROCK -30",new Vector2(1100,40),new Vector2(0,-335),19);
            ui.startPanel=Panel("Start screen",canvas.transform);
            Label("Title",ui.startPanel.transform,"COSMIC BATTER",new Vector2(850,80),new Vector2(0,170),54);
            Label("Subtitle",ui.startPanel.transform,"Batter Makes You Better!",new Vector2(800,60),new Vector2(0,95),26);
            Label("Instructions",ui.startPanel.transform,"A / D or Arrow Keys to move\n\nCollect Milk, Sugar and Eggs. Avoid Birds and Rocks!\n\n60 seconds. How much batter can you gather?",new Vector2(850,180),new Vector2(0,-40),23);
            ui.startButton=Button(ui.startPanel.transform,"LET'S BAKE!");
            ui.resultPanel=Panel("Result screen",canvas.transform);
            ui.resultText=Label("Result",ui.resultPanel.transform,"",new Vector2(800,350),new Vector2(0,50),44);
            ui.restartButton=Button(ui.resultPanel.transform,"PLAY AGAIN"); ui.resultPanel.SetActive(false); ui.hud.SetActive(false);
            var events=new GameObject("EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<InputSystemUIInputModule>();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
