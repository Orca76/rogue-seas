using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{
    // Start is called before the first frame update
    public float HP;//敵の体力
    public float MaxHP;
    public float AttackSpan;//敵の攻撃速度
    public GameObject Bullet;//攻撃に使う弾丸 
    GameObject player; float attackTimer = 0f;
  public  GameObject target;
    public float moveSpeed;//移動速度

    public GameObject Soul;// 所謂経験値オブジェクト
    public int ExpValue;//倒した時の経験値量

    public float LimitDistance;//これ以上離れたら消える

    public DamagePopup popup;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        Debug.Log($"[ELG] Start {name}, playing={Application.isPlaying}");
        MaxHP = HP;
    }

    public Image HPBar;
    // Update is called once per frame

  
    void Update()
    {
        if(HPBar != null) HPBar.fillAmount = HP / MaxHP;

        if ( player == null )
        {
            player = GameObject.FindWithTag("Player");
        }
        if (HP <= 0)
        {
            //死んだときの処理
            GameObject.Find("enemyCreator").GetComponent<EnemyCreator>().KilledEnemyNum++;
            GameObject soul = Instantiate(Soul, transform.position, transform.rotation);
            SoulBase soulBase = soul.GetComponent<SoulBase>();
            soulBase.soulValue = ExpValue;
            Destroy(gameObject);
        }

        attackTimer += Time.deltaTime;
        MoveTowardPlayer();  // ← プレイヤーに近づく
        if (attackTimer >= AttackSpan)
        {


            if (player != null)
            {
                // 弾の向き：プレイヤーの方向
                target = FindNearestPlayerOrSentry(gameObject.transform.position);
                Vector3 dir = (target.transform.position - transform.position).normalized;

                // プレイヤーの方向に回転させて弾を生成
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, dir);
                Instantiate(Bullet, transform.position, rot);
            }
            attackTimer = 0f;
        }
        if (Vector2.Distance(player.transform.position, gameObject.transform.position) > LimitDistance)
        {
            Destroy(gameObject);
        }

    }
    void MoveTowardPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.transform.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    //最も近いプレイヤーたち
    public GameObject FindNearestPlayerOrSentry(Vector3 fromPosition)
    {
        // Sentryタグだけ取得
        GameObject[] sentries = GameObject.FindGameObjectsWithTag("Sentry");

        // リストにplayerを追加
        GameObject[] allTargets = sentries.Concat(new GameObject[] { player }).ToArray();

        // 何もなければnull返す
        if (allTargets.Length == 0) return null;

        // 最も近いものを探索
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject target in allTargets)
        {
            if (target == null) continue; // 念のため
            float dist = Vector3.Distance(fromPosition, target.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = target;
            }
        }

        return nearest;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PlayerAt")
        {
            HP -= collision.GetComponent<BulletBase>().Damage;
            var pos = transform.position;
            DamagePopup.Spawn(popup, pos, Mathf.RoundToInt(collision.GetComponent<BulletBase>().Damage));

            if (!collision.GetComponent<BulletBase>().ispenetrate)
            {
                Destroy(collision.gameObject);
            }
           
        }
    }
}
