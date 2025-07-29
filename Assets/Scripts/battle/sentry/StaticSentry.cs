using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticSentry :SentryBase
{
    //public LayerMask enemyLayer;
    //public float detectionInterval = 0.5f;

    //private float detectionTimer = 0f;

    //protected override void HandleBehavior()
    //{
    //    detectionTimer += Time.deltaTime;
    //    if (detectionTimer >= detectionInterval)
    //    {
    //        detectionTimer = 0f;
    //        SearchAndAttack();
    //    }
    //}

    //void SearchAndAttack()
    //{
    //    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
    //    if (hits.Length > 0)
    //    {
    //        Transform closestTarget = hits[0].transform;
    //        float closestDistance = Vector2.Distance(transform.position, closestTarget.position);

    //        foreach (var hit in hits)
    //        {
    //            float dist = Vector2.Distance(transform.position, hit.transform.position);
    //            if (dist < closestDistance)
    //            {
    //                closestTarget = hit.transform;
    //                closestDistance = dist;
    //            }
    //        }

    //        TryShootAtTarget(closestTarget);
    //    }
    //}

    //void OnDrawGizmosSelected()
    //{
    //    // 攻撃範囲の可視化（エディタ用）
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, attackRange);
    //}
}
