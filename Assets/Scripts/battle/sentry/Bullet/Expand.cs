using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expand : BulletBase
{
    // Start is called before the first frame update
    float firstScale;//最初のスケールを保持
   protected override void Start()
    {
        base.Start();
        firstScale = gameObject.transform.localScale.x;
        ispenetrate = true;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        Vector3 scale = transform.localScale;
        float max = firstScale * 5f;
        float grow = 1f * Time.deltaTime; // 拡大スピード調整

        if (scale.x < max)
        {
            // 等倍拡大
            scale += Vector3.one * grow;
            transform.localScale = scale;
        }

    }
    //protected override void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        collision.GetComponent<EnemyBase>().HP -= Damage;
    //       //貫通
    //    }
    //}
}
