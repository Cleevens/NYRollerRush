// NY ROLLER RUSH - CORE SYSTEM
// Standalone SimpleTest scene: ground, player, follow cam, cubes, coins. No pooling.

using NYRollerRush.Runner;
using UnityEngine;

namespace NYRollerRush.Core
{
    [DefaultExecutionOrder(-200)]
    public class SimpleTestBootstrap : MonoBehaviour
    {
        void Awake()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var ground = Make(PrimitiveType.Cube, "Ground", new Vector3(0f, -0.5f, 20f), new Vector3(50f, 1f, 50f), Color.gray, lit);
            ground.isStatic = true;

            var player = Make(PrimitiveType.Capsule, "Player", new Vector3(0f, 0f, 2f), Vector3.one, new Color(0.15f, 0.85f, 0.55f), lit);
            player.tag = "Player";
            Destroy(player.GetComponent<Collider>());
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.32f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            player.AddComponent<SkateController>();

            var cam = Camera.main != null ? Camera.main.gameObject : BuildCamera();
            cam.transform.position = new Vector3(0f, 6f, -6f);
            var follow = cam.GetComponent<CameraFollow>() ?? cam.AddComponent<CameraFollow>();
            follow.SetTarget(player.transform);

            Vector3[] cubes =
            {
                new Vector3(-2f, 0.6f, 12f),
                new Vector3(2f, 0.6f, 18f),
                new Vector3(0f, 0.6f, 26f),
                new Vector3(-2f, 0.6f, 34f)
            };
            for (int i = 0; i < cubes.Length; i++)
            {
                var cube = Make(PrimitiveType.Cube, "Obstacle_" + i, cubes[i], new Vector3(1.2f, 1.2f, 1.2f), new Color(0.75f, 0.25f, 0.2f), lit);
                cube.transform.SetParent(transform, true);
            }

            Vector3[] coins =
            {
                new Vector3(0f, 0.8f, 8f),
                new Vector3(-2f, 0.8f, 14f),
                new Vector3(2f, 0.8f, 16f),
                new Vector3(0f, 0.8f, 22f),
                new Vector3(2f, 0.8f, 30f)
            };
            for (int i = 0; i < coins.Length; i++)
            {
                var coin = Make(PrimitiveType.Sphere, "Coin_" + i, coins[i], Vector3.one * 0.7f, Color.yellow, lit);
                coin.transform.SetParent(transform, true);
                var col = coin.GetComponent<Collider>();
                col.isTrigger = true;
                coin.AddComponent<SimpleCoin>();
            }
        }

        static GameObject Make(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color color, Shader lit)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.localScale = scale;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(lit != null ? lit : Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = color;
                if (renderer.material.HasProperty("_BaseColor"))
                    renderer.material.SetColor("_BaseColor", color);
            }

            return obj;
        }

        static GameObject BuildCamera()
        {
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, 6f, -6f);
            return cam;
        }
    }

    public class SimpleCoin : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(Vector3.up, 120f * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag("Player"))
                gameObject.SetActive(false);
        }
    }
}
