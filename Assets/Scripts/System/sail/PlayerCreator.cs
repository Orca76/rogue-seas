using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCreator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject spawnPos;
    void Awake()
    {
        // シーン内に既にPlayerタグを持つオブジェクトが存在するかチェック
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            // いなければ生成
            Instantiate(playerPrefab, spawnPos.transform.position, Quaternion.identity);
        }
        // いれば（DontDestroyOnLoadで来た場合）は何もしない
    }
}


