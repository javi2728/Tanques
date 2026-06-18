using UnityEngine;

public class EnemyBlocker : MonoBehaviour
{
    public float speed = 8f;
    public Vector2 arenaX = new Vector2(-20f, 20f);
    public Vector2 arenaZ = new Vector2(-20f, 20f);

    public float reachDistance = 0.7f;
    public float yLock = 0.5f;

    private Vector3 target;

    private void OnEnable()
    {
        PickNewTarget();
    }

    private void Update()
    {
        // se mueve aunque Time.timeScale esté raro
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) dt = 0.016f;

        Vector3 pos = transform.position;
        pos.y = yLock;

        Vector3 dir = target - pos;
        dir.y = 0f;

        if (dir.magnitude <= reachDistance)
        {
            PickNewTarget();
            return;
        }

        transform.position = pos + dir.normalized * speed * dt;

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    private void PickNewTarget()
    {
        float x = Random.Range(arenaX.x, arenaX.y);
        float z = Random.Range(arenaZ.x, arenaZ.y);
        target = new Vector3(x, yLock, z);

        // Debug para confirmar que está “vivo”
        // Debug.Log($"{name} new target: {target}");
    }
}
