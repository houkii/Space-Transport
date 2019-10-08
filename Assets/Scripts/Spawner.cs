using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private List<Mesh> meshes;

    [SerializeField]
    private Material material;

    private List<Mesh> meshesToRender = new List<Mesh>();

    public void Awake()
    {
        //base.Awake();
        DontDestroyOnLoad(gameObject);
        PrepareMeshes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlayScene")
        {
            DestroyEntities();

            foreach (PlanetarySystemInstance system in GameController.Instance.MissionController.CurrentMission.PlanetarySystems)
            {
                SpawnAsteroids(system.Origin);
            }

            SpawnAsteroidBounds(GameController.Instance.MissionController.CurrentMission.BoundsSize);

            //foreach (Mesh mesh in meshesToRender)
            //{
            //    Spawn(65, mesh, 1000, 1300, -15, 45);
            //}

            //foreach (Mesh mesh in meshesToRender)
            //{
            //    Spawn(3, mesh, 200, 1200, 100, 200);
            //}

            //foreach (Mesh mesh in meshesToRender)
            //{
            //    Spawn(3, mesh, 200, 1200, 100, 200);
            //}
        }
        else if (scene.name == "MainMenu")
        {
            DestroyEntities();
            foreach (Mesh mesh in meshesToRender)
            {
                Spawn(5, mesh, 350, 650, 250, 550, Vector2.zero);
            }
        }
    }

    private void PrepareMeshes()
    {
        foreach (Mesh mesh in meshes)
        {
            meshesToRender.Add(GetScaledMesh(mesh, Random.Range(10f, 20f)));
        }
    }

    private void SpawnAsteroidBounds(int boundsSize)
    {
        int rMin = boundsSize - 250;
        int rMax = boundsSize + 250;
        foreach (Mesh mesh in meshesToRender)
        {
            Spawn(5, mesh, rMin, rMax, -50, -100, Vector2.zero);
        }

        foreach (Mesh mesh in meshesToRender)
        {
            Spawn(25, mesh, rMin, rMax, 50, 100, Vector2.zero);
        }
    }

    public void SpawnAsteroids(Vector2 origin)
    {
        foreach (Mesh mesh in meshesToRender)
        {
            Spawn(1, mesh, 200, 1200, 100, 200, origin);
        }

        foreach (Mesh mesh in meshesToRender)
        {
            Spawn(1, mesh, 200, 1200, -100, -200, origin);
        }
    }

    private void Spawn(int numToSpawn, Mesh mesh, float innerRadius, float outerRadius, float zMin, float zMax, Vector2 origin)
    {
        EntityManager entityManager = World.Active.EntityManager;

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(MoveComponent),
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
            );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(numToSpawn, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            var position = RandomInRing(innerRadius, outerRadius);

            entityManager.SetComponentData(entity, new MoveComponent
            {
                rotationSpeeds = RandomRotationSpeed,
                origin = new Unity.Mathematics.float2(origin.x, origin.y),
                current = position
            });

            position.x += origin.x;
            position.y += origin.y;

            entityManager.SetComponentData(entity, new Translation
            {
                Value = new Unity.Mathematics.float3(position.x, position.y, Random.Range(zMin, zMax))
            });

            entityManager.SetComponentData(entity, new Rotation
            {
                Value = RandomRotation
            });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = material
            });
        }

        entityArray.Dispose();
    }

    private Unity.Mathematics.quaternion RandomRotation =>
        Unity.Mathematics.quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );

    private Unity.Mathematics.float3 RandomRotationSpeed =>
        new Unity.Mathematics.float3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), Random.Range(-2f, 2f));

    private Unity.Mathematics.float2 RandomInRing(float innerRadius, float outerRadius)
    {
        var pos = Utils.GetRotatedPosition(Vector2.right * Random.Range(innerRadius, outerRadius), Random.Range(0, 360f));
        var randomInRing = new Unity.Mathematics.float2(pos.x, pos.y);
        return randomInRing;
    }

    private void OnDestroy()
    {
        DestroyEntities();
    }

    private void DestroyEntities()
    {
        EntityManager entityManager;
        try
        {
            entityManager = World.Active.EntityManager;
            var entityArray = entityManager.GetAllEntities();
            foreach (var e in entityArray)
                entityManager.DestroyEntity(e);
            entityArray.Dispose();
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning("No entity manager available!");
        }
    }

    private Mesh GetScaledMesh(Mesh mesh, float scale)
    {
        Vector3[] _baseVertices = mesh.vertices;
        Mesh newMesh = new Mesh();
        newMesh.name = "clone";
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.normals;
        newMesh.uv = mesh.uv;

        var vertices = new Vector3[_baseVertices.Length];
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = _baseVertices[i];
            vertex.x = vertex.x * scale;
            vertex.y = vertex.y * scale;
            vertex.z = vertex.z * scale;
            vertices[i] = vertex;
        }
        newMesh.vertices = vertices;
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        return newMesh;
    }
}

public struct MoveComponent : IComponentData
{
    public Unity.Mathematics.float3 rotationSpeeds;
    public Unity.Mathematics.float2 origin;
    public Unity.Mathematics.float2 current;
}

public class MoveSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref Rotation rotation, ref MoveComponent moveData) =>
        {
            //var entityPos = new Vector2(translation.Value.x, translation.Value.y);
            //var newPosXY = Utils.GetRotatedPosition(entityPos, translation.Value.z / 50 * Time.deltaTime);
            //translation.Value = new Unity.Mathematics.float3(newPosXY.x + moveData.origin.x, newPosXY.y + moveData.origin.y, translation.Value.z);

            var entityPos = new Vector2(moveData.current.x, moveData.current.y);
            var newPosXY = Utils.GetRotatedPosition(entityPos, translation.Value.z / 50 * Time.deltaTime);
            moveData.current = new Unity.Mathematics.float2(newPosXY.x, newPosXY.y);
            translation.Value = new Unity.Mathematics.float3(newPosXY.x + moveData.origin.x, newPosXY.y + moveData.origin.y, translation.Value.z);

            rotation.Value = Unity.Mathematics.quaternion.Euler(
                moveData.rotationSpeeds.x * Time.unscaledTime,
                moveData.rotationSpeeds.y * Time.unscaledTime,
                0//moveData.rotationSpeeds.z * Time.unscaledTime
            );
        });
    }
}