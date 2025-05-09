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
            // �G�ɖ���������i�������j
            // �G��HP�X�N���v�g���������̂��߁A���͔j�󂾂�
            Destroy(gameObject);
        }
    }
}
