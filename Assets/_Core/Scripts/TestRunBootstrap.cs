// NY ROLLER RUSH - CORE SYSTEM
// Builds placeholder prefabs + a playable TestRun layout at runtime.

using NYRollerRush.Pooling;
using NYRollerRush.Runner;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Core
{
    [DefaultExecutionOrder(-200)]
    public class TestRunBootstrap : MonoBehaviour
    {
        [SerializeField] bool buildIfMissing = true;

        GameObject chunkPrefab;
        GameObject carPrefab;
        GameObject coinPrefab;
        GameObject pedPrefab;
        GameObject powerPrefab;
        GameObject companionPrefab;

        void Awake()
        {
            if (!buildIfMissing) return;
            EnsureTags();
            CreateMaterials();
            chunkPrefab = BuildChunkPrefab();
            carPrefab = BuildCarPrefab();
            coinPrefab = BuildCoinPrefab();
            pedPrefab = BuildPedPrefab();
            powerPrefab = BuildPowerPrefab();
            companionPrefab = BuildCompanionPrefab();

            var hub = FindOrAdd<PoolHub>("Systems");
            hub.AssignPrefabs(chunkPrefab, carPrefab, coinPrefab, pedPrefab, powerPrefab, companionPrefab);

            var player = BuildPlayer();
            var cam = Camera.main != null ? Camera.main.gameObject : BuildCamera();
            var follow = cam.GetComponent<CameraFollow>() ?? cam.AddComponent<CameraFollow>();
            follow.SetTarget(player.transform);

            FindOrAdd<SaveSystem>("Systems");
            FindOrAdd<CurrencyManager>("Systems");
            FindOrAdd<ShopManager>("Systems");
            FindOrAdd<NeighborhoodManager>("Systems");
            FindOrAdd<GameManager>("Systems");
            FindOrAdd<TrafficManager>("Systems");
            FindOrAdd<PowerUpManager>("Systems");
            FindOrAdd<CompanionSkaterSpawner>("Systems");
            FindOrAdd<EndlessChunkSpawner>("Systems");
            FindOrAdd<TrafficNetwork>("Systems");
            FindOrAdd<AudioManager>("Systems");
            FindOrAdd<GameUI>("Systems");

            BuildLanePaths();
            BuildStarterLight(new Vector3(0f, 0f, 22f));
        }

        void EnsureTags()
        {
            // Tags must exist in TagManager; bootstrap only assigns them.
        }

        Material streetMat;
        Material laneMat;
        Material carMat;
        Material coinMat;
        Material pedMat;
        Material lampMat;
        Material companionMat;

        static Shader UrpLit()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            return shader;
        }

        void CreateMaterials()
        {
            var shader = UrpLit();
            streetMat = NewMat(shader, new Color(0.18f, 0.18f, 0.2f));
            laneMat = NewMat(shader, new Color(0.92f, 0.85f, 0.2f));
            carMat = NewMat(shader, new Color(0.75f, 0.15f, 0.18f));
            coinMat = NewMat(shader, new Color(1f, 0.84f, 0.15f));
            pedMat = NewMat(shader, new Color(0.35f, 0.55f, 0.85f));
            lampMat = NewMat(shader, Color.green);
            companionMat = NewMat(shader, new Color(0.95f, 0.45f, 0.85f));
        }

        static GameObject Primitive(PrimitiveType type)
        {
            var obj = GameObject.CreatePrimitive(type);
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = Color.gray;
            }

            return obj;
        }

        static Material NewMat(Shader shader, Color color)
        {
            var mat = new Material(shader != null ? shader : UrpLit());
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        GameObject BuildChunkPrefab()
        {
            var root = Hidden("ChunkPrefab");
            var ground = Primitive(PrimitiveType.Cube);
            ground.name = "Asphalt";
            ground.transform.SetParent(root.transform, false);
            ground.transform.localScale = new Vector3(14f, 0.2f, 40f);
            ground.transform.localPosition = new Vector3(0f, -0.1f, 20f);
            ground.GetComponent<Renderer>().sharedMaterial = streetMat;

            for (int i = 0; i < 2; i++)
            {
                var line = Primitive(PrimitiveType.Cube);
                line.name = "LaneLine";
                line.transform.SetParent(root.transform, false);
                line.transform.localScale = new Vector3(0.08f, 0.04f, 40f);
                line.transform.localPosition = new Vector3(i == 0 ? -1f : 1f, 0.02f, 20f);
                Destroy(line.GetComponent<Collider>());
                line.GetComponent<Renderer>().sharedMaterial = laneMat;
            }

            var begin = new GameObject("Begin").transform;
            begin.SetParent(root.transform, false);
            begin.localPosition = Vector3.zero;
            var end = new GameObject("End").transform;
            end.SetParent(root.transform, false);
            end.localPosition = new Vector3(0f, 0f, 40f);

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2f, 20f);
            trigger.size = new Vector3(16f, 8f, 40f);

            var coins = new Transform[6];
            for (int i = 0; i < coins.Length; i++)
            {
                var t = new GameObject("CoinSpawn").transform;
                t.SetParent(root.transform, false);
                t.localPosition = new Vector3(((i % 3) - 1) * 2f, 0.8f, 6f + i * 5f);
                coins[i] = t;
            }

            var cars = new[] { Marker(root, "CarSpawnL", new Vector3(-2f, 0.55f, 28f)), Marker(root, "CarSpawnR", new Vector3(2f, 0.55f, 32f)) };
            var peds = new[] { Marker(root, "PedL", new Vector3(-5.2f, 0f, 16f)), Marker(root, "PedR", new Vector3(5.2f, 0f, 24f)) };

            var chunk = root.AddComponent<PooledChunk>();
            chunk.Configure(begin, end, cars, peds, coins);
            return root;
        }

        GameObject BuildCarPrefab()
        {
            var root = Hidden("CarPrefab");
            var body = Primitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1.4f, 1.1f, 3.2f);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = carMat;
            body.GetComponent<BoxCollider>().isTrigger = false;
            root.tag = "Car";
            body.tag = "Car";
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            root.AddComponent<KinematicVehicleAI>();
            var near = root.AddComponent<BoxCollider>();
            near.isTrigger = true;
            near.center = new Vector3(0f, 0.6f, 0f);
            near.size = new Vector3(2.1f, 1.4f, 4.2f);
            return root;
        }

        GameObject BuildCoinPrefab()
        {
            var root = Hidden("CoinPrefab");
            var gem = Primitive(PrimitiveType.Sphere);
            gem.name = "Gem";
            gem.transform.SetParent(root.transform, false);
            gem.transform.localScale = Vector3.one * 0.55f;
            gem.transform.localPosition = Vector3.up * 0.35f;
            gem.GetComponent<Renderer>().sharedMaterial = coinMat;
            var col = gem.GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.6f;
            root.tag = "Coin";
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            root.AddComponent<CollectibleCoin>();
            return root;
        }

        GameObject BuildPedPrefab()
        {
            var root = Hidden("PedPrefab");
            var body = Primitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.7f, 0.8f, 0.7f);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = pedMat;
            var col = body.GetComponent<Collider>();
            col.isTrigger = true;
            root.tag = "Pedestrian";
            body.tag = "Pedestrian";
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            root.AddComponent<PooledPedestrian>();
            return root;
        }

        GameObject BuildPowerPrefab()
        {
            var root = Hidden("PowerPrefab");
            var gem = Primitive(PrimitiveType.Sphere);
            gem.name = "Gem";
            gem.transform.SetParent(root.transform, false);
            gem.transform.localScale = Vector3.one * 0.5f;
            gem.transform.localPosition = Vector3.up * 0.45f;
            gem.GetComponent<Renderer>().sharedMaterial = lampMat;
            gem.GetComponent<Collider>().isTrigger = true;
            root.tag = "PowerUp";
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            root.AddComponent<PowerUpPickup>();
            return root;
        }

        GameObject BuildCompanionPrefab()
        {
            var root = Hidden("CompanionPrefab");
            var body = Primitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.7f, 0.75f, 0.7f);
            body.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = companionMat;
            var col = body.GetComponent<Collider>();
            col.isTrigger = true;
            root.tag = "Companion";
            body.tag = "Companion";
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            root.AddComponent<CompanionSkater>();
            return root;
        }

        GameObject BuildPlayer()
        {
            var existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null && existing.GetComponent<SkateController>() != null)
                return existing;

            var player = Primitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0f, 2f);
            Destroy(player.GetComponent<Collider>());
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.32f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            player.AddComponent<SwipeInput>();
            player.AddComponent<ArrowKeyInput>();
            player.AddComponent<CompositeRunnerInput>();
            player.AddComponent<SkateController>();
            var skin = player.GetComponent<Renderer>();
            if (skin != null)
                skin.sharedMaterial = NewMat(streetMat.shader, new Color(0.15f, 0.85f, 0.55f));
            return player;
        }

        GameObject BuildCamera()
        {
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.AddComponent<CameraFollow>();
            cam.transform.position = new Vector3(0f, 6f, -6f);
            return cam;
        }

        void BuildLanePaths()
        {
            var net = FindOrAdd<TrafficNetwork>("Systems");
            var paths = new WaypointPath[3];
            for (int i = 0; i < 3; i++)
            {
                var pathGo = new GameObject("LanePath_" + i);
                pathGo.transform.SetParent(net.transform, false);
                var path = pathGo.AddComponent<WaypointPath>();
                path.id = i;
                var nodes = new WaypointNode[4];
                float x = (i - 1) * 2f;
                for (int n = 0; n < nodes.Length; n++)
                {
                    var nodeGo = new GameObject("WP_" + n);
                    nodeGo.transform.SetParent(pathGo.transform, false);
                    nodeGo.transform.position = new Vector3(x, 0.55f, n * 80f);
                    nodes[n] = nodeGo.AddComponent<WaypointNode>();
                }

                for (int n = 0; n < nodes.Length - 1; n++)
                    nodes[n].next = new[] { nodes[n + 1] };
                path.waypoints = nodes;
                path.nextPaths = new[] { path };
                paths[i] = path;
            }

            net.paths = paths;
        }

        void BuildStarterLight(Vector3 position)
        {
            var root = new GameObject("TrafficLight");
            root.tag = "TrafficLight";
            root.transform.position = position;

            var pole = Primitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(root.transform, false);
            pole.transform.localScale = new Vector3(0.15f, 1.6f, 0.15f);
            pole.transform.localPosition = new Vector3(4.6f, 1.6f, 0f);
            Destroy(pole.GetComponent<Collider>());

            var lamp = Primitive(PrimitiveType.Sphere);
            lamp.name = "Lamp";
            lamp.transform.SetParent(root.transform, false);
            lamp.transform.localScale = Vector3.one * 0.45f;
            lamp.transform.localPosition = new Vector3(4.6f, 3.3f, 0f);
            Destroy(lamp.GetComponent<Collider>());
            lamp.GetComponent<Renderer>().sharedMaterial = lampMat;

            var stop = new GameObject("StopLine");
            stop.transform.SetParent(root.transform, false);
            stop.transform.localPosition = Vector3.zero;
            var stopPoint = stop.AddComponent<RoadStopPoint>();

            var cycle = root.AddComponent<TrafficLightCycle>();
            cycle.DrivenExternally = true;
            var controller = root.AddComponent<TrafficLightController>();
            controller.Configure(lamp.GetComponent<Renderer>(), stopPoint, cycle);

            var net = TrafficNetwork.Instance;
            if (net != null)
                net.lights = new[] { cycle };
        }

        static Transform Marker(GameObject parent, string name, Vector3 local)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent.transform, false);
            t.localPosition = local;
            return t;
        }

        GameObject Hidden(string name)
        {
            var folder = transform.Find("Templates");
            if (folder == null)
            {
                folder = new GameObject("Templates").transform;
                folder.SetParent(transform, false);
                folder.gameObject.SetActive(false);
            }

            var go = new GameObject(name);
            go.transform.SetParent(folder, false);
            return go;
        }

        T FindOrAdd<T>(string hostName) where T : Component
        {
            var existing = FindObjectOfType<T>();
            if (existing != null) return existing;
            var host = GameObject.Find(hostName);
            if (host == null)
                host = new GameObject(hostName);
            return host.AddComponent<T>();
        }
    }
}
