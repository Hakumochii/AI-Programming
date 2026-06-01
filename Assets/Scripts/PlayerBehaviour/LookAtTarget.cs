using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;

    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;  // keep it flat

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90f, angle, 0f);  // 90 keeps it flat on ground
    }
}