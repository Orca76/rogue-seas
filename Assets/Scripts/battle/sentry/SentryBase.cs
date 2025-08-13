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
    // �ړ����x�i�����p�j
    public float moveSpeed = 3f;

    // �G�ɋ߂Â�����
    public float approachDistance;

    private GameObject targetEnemy; // ���݂̃^�[�Q�b�g�G

    public GameObject Player;
    Player PlayerData;//�X�e�[�^�X�Ƃ�
    SentryManager managerSc;//���݂̎w��
    float recharge;//�����p�̃��`���[�W����


    public DamagePopup popupPrefab;
    void Start()
    {
        // ������������΂�����
        Player = GameObject.Find("Player");
        managerSc=Player.GetComponent<SentryManager>();
        PlayerData = Player.GetComponent<Player>();
      //  HP = MaxHP;
    }



    void DestroySentry()
    {
        //���񂾂Ƃ��̏������ꂱ��
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

    private void Shoot()//���Ԋu�őł�
    {
        if (recharge > AttackSpeed && targetEnemy != null)
        {
            // �G�̕������v�Z
            Vector3 dir = (targetEnemy.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // �e�ې����i������G�����ɂ���j
            GameObject bullet = Instantiate(BulletPrefab, transform.position, Quaternion.Euler(0f, 0f, angle - 90f));

            // �_���[�W�ݒ�
            bullet.GetComponent<PlayerBullet>().Damage = Power;

            recharge = 0;
        }
    }
    private void UpdateStats()//�X�e�[�^�X�X�V
    {
        MaxHP = BaseHP * PlayerData.levels[0];
        Power=BasePower * PlayerData.levels[1];
        AttackSpeed=1/(BaseAttackSpeed * PlayerData.levels[2]);
    }


    // �Ŋ��̓G��T��
    GameObject UpdateTargetEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float nearestDistSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - currentPos;
            float distSqr = diff.sqrMagnitude; // �����̓��Ŕ�r�i�������j

            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearest = enemy;
            }
        }

        //targetEnemy = nearest;
        return nearest;
    }

    // �G�Ɉ�苗���܂ŋ߂Â�����
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
            // ��苗�����Ȃ̂Œ�~�i�U�������͂����ɓ����\��j
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "EnemyAt")
        {
            HP -= collision.GetComponent<EnemyBullet>().Damage;
            // �\���ʒu�i���L�����̏�����j
            var pos = transform.position;
            DamagePopup.Spawn(popupPrefab, pos, Mathf.RoundToInt(collision.GetComponent<EnemyBullet>().Damage));

            Destroy(collision.gameObject);
        }
    }
}
