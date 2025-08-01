using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulBase : MonoBehaviour
{
    [Header("�\�E���̐ݒ�")]
    [Tooltip("���̃\�E�����v���C���[�ɗ^����o���l��")]
    public int soulValue = 10;  // �o���l�l

    [Tooltip("�v���C���[�����̋����ȓ��ɓ���Ƌz���񂹊J�n")]
    public float attractRadius = 5f;

    [Tooltip("�z���񂹎��̈ړ����x")]
    public float moveSpeed = 5f;

    private Transform player;

    void Start()
    {
        // �v���C���[���擾�i�^�O�ŒT���j
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // �v���C���[�Ƃ̋������v�Z
        float distance = Vector3.Distance(transform.position, player.position);

        // ���a���Ȃ�z���񂹂�
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
            collision.GetComponent<Player>().Exp += soulValue;//�o���l����
            Destroy(gameObject);
        }
    }


}
