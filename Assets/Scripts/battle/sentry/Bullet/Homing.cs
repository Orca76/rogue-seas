using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homing : BulletBase
{
    [SerializeField] float turnSpeed = 360f; // 旋回速度（度/秒）

    protected override void Update()
    {
        base.Update(); // 寿命管理とか共通処理があるなら呼ぶ

        // 常時一番近い敵を探す
        Transform target = FindNearestEnemy();

        if (target != null)
        {
            // 進行方向を少しずつ敵に向ける
            Vector2 toTarget = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f; // up基準
            float current = transform.eulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, targetAngle, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, next);
        }

        // 前に進む
        transform.position += transform.up * speed * Time.deltaTime;
    }

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform best = null;
        float bestSqr = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var e in enemies)
        {
            float d2 = (e.transform.position - pos).sqrMagnitude;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                best = e.transform;
            }
        }
        return best;
    }
}
