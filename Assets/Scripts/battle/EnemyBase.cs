using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // Start is called before the first frame update
    public float HP;//敵の体力
    public float AttackSpan;//敵の攻撃速度
    public GameObject Bullet;//攻撃に使う弾丸 
    GameObject player; float attackTimer = 0f;

    public float moveSpeed;//移動速度


    void Start()
    {
        player = GameObject.Find("Player");   
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer += Time.deltaTime;
        MoveTowardPlayer();  // ← プレイヤーに近づく
        if (attackTimer >= AttackSpan)
        {
           

            if (player != null)
            {
                // 弾の向き：プレイヤーの方向
                Vector3 dir = (player.transform.position - transform.position).normalized;

                // プレイヤーの方向に回転させて弾を生成
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, dir);
                Instantiate(Bullet, transform.position, rot);
            }
            attackTimer = 0f;
        }
    }
    void MoveTowardPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.transform.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
