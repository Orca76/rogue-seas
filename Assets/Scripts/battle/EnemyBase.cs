using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // Start is called before the first frame update
    public float HP;//�G�̗̑�
    public float AttackSpan;//�G�̍U�����x
    public GameObject Bullet;//�U���Ɏg���e�� 
    GameObject player; float attackTimer = 0f;

    public float moveSpeed;//�ړ����x


    void Start()
    {
        player = GameObject.Find("Player");   
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer += Time.deltaTime;
        MoveTowardPlayer();  // �� �v���C���[�ɋ߂Â�
        if (attackTimer >= AttackSpan)
        {
           

            if (player != null)
            {
                // �e�̌����F�v���C���[�̕���
                Vector3 dir = (player.transform.position - transform.position).normalized;

                // �v���C���[�̕����ɉ�]�����Ēe�𐶐�
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
