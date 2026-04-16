using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ParticuleHandler : MonoBehaviour
{
    private Dictionary<string, Queue<ParticleSystem>> particlePools;
    private List<ParticleSystem> inUse;

    [SerializeField] GameObject dropParticles;
    [SerializeField] GameObject lineParticles;

    void Awake()
    {
        particlePools = new Dictionary<string, Queue<ParticleSystem>>();
        inUse = new List<ParticleSystem>();

        InitializePool("drop", dropParticles, 5);
        InitializePool("line", lineParticles, 8);
    }

    private void InitializePool(string poolName, GameObject prefab, int poolSize)
    {
        var pool = new Queue<ParticleSystem>();
        for (int i = 0; i < poolSize; ++i)
        {
            GameObject instance = Instantiate(prefab);
            instance.name = $"Particle_{poolName}_{i}";
            pool.Enqueue(instance.GetComponent<ParticleSystem>());
        }
        particlePools[poolName] = pool;
    }

    public void EmitOnDrop(PieceObject obj)
    {
        EmitParticles("drop", obj.transform.position, 20);
    }

    public void EmitOnLineDestroyed(Vector3 lineCenterPosition)
    {
        EmitParticles("line", lineCenterPosition, 100);
    }

    private void EmitParticles(string poolName, Vector3 position, int emitCount)
    {
        if (!particlePools.ContainsKey(poolName) || particlePools[poolName].Count == 0) return;

        var system = particlePools[poolName].Dequeue();
        inUse.Add(system);
        system.transform.position = position;
        system.Emit(emitCount);

        var mainParameter = system.main;
        StartCoroutine(ReturnSystemToPool(system, mainParameter.startLifetime.constant, poolName));
    }

    private IEnumerator ReturnSystemToPool(ParticleSystem system, float delay, string poolName)
    {
        yield return new WaitForSeconds(delay);
        inUse.Remove(system);
        particlePools[poolName].Enqueue(system);
    }
}
