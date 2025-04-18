using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectSpawner : MonoBehaviour
{
    public IslandTile tileController; // tileMapData�������Ă���
    public GameObject tileMapObj;//�^�C���}�b�v
    public Transform player;
    public GameObject rockPrefab;

    public int viewRange = 10; // ��F10�}�X�ȓ��ɂ�����̂�������
                               // 1�^�C����0.16���j�b�g�̏ꍇ
    float tileSize = 0.16f;
    Dictionary<Vector2Int, GameObject> spawnedObjects = new();
    private Vector3 lastPlayerPos;
    private void Start()
    {
        tileController = tileMapObj.GetComponent<IslandTile>();
        lastPlayerPos = player.position;
        //  Debug.Log(tileController.tileMapData);
    }
    void Update()
    {

        // �v���C���[�̍��W���ς�����珈�������s
        if (!Mathf.Approximately((player.position - lastPlayerPos).sqrMagnitude, 0f) || Input.GetKeyDown(KeyCode.U)||Input.GetKeyDown(KeyCode.I))
        {
            lastPlayerPos = player.position;
            RefreshAroundPlayer();
        }
       
    }

    void RefreshAroundPlayer()
    {
        Vector2 playerPos = player.position;
        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / tileSize),
            Mathf.FloorToInt(playerPos.y / tileSize)
        );
        //  Debug.Log(playerTile.ToString());
        for (int x = playerTile.x - viewRange; x <= playerTile.x + viewRange; x++)
        {
            for (int y = playerTile.y - viewRange; y <= playerTile.y + viewRange; y++)
            {
                if (!IsInBounds(x, y)) continue;

                Vector2Int pos = new Vector2Int(x, y);
                int id = tileController.tileMapData[x, y];


                if (id == 31)
                {
                    if (!spawnedObjects.ContainsKey(pos))
                    {
                        Vector3 worldPos = new Vector3(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f, 0f);
                        GameObject obj = Instantiate(rockPrefab, worldPos, Quaternion.identity);
                        spawnedObjects[pos] = obj;

                        RockBlock rb = obj.GetComponent<RockBlock>();
                        rb.tilePos = new Vector2Int(x, y);
                        rb.islandTiles = tileController;
                    }
                    else
                    {
                        float playerZ = player.position.z;
                        float objZ = spawnedObjects[pos].transform.position.z;

                        if (Mathf.Approximately(playerZ, objZ))
                        {
                            spawnedObjects[pos].SetActive(true);
                        }
                    }
                }
            }
        }

        // �͈͊O�̃I�u�W�F�N�g�͍폜
        List<Vector2Int> toRemove = new();
        foreach (var kvp in spawnedObjects)
        {
            if (Vector2Int.Distance(kvp.Key, playerTile) > viewRange)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
            spawnedObjects.Remove(key);
    }

    bool IsInBounds(int x, int y)
    {
        if (tileController == null || tileController.tileMapData == null)
        {
            Debug.Log(tileController.tileMapData);
            return false;
        }

        return x >= 0 && y >= 0 &&
               x < tileController.tileMapData.GetLength(0) &&
               y < tileController.tileMapData.GetLength(1);
    }
}
