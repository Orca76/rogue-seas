using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBase : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed;
    public float lifetime;
    public float Damage;
    //public GameObject popupPrefab;
    public bool ispenetrate = true;
    protected virtual void Start()
    {
        Destroy(gameObject, lifetime);
    }

    protected virtual void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }

    //protected virtual void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        var enemy = collision.GetComponent<EnemyBase>();
    //        if (enemy != null) enemy.HP -= Damage;



           
          
    //        DamagePopup.Spawn(popupPrefab, enemy.transform.position, Mathf.RoundToInt(collision.GetComponent<EnemyBullet>().Damage));
           


    //        Destroy(gameObject);
    //    }
    //}
}
