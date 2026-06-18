using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int count = 3;
    public bool spawnOnStart = true;

    public Vector2 arenaX = new Vector2(-20f, 20f);
    public Vector2 arenaZ = new Vector2(-20f, 20f);
    public float ySpawn = 0.5f;

    [Header("Force Settings")]
    public float forceSpeed = 8f;         // para que se note el movimiento
    public bool forceAddComponents = true;

    private GameObject[] spawned;

    private void Start()
    {
        if (spawnOnStart)
            SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemyPrefab NO asignado.");
            return;
        }

        ClearEnemies();

        spawned = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(arenaX.x, arenaX.y),
                ySpawn,
                Random.Range(arenaZ.x, arenaZ.y)
            );

            GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
            go.name = "EnemyBlocker_" + i;
            go.isStatic = false;

            if (forceAddComponents)
                ForceSetup(go);

            spawned[i] = go;
        }
    }

    private void ForceSetup(GameObject go)
    {
        // Collider sólido
        Collider col = go.GetComponent<Collider>();
        if (col == null) col = go.AddComponent<CapsuleCollider>();
        col.isTrigger = false;

        // Rigidbody estable
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Script de movimiento: si no existe, lo agrega
        EnemyBlocker eb = go.GetComponent<EnemyBlocker>();
        if (eb == null) eb = go.AddComponent<EnemyBlocker>();

        eb.enabled = true;
        eb.speed = Mathf.Max(forceSpeed, 1f);
        eb.arenaX = arenaX;
        eb.arenaZ = arenaZ;
        eb.yLock = ySpawn;
    }

    public void ClearEnemies()
    {
        if (spawned == null) return;

        for (int i = 0; i < spawned.Length; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
    }
}
