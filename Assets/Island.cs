using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Island : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap tilemap;
    // �����͗��^�C���A�O���͊C�^�C���i�����Ƃ� 1 ��ނ̂݁j
    public TileBase islandTile;
    public TileBase waterTile;

    [Header("Grid & Region Settings")]
    [Tooltip("�����̈�̕��i���j�b�g�j�B�^�C���T�C�Y��1�Ɖ��肵�܂�")]
    public float regionWidth = 100f;
    [Tooltip("�����̈�̍����i���j�b�g�j")]
    public float regionHeight = 100f;
    [Tooltip("�O���b�h�̗񐔁i�����ł�5�Œ�j")]
    public int gridCols = 5;
    [Tooltip("�O���b�h�̍s���i�����ł�5�Œ�j")]
    public int gridRows = 5;

    [Header("Midpoint Displacement Settings")]
    [Tooltip("���_���炵�̔�����")]
    public int iterations = 3;
    [Tooltip("���񒆓_���炵�ł̍ő傸�炵�ʁi���j�b�g�j")]
    public float initialDisplacement = 10f;
    [Tooltip("�e�������Ƃɂ��炵�ʂ����炷�W�� (��: 0.5�Ȃ甼��������)")]
    public float displacementReduction = 0.5f;

    [Header("Tilemap Fill Settings")]
    [Tooltip("�^�C���}�b�v�̉����i�Z�����j�B�����ł� regionWidth �Ɠ����z��")]
    public int mapWidth = 100;
    [Tooltip("�^�C���}�b�v�̏c���i�Z�����j")]
    public int mapHeight = 100;

    [Header("Regeneration")]
    [Tooltip("�Đ�������L�[�i�f�t�H���g�� Space�j")]
    public KeyCode regenerateKey = KeyCode.Space;

    // �����Ő������ꂽ���̋��E�i���p�`�j���i�[���郊�X�g
    private List<Vector2> islandBoundary;

    void Start()
    {
        GenerateAndFillIsland();
    }

    void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            GenerateAndFillIsland();
        }
    }

    /// <summary>
    /// ���̋��E�i���p�`�j�𐶐����A���̓����^�O���ɂ���ă^�C���}�b�v�ɗ��^�C�^�C����z�u����B
    /// </summary>
    void GenerateAndFillIsland()
    {
        // Step 1: ���̋��E�i���p�`�j�𐶐�
        islandBoundary = GenerateIslandPolygon();

        // Step 2: ���̑��p�`�̓��O�𔻒肵�A�^�C���}�b�v�ɗ��Ƃ�����
        FillTilemapFromPolygon(islandBoundary);
    }

    /// <summary>
    /// 5�~5�̃O���b�h�i�p�Z�������j���珉���̋��E�_�����AMidpoint Displacement�ōו������ē����E�𐶐�����
    /// </summary>
    List<Vector2> GenerateIslandPolygon()
    {
        List<Vector2> initialPoints = new List<Vector2>();
        float cellWidth = regionWidth / gridCols;
        float cellHeight = regionHeight / gridRows;

        // ��s�F�s = gridRows - 1�A�� 1�`gridCols-2
        int topRow = gridRows - 1;
        for (int col = 1; col < gridCols - 1; col++)
        {
            initialPoints.Add(RandomPointInCell(col, topRow, cellWidth, cellHeight));
        }
        // �E��F�� = gridCols - 1�A�s gridRows-2�`1 (������)
        int rightCol = gridCols - 1;
        for (int row = gridRows - 2; row >= 1; row--)
        {
            initialPoints.Add(RandomPointInCell(rightCol, row, cellWidth, cellHeight));
        }
        // ���s�F�s = 0�A�� gridCols-2�`1 (�t����)
        int bottomRow = 0;
        for (int col = gridCols - 2; col >= 1; col--)
        {
            initialPoints.Add(RandomPointInCell(col, bottomRow, cellWidth, cellHeight));
        }
        // ����F�� = 0�A�s 1�`gridRows-2
        int leftCol = 0;
        for (int row = 1; row < gridRows - 1; row++)
        {
            initialPoints.Add(RandomPointInCell(leftCol, row, cellWidth, cellHeight));
        }
        // �����̋��E�_�� 12 �_�ɂȂ�

        List<Vector2> poly = new List<Vector2>(initialPoints);
        // Midpoint Displacement �� iterations ����{
        for (int i = 0; i < iterations; i++)
        {
            float disp = initialDisplacement * Mathf.Pow(displacementReduction, i);
            poly = SubdividePolygon(poly, disp);
        }
        return poly;
    }

    // �w�肳�ꂽ�O���b�h�Z���icol, row�j�̓����ŁA�Z���̒[������������_���ȓ_��Ԃ�
    Vector2 RandomPointInCell(int col, int row, float cellW, float cellH)
    {
        float marginX = cellW * 0.1f;
        float marginY = cellH * 0.1f;
        float x = col * cellW + Random.Range(marginX, cellW - marginX);
        float y = row * cellH + Random.Range(marginY, cellH - marginY);
        return new Vector2(x, y);
    }

    // �|���S���̊e�G�b�W�̒��_�����߁A�����_���ɂ��炵�ĐV�������_�Ƃ��đ}��
    List<Vector2> SubdividePolygon(List<Vector2> poly, float maxDisp)
    {
        List<Vector2> newPoly = new List<Vector2>();
        int count = poly.Count;
        for (int i = 0; i < count; i++)
        {
            Vector2 p0 = poly[i];
            Vector2 p1 = poly[(i + 1) % count];
            newPoly.Add(p0);
            Vector2 mid = (p0 + p1) / 2f;
            mid.x += Random.Range(-maxDisp, maxDisp);
            mid.y += Random.Range(-maxDisp, maxDisp);
            newPoly.Add(mid);
        }
        return newPoly;
    }

    /// <summary>
    /// �^�C���}�b�v�̊e�Z���i�^�C���̒��S�j���A���p�`�i�����E�j�̓������ǂ������肵�A
    /// �����Ȃ瓇�^�C���A�O���Ȃ�C�^�C����z�u����B
    /// </summary>
    void FillTilemapFromPolygon(List<Vector2> poly)
    {
        tilemap.ClearAllTiles();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // �e�^�C���̒��S���W�i�^�C���T�C�Y 1 ���j�b�g�Ƃ��āj
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = IsPointInPolygon(p, poly);
                TileBase chosenTile = inside ? islandTile : waterTile;
                tilemap.SetTile(new Vector3Int(x, y, 0), chosenTile);
            }
        }
    }

    /// <summary>
    /// Ray Casting �@�ɂ��_�����p�`�����ɂ��邩�̔���
    /// </summary>
    bool IsPointInPolygon(Vector2 point, List<Vector2> poly)
    {
        bool inside = false;
        int count = poly.Count;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 pi = poly[i];
            Vector2 pj = poly[j];
            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
