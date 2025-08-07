using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    public int enemyLimitNum;         // 最大敵数
    public float Radiusmin;           // 最小スポーン距離
    public float RadiusMax;           // 最大スポーン距離
    public GameObject[] Enemies;      // 敵プレハブリスト
    public float CreateSpan;          // 敵生成間隔

    GameObject player;
    float lastSpawnTime;

    void Start()
    {
        player = GameObject.FindWithTag("Player"); // プレイヤーを取得
        lastSpawnTime = Time.time;
    }

    void Update()
    {
        // 敵が最大数以下かつ、生成間隔を満たしたらスポーン
        if (CountEnemies() < enemyLimitNum && Time.time - lastSpawnTime >= CreateSpan)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = setCreatePosition();
        GameObject prefab = Enemies[Random.Range(0, Enemies.Length)];
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    Vector2 setCreatePosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.value) * (RadiusMax - Radiusmin) + Radiusmin;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return (Vector2)player.transform.position + offset;
    }

    int CountEnemies()
    {
        // "Enemy" タグが付いている敵をカウント（事前にプレハブに Enemy タグをつけて）
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
