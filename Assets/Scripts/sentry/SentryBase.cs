using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SentryBase : MonoBehaviour
{
    [Header("Sentry Stats")]
    public float maxHP = 100f;
    protected float currentHP;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public float attackCooldown = 1f;
    public float attackRange = 5f;

    protected float cooldownTimer = 0f;

    protected virtual void Start()
    {
        currentHP = maxHP;
    }

    protected virtual void Update()
    {
        cooldownTimer += Time.deltaTime;
        HandleBehavior();
    }

    // 派生クラスで行動を定義（例：静止型、索敵型など）
    protected abstract void HandleBehavior();

    public virtual void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            OnDestroyed();
        }
    }

    protected virtual void OnDestroyed()
    {
        Destroy(gameObject);
    }

    protected void TryShootAtTarget(Transform target)
    {
        if (cooldownTimer >= attackCooldown)
        {
            cooldownTimer = 0f;

            Vector3 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle - 90f); // -90度はスプライトの前方が上を向いてる場合だけ

            GameObject bullet = Instantiate(bulletPrefab, transform.position, rot);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            //if (rb != null)
            //{
            //    rb.velocity = direction * 10f; // 仮速度
            //}
        }
    }
}
