using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryBase : MonoBehaviour
{

    [HideInInspector] public float BaseHP;
    public float HP;
    public float MaxHP;
    [HideInInspector] public float BasePower;
    public float Power;
    [HideInInspector] public float BaseAttackSpeed;
    public float AttackSpeed;
    public GameObject BulletPrefab;

    public int Rarity;//0 1 2 norma rare Srare
    // 移動速度（調整用）
    public float moveSpeed = 3f;

    // 敵に近づく距離
    public float approachDistance;

    private GameObject targetEnemy; // 現在のターゲット敵

    public GameObject Player;
    Player PlayerData;//ステータスとか
    SentryManager managerSc;//現在の指示
    float recharge;//撃つ時用のリチャージ時間


    public DamagePopup popupPrefab;
    void Start()
    {
        // 初期化があればここに
        Player = GameObject.Find("Player");
        managerSc=Player.GetComponent<SentryManager>();
        PlayerData = Player.GetComponent<Player>();
      //  HP = MaxHP;
    }



    void DestroySentry()
    {
        //死んだときの処理あれこれ
        managerSc.sentryList.Remove(gameObject);

        Destroy(gameObject);
    }
    void Update()
    {
        
        UpdateStats();


        if (HP <= 0)
        {
            DestroySentry();
        }
        targetEnemy = UpdateTargetEnemy();
        if ((managerSc.currentState == SentryManager.SentryState.Disperse))
        {
            
            MoveTowardsEnemy();
        }
        recharge += Time.deltaTime;

        Shoot();
       
    }

    private void Shoot()//一定間隔で打つ
    {
        if (recharge > AttackSpeed && targetEnemy != null)
        {
            // 敵の方向を計算
            Vector3 dir = (targetEnemy.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 弾丸生成（向きを敵方向にする）
            GameObject bullet = Instantiate(BulletPrefab, transform.position, Quaternion.Euler(0f, 0f, angle - 90f));

            // ダメージ設定
            bullet.GetComponent<PlayerBullet>().Damage = Power;

            recharge = 0;
        }
    }
    private void UpdateStats()//ステータス更新
    {
        MaxHP = BaseHP * PlayerData.levels[0];
        Power=BasePower * PlayerData.levels[1];
        AttackSpeed=1/(BaseAttackSpeed * PlayerData.levels[2]);
    }


    // 最寄りの敵を探す
    GameObject UpdateTargetEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float nearestDistSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - currentPos;
            float distSqr = diff.sqrMagnitude; // 距離の二乗で比較（効率化）

            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearest = enemy;
            }
        }

        //targetEnemy = nearest;
        return nearest;
    }

    // 敵に一定距離まで近づく処理
    void MoveTowardsEnemy()
    {
        if (targetEnemy == null) return;

        Vector3 direction = targetEnemy.transform.position - transform.position;
        float distance = direction.magnitude;

        if (distance > approachDistance)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 一定距離内なので停止（攻撃処理はここに入れる予定）
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "EnemyAt")
        {
            HP -= collision.GetComponent<EnemyBullet>().Damage;
            // 表示位置（自キャラの少し上）
            var pos = transform.position;
            DamagePopup.Spawn(popupPrefab, pos, Mathf.RoundToInt(collision.GetComponent<EnemyBullet>().Damage));

            Destroy(collision.gameObject);
        }
    }
}
