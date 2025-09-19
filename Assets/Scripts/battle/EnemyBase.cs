using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{
    // Start is called before the first frame update
    public float HP;//�G�̗̑�
    public float MaxHP;
    public float AttackSpan;//�G�̍U�����x
    public GameObject Bullet;//�U���Ɏg���e�� 
    GameObject player; float attackTimer = 0f;
  public  GameObject target;
    public float moveSpeed;//�ړ����x

    public GameObject Soul;// �����o���l�I�u�W�F�N�g
    public int ExpValue;//�|�������̌o���l��

    public float LimitDistance;//����ȏ㗣�ꂽ�������

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
            //���񂾂Ƃ��̏���
            GameObject.Find("enemyCreator").GetComponent<EnemyCreator>().KilledEnemyNum++;
            GameObject soul = Instantiate(Soul, transform.position, transform.rotation);
            SoulBase soulBase = soul.GetComponent<SoulBase>();
            soulBase.soulValue = ExpValue;
            Destroy(gameObject);
        }

        attackTimer += Time.deltaTime;
        MoveTowardPlayer();  // �� �v���C���[�ɋ߂Â�
        if (attackTimer >= AttackSpan)
        {


            if (player != null)
            {
                // �e�̌����F�v���C���[�̕���
                target = FindNearestPlayerOrSentry(gameObject.transform.position);
                Vector3 dir = (target.transform.position - transform.position).normalized;

                // �v���C���[�̕����ɉ�]�����Ēe�𐶐�
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

    //�ł��߂��v���C���[����
    public GameObject FindNearestPlayerOrSentry(Vector3 fromPosition)
    {
        // Sentry�^�O�����擾
        GameObject[] sentries = GameObject.FindGameObjectsWithTag("Sentry");

        // ���X�g��player��ǉ�
        GameObject[] allTargets = sentries.Concat(new GameObject[] { player }).ToArray();

        // �����Ȃ����null�Ԃ�
        if (allTargets.Length == 0) return null;

        // �ł��߂����̂�T��
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject target in allTargets)
        {
            if (target == null) continue; // �O�̂���
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
