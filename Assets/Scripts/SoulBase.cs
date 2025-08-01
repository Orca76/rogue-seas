using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulBase : MonoBehaviour
{
    [Header("ソウルの設定")]
    [Tooltip("このソウルがプレイヤーに与える経験値量")]
    public int soulValue = 10;  // 経験値値

    [Tooltip("プレイヤーがこの距離以内に入ると吸い寄せ開始")]
    public float attractRadius = 5f;

    [Tooltip("吸い寄せ時の移動速度")]
    public float moveSpeed = 5f;

    private Transform player;

    void Start()
    {
        // プレイヤーを取得（タグで探す）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // プレイヤーとの距離を計算
        float distance = Vector3.Distance(transform.position, player.position);

        // 半径内なら吸い寄せる
        if (distance <= attractRadius)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<Player>().Exp += soulValue;//経験値足す
            Destroy(gameObject);
        }
    }


}
