using System.Collections.Generic;
using UnityEngine;

public class ChartVectorManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameClock gameClock; // ���Ԃ���p�x���擾
    public GameObject chartTileObject; // �C�}�̐e�I�u�W�F�N�g
    public GameObject vectorPrefab; // LineRenderer�����v���n�u
    public KeyCode generateVectorKey = KeyCode.O;
    public KeyCode resetKey = KeyCode.T;

    [Header("Vector Settings")]
    public float segmentLength = 2f; // 1���@�Ε��̃x�N�g����
    public Vector3 origin = new Vector3(16, 16, 0); // ���S�_

    private List<GameObject> vectorObjects = new();
    private List<Vector2> vectorSegments = new();

    public ChartTile chartTileScript;

    void Update()
    {
        if (!chartTileObject.activeSelf) return;

        if (Input.GetKeyDown(generateVectorKey))
        {
            GenerateVector();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetVectors();
        }
    }

    void GenerateVector()
    {
        float angle = gameClock.GetAngleForCurrentTime(); // 0?360�x
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        Vector2 segment = direction * segmentLength;

        vectorSegments.Add(segment);

        Vector3 start = origin;
        for (int i = 0; i < vectorSegments.Count - 1; i++)
        {
            start += (Vector3)vectorSegments[i];
        }
        Vector3 end = start + (Vector3)segment;

        GameObject vecObj = Instantiate(vectorPrefab, Vector3.zero, Quaternion.identity, transform);
        LineRenderer lr = vecObj.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startColor = lr.endColor = new Color(Random.value, Random.value, Random.value);

        vectorObjects.Add(vecObj);

        Vector3Int tilePos = chartTileScript.tilemapChart.WorldToCell(end); // �^�C�����W�֕ϊ�
        int zoneType = chartTileScript.GetZoneTypeAt(tilePos.x, tilePos.y);
        string zoneName = zoneType switch
        {
            0 => "���g�C��",
            1 => "����C��",
            2 => "�Z���C��",
            3 => "�\���C��",
            4 => "�L�@�C��",
            5 => "�_��C��",
            _ => "���m"
        };

        Debug.Log($"�o�q�x�N�g����[: ({tilePos.x}, {tilePos.y}) - �C��: {zoneName}");
    }

    void ResetVectors()
    {
        foreach (var obj in vectorObjects)
        {
            Destroy(obj);
        }
        vectorObjects.Clear();
        vectorSegments.Clear();
    }
}
