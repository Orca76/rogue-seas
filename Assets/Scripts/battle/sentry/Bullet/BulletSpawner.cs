using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    BulletBase bulletBase;
    public GameObject[] bullet;
    public int BaseBulletID;//�ǂ̋ʂ��g���H
 //   public int AttackPatternID;//�ǂ̑ł����H
    public float spreadAngle = 15f; // �}�p�x

    public float radius= 0.8f;
    void Start()
    {
        bulletBase = GetComponent<BulletBase>();//���g�ɂ��Ă���
        Spawn();
        Destroy(gameObject, 5);
    }

    public enum FirePattern { Single, ThreeWay, AllWay, RandomGate }

    public FirePattern pattern;

    void Spawn()
    {
        switch (pattern)
        {
            case FirePattern.Single:
                SpawnSingle();
                break;
            case FirePattern.ThreeWay:
                SpawnThreeWay();
                break;
            case FirePattern.AllWay:
                SpawnAllWay();
                break;
            case FirePattern.RandomGate:
                SpawnRandom();
                break;
        }
    }

    public void SpawnSingle()
    {
        var obj = Instantiate(bullet[BaseBulletID], transform.position, transform.rotation);
        obj.GetComponent<BulletBase>().Damage = bulletBase.Damage;//�_���[�W���p��


    }
    public void SpawnThreeWay()
    {
        for (int i = -1; i <= 1; i++)
        {
            var rot = Quaternion.AngleAxis(i * spreadAngle, Vector3.forward) * transform.rotation;
            var obj = Instantiate(bullet[BaseBulletID], transform.position, rot);

            var bb = obj.GetComponent<BulletBase>();
            if (bb != null) bb.Damage = bulletBase.Damage/3; // �_���[�W���p��
        }

    }

    public void SpawnAllWay()
    {
        int count = 8;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            var rot = Quaternion.AngleAxis(step * i, Vector3.forward) * transform.rotation;
            var obj = Instantiate(bullet[BaseBulletID], transform.position, rot);

            var bb = obj.GetComponent<BulletBase>();
            if (bb != null) bb.Damage = bulletBase.Damage/8; // �_���[�W���p��
        }
    }

    public void SpawnRandom()
    {
        int count = Random.Range(3, 8 + 1);

        for (int i = 0; i < count; i++)
        {
            // �����_���ȉ~����̈ʒu
            Vector2 offset = Random.insideUnitCircle.normalized * radius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

           

            // �e����
            var obj = Instantiate(bullet[BaseBulletID], spawnPos, transform.rotation);

            var bb = obj.GetComponent<BulletBase>();
            if (bb != null) bb.Damage = bulletBase.Damage/count; // �_���[�W���p��
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
