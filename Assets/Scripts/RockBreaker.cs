using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RockBreaker : MonoBehaviour
{
    public Tilemap rockTilemap; // ��̃^�C���}�b�v
    public TileBase destroyedTile; // ����󂵂���̌����ځinull�ł��j

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���N���b�N
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = rockTilemap.WorldToCell(mouseWorldPos);

            if (rockTilemap.HasTile(cellPos))
            {
                // �󂷏���
                rockTilemap.SetTile(cellPos, destroyedTile); // or null
                Debug.Log("���j��I");
            }
        }
    }
}
