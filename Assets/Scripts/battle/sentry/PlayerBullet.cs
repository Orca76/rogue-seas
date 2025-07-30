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
            // �G�ɖ���������i�������j
            // �G��HP�X�N���v�g���������̂��߁A���͔j�󂾂�
            collision.GetComponent<EnemyBase>().HP -= Damage;
            //Ui����

            Destroy(gameObject);
        }
    }
}
