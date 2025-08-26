using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    BulletBase bulletBase;
    public GameObject[] bullet;
    public int BaseBulletID;//どの玉を使う？
 //   public int AttackPatternID;//どの打ち方？
    public float spreadAngle = 15f; // ±角度

    public float radius= 0.8f;
    void Start()
    {
        bulletBase = GetComponent<BulletBase>();//自身についている
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
        obj.GetComponent<BulletBase>().Damage = bulletBase.Damage;//ダメージ引継ぎ


    }
    public void SpawnThreeWay()
    {
        for (int i = -1; i <= 1; i++)
        {
            var rot = Quaternion.AngleAxis(i * spreadAngle, Vector3.forward) * transform.rotation;
            var obj = Instantiate(bullet[BaseBulletID], transform.position, rot);

            var bb = obj.GetComponent<BulletBase>();
            if (bb != null) bb.Damage = bulletBase.Damage/3; // ダメージ引継ぎ
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
            if (bb != null) bb.Damage = bulletBase.Damage/8; // ダメージ引継ぎ
        }
    }

    public void SpawnRandom()
    {
        int count = Random.Range(3, 8 + 1);

        for (int i = 0; i < count; i++)
        {
            // ランダムな円周上の位置
            Vector2 offset = Random.insideUnitCircle.normalized * radius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

           

            // 弾生成
            var obj = Instantiate(bullet[BaseBulletID], spawnPos, transform.rotation);

            var bb = obj.GetComponent<BulletBase>();
            if (bb != null) bb.Damage = bulletBase.Damage/count; // ダメージ引継ぎ
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
