using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBlock : BlockBase
{
    public Vector3Int mapPosition;           // ��̍��W
    public IslandTile islandTiles;          // �f�[�^�ێ����̎Q�ƁitileMapData�����j

    public override void Interact()
    {
        // 1. �_���f�[�^���X�V
        islandTiles.tileMapData[tilePos.x, tilePos.y] = 30;//30�͖��Ȃ̂Ł@31����̃R�[�h
                                                                   // 3. �f�o�b�O�o��
        Debug.Log($"���j��I�ʒu: {tilePos.x}, {tilePos.y}");
        // 2. �����ڂ̍폜
        Destroy(gameObject);

       
    }
}
