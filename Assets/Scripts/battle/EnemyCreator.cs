using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCreator : MonoBehaviour
{
    public int enemyLimitNum;         // �ő�G��
    public float Radiusmin;           // �ŏ��X�|�[������
    public float RadiusMax;           // �ő�X�|�[������
    public GameObject[] Enemies;      // �G�v���n�u���X�g
    public float CreateSpan;          // �G�����Ԋu

    GameObject player;
    float lastSpawnTime;

    void Start()
    {
        player = GameObject.FindWithTag("Player"); // �v���C���[���擾
        lastSpawnTime = Time.time;
    }

    void Update()
    {
        // �G���ő吔�ȉ����A�����Ԋu�𖞂�������X�|�[��
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
        // "Enemy" �^�O���t���Ă���G���J�E���g�i���O�Ƀv���n�u�� Enemy �^�O�����āj
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
