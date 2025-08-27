using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Penetrate : BulletBase
{
    // Start is called before the first frame update
   protected override void Start()
    {
        ispenetrate = true;
    }

    // Update is called once per frame

    //protected override void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        collision.GetComponent<EnemyBase>().HP -= Damage;
    //      //  Destroy(gameObject);ŠÑ’Ê‚·‚é‚Ì‚Å
    //    }
    //}
}
