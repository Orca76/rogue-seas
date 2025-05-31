using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class BlockBase : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2Int tilePos;

    [SerializeField] public IslandTile islandTiles;  // �� Spawner ���Z�b�g


    [SerializeField] int maxHp = 1;
    int hp;

    void Awake()
    {
        hp = maxHp;
        // ���̏����� �c

        /* �� �ی��R�[�h��ǋL */
        if (islandTiles == null)
            islandTiles = FindObjectOfType<IslandTile>();
    }
    void Start()
    {
        
    }
    public virtual void Damage(int amt = 1)
    {
        hp -= amt;
        if (hp <= 0) Break();               // HP ���s������j��
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact()
    {
        Debug.Log($"Block at {tilePos} was interacted with.");
        //if (hp <= 0)
        //{
        //    RockDepthManager.Instance.RequestRebake(tilePos); // �� NEW
        //    Destroy(gameObject);
        //}
    }
    public virtual void Break()
    {
        // �@ Tilemap �̃Z������ɂ���
        Vector3Int cell = new(tilePos.x, tilePos.y, 0);
        islandTiles.tilemapSurface.SetTile(cell, null);
        islandTiles.tileMapData[tilePos.x, tilePos.y] = 0;

        // �A �[�x�}�l�[�W���ɍČv�Z�v��
        RockDepthManager.Instance.RequestRebake(tilePos);

        // �B �v���n�u���g���폜
        Destroy(gameObject);
        //// �@ Tilemap �̃Z�������^�C���ɍ����ւ�
        //TileBase floor = islandTiles.mountainTile;   // �� �C���X�y�N�^�ŃZ�b�g
        //islandTiles.tilemapSurface.SetTile(cell, floor);
        //islandTiles.tileMapData[tilePos.x, tilePos.y] = 37; // �f�[�^���X�V
    }
  
}
