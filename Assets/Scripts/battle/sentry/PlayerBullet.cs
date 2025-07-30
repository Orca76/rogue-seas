using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : BulletBase
{
    // Start is called before the first frame update

  
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 敵に命中したら（仮処理）
            // 敵のHPスクリプトが未実装のため、今は破壊だけ
            collision.GetComponent<EnemyBase>().HP -= Damage;
            //Ui処理

            Destroy(gameObject);
        }
    }
}
