using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// �^�C���P�ʂ̏������C�e�B���O���Ǘ�����R���|�[�l���g
/// �EInit(tilemap, isGroundLayer) �ŏ�����
/// �EAddTorch(cell) / RemoveTorch(cell) �������v���n�u����Ă�
/// �ELateUpdate �� dirty �ȃZ������ Color ���Đݒ�
/// </summary>
public class TileLighting : MonoBehaviour
{
    // ������������������ �ݒ�l ������������������
    [SerializeField] int torchPower = 8;          // ���a 8 �}�X
    [SerializeField] Color litColor = Color.white;
    [SerializeField] Color darkColor = Color.black;

    // ������������������ ������� ������������������
    Tilemap map;
    int[,] lightMap;             // ���� 0-8
    List<Vector3Int> dirty = new();   // �F���X�V����Z��

    int w, h;                     // �}�b�v���E����
    int ox, oy;                   // map.cellBounds.min �p�I�t�Z�b�g�i�����W�΍�j

    static readonly Vector3Int[] DIR4 = {
        new ( 1, 0, 0), new (-1, 0, 0),
        new ( 0, 1, 0), new ( 0,-1, 0)
    };

    /*��������������������������������������������������������������������������������������������
      ������
      �P�P�P�P
      isGroundLayer == true �Ȃ�S�Z������ 8 �ɂ���
      �n�ヌ�C���p�ɂ���
    ��������������������������������������������������������������������������������������������*/
    public void Init(Tilemap tilemap, bool isGroundLayer)
    {
        map = tilemap;

        BoundsInt b = map.cellBounds;         // �g�p�̈�
        w = b.size.x;
        h = b.size.y;
        ox = b.xMin;                          // x,y ���}�C�i�X�̎��̕␳
        oy = b.yMin;

        lightMap = new int[w, h];

        int initial = isGroundLayer ? 8 : 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                lightMap[x, y] = initial;
    }

    /*��������������������������������������������������������������������������������������������
      ������u�� / ��
    ��������������������������������������������������������������������������������������������*/
    public void AddTorch(Vector3Int cell) { PropagateLight(cell, +torchPower); }
    public void RemoveTorch(Vector3Int cell) { PropagateLight(cell, -torchPower); }

    void PropagateLight(Vector3Int cell, int deltaPower)
    {
        // �Z�����W��z��C���f�b�N�X�ɕϊ�
        Vector3Int c = cell - new Vector3Int(ox, oy, 0);

        Queue<Vector3Int> q = new(); HashSet<Vector3Int> seen = new();
        q.Enqueue(c); seen.Add(c);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            int dist = Mathf.Abs(p.x - c.x) + Mathf.Abs(p.y - c.y);
            if (dist > torchPower) continue;

            int newL = Mathf.Clamp(lightMap[p.x, p.y] + deltaPower - dist, 0, 8);
            if (newL == lightMap[p.x, p.y]) continue;          // �ω��Ȃ�

            lightMap[p.x, p.y] = newL;
            dirty.Add(p);

            foreach (var d in DIR4)
            {
                var n = p + d;
                if (n.x < 0 || n.x >= w || n.y < 0 || n.y >= h) continue;
                if (!seen.Add(n)) continue;
                q.Enqueue(n);
            }
        }
    }

    /*��������������������������������������������������������������������������������������������
      �ύX���ꂽ�Z���������_�J���[����������
    ��������������������������������������������������������������������������������������������*/
    void LateUpdate()
    {
        if (dirty.Count == 0) return;

        foreach (var p in dirty)
        {
            int l = lightMap[p.x, p.y];          // 0-8
            float t = 1f - l / 8f;               // 0=�� 1=��
            Color col = Color.Lerp(litColor, darkColor, t);

            // �z�񁨃^�C�����W�ɖ߂�
            var world = new Vector3Int(p.x + ox, p.y + oy, 0);
            map.SetColor(world, col);
        }
        dirty.Clear();
    }
}
