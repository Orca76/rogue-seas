using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float BulletSpeed;//�e�ۈړ����x
    public float Damage;//�e�ۉΗ�
    public float LifeTime;//�e�ێ���

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,LifeTime);
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * BulletSpeed * Time.deltaTime);
    }
}
