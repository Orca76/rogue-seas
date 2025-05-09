using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : BulletBase
{
    // Start is called before the first frame update

   new void  Update()
    {
        Debug.Log("speed: " + speed);
        transform.position += transform.up * speed * Time.deltaTime;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 敵に命中したら（仮処理）
            // 敵のHPスクリプトが未実装のため、今は破壊だけ
            Destroy(gameObject);
        }
    }
}
