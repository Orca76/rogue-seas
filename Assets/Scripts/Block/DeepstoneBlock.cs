using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepstoneBlock : BlockBase
{

   // public IslandTile islandTiles;          // �f�[�^�ێ����̎Q�ƁitileMapData�����j

    public override void Interact()
    {
        // 1. �_���f�[�^���X�V
        islandTiles.tileMapDataUnderground[tilePos.x, tilePos.y] = 40;//40�͖��Ȃ̂Ł@
                                                           // 3. �f�o�b�O�o��
        Debug.Log($"���j��I�ʒu: {tilePos.x}, {tilePos.y}");
        // 2. �����ڂ̍폜
        Destroy(gameObject);


    }
}
