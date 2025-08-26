using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homing : BulletBase
{
    [SerializeField] float turnSpeed = 360f; // ���񑬓x�i�x/�b�j

    protected override void Update()
    {
        base.Update(); // �����Ǘ��Ƃ����ʏ���������Ȃ�Ă�

        // �펞��ԋ߂��G��T��
        Transform target = FindNearestEnemy();

        if (target != null)
        {
            // �i�s�������������G�Ɍ�����
            Vector2 toTarget = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f; // up�
            float current = transform.eulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, targetAngle, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, next);
        }

        // �O�ɐi��
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
