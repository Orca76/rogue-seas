using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCreator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject spawnPos;
    void Awake()
    {
        // �V�[�����Ɋ���Player�^�O�����I�u�W�F�N�g�����݂��邩�`�F�b�N
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            // ���Ȃ���ΐ���
            Instantiate(playerPrefab, spawnPos.transform.position, Quaternion.identity);
        }
        // ����΁iDontDestroyOnLoad�ŗ����ꍇ�j�͉������Ȃ�
    }
}


