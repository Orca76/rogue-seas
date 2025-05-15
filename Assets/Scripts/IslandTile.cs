using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;
using System.Linq;

public class IslandTile : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemapSurface;
    public Tilemap tilemapUnderground;

    [Header("Surface Tiles")]
    public TileBase[] seaTile;
    public TileBase desertTile;
    public TileBase[] grasslandTile;
    public TileBase forestTile;
    public TileBase mountainTile;

    [Header("Underground Tiles")]
    public TileBase rockTile;
    public TileBase caveTile; // �ʏ��null�ɂ���i�󓴁j
    public TileBase deepstoneTile;//�[�w��
    public TileBase bedrockTile; // ������V�K�i���Ȃ���Ձj

    [Header("Island Settings")]
    public float regionWidth = 100f;
    public float regionHeight = 100f;
    public int gridCols = 5;
    public int gridRows = 5;
    public int iterations = 3;
    public float initialDisplacement = 10f;
    public float displacementReduction = 0.5f;

    [Header("Noise Settings")]
    public float biomeFrequency = 0.05f;
    public float biomeThresholdDesert = -0.3f;
    public float biomeThresholdGrassland = 0.0f;
    public float biomeThresholdForest = 0.4f;

    public float biomeThresholdCave;

    [Header("Map Size")]
    public int mapWidth = 100;
    public int mapHeight = 100;

    [Header("Controls")]
    public KeyCode regenerateKey = KeyCode.Space;
    public KeyCode toggleLayerKey = KeyCode.U;

    private List<Vector2> islandBoundary;
    private int noiseSeed = 123;
    private bool showingUnderground = false;


    public int[,] tileMapData;
    public int[,] tileMapDataUnderground;

    public Transform playerTransform; // �v���C���[���Q�Ƃ��邽�߂ɒǉ�
    public float undergroundZ = 1f;   // �n����Z���W
    public float surfaceZ = 0f;       // �n���Z���W

    private GameObject islandRoot;//��
    private void Awake()
    {
        tileMapData = new int[mapWidth, mapHeight];
        tileMapDataUnderground = new int[mapWidth, mapHeight];
    }
    void Start()
    {
        GenerateAndFillMaps();

    }
    public void CreateIsland()
    {
        if (islandRoot != null) // 
            Destroy(islandRoot);
        islandRoot = new GameObject("IslandRoot");
        noiseSeed = Random.Range(0, 10000);
        GenerateAndFillMaps();
    }


    void Update()
    {



        if (Input.GetKeyDown(regenerateKey))
        {
            CreateIsland();
        }
        //if (Input.GetKeyDown(toggleLayerKey))
        //{
        //    ToggleLayer();
        //}

        // �v���C���[��Z���W�ɉ����ă��C���[��؂�ւ���
        bool playerInUnderground = Mathf.Approximately(playerTransform.position.z, undergroundZ);
        tilemapSurface.gameObject.SetActive(!playerInUnderground);
        tilemapUnderground.gameObject.SetActive(playerInUnderground);
    }

    void GenerateAndFillMaps()
    {
        islandBoundary = GenerateIslandPolygon();
        FillSurfaceMap();
        FillUndergroundMap();
    }

    void ToggleLayer()
    {
        showingUnderground = !showingUnderground;
        tilemapSurface.gameObject.SetActive(!showingUnderground);
        tilemapUnderground.gameObject.SetActive(showingUnderground);
    }

    Vector2 GetNoiseOffset(int seed)
    {
        float offsetX = (seed * 12345 % 1000) / 1000f * 100f;
        float offsetY = (seed * 67890 % 1000) / 1000f * 100f;
        return new Vector2(offsetX, offsetY);
    }

    void FillSurfaceMap()
    {
        tilemapSurface.ClearAllTiles();
        Vector2 biomeOffset = GetNoiseOffset(noiseSeed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                if (!IsPointInPolygon(pos, islandBoundary))
                {
                    tilemapSurface.SetTile(new Vector3Int(x, y, 0), seaTile[Random.Range(0,seaTile.Length)]);
                    continue;
                }
                float biomeNoise = Mathf.PerlinNoise((x + biomeOffset.x) * biomeFrequency, (y + biomeOffset.y) * biomeFrequency) * 2f - 1f;
                TileBase tile = ChooseBiomeTile(biomeNoise);

                if (tile == forestTile)
                {
                    int[] options = { 10 }; // ���@�؁@�ԁ@��
                    tileMapData[x, y] = options[Random.Range(0, options.Length)];
                }
                if (grasslandTile.Contains(tile))
                {
                    int[] options = { 20 }; // ���@�؁@�ԁ@��
                    tileMapData[x, y] = options[Random.Range(0, options.Length)];
                }
                if (tile == mountainTile)
                {
                    int[] options = { 30, 31 }; //�� ��@�z�΁@�z�΁@�z��
                    tileMapData[x, y] = 31;//�܂��͊m��Ŋ�𐶐�
                }

                tilemapSurface.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    void FillUndergroundMap()
    {
        tilemapUnderground.ClearAllTiles();
        Vector2 biomeOffset = GetNoiseOffset(noiseSeed + 999);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3Int pos3 = new Vector3Int(x, y, 0);
                Vector2 pos2 = new Vector2(x + 0.5f, y + 0.5f);

                // �n�オ�C�̃��W�b�N �� �n���� bedrock
                if (!IsPointInPolygon(pos2, islandBoundary))
                {
                    tilemapUnderground.SetTile(pos3, bedrockTile);
                    int[] options = { 60 }; //���
                    tileMapDataUnderground[x, y] = options[Random.Range(0, options.Length)];
                    continue;
                }

                // �n���m�C�Y�ɂ��� or ��^�C���̔z�u
                float biomeNoise = Mathf.PerlinNoise((x + biomeOffset.x) * biomeFrequency,
                                                      (y + biomeOffset.y) * biomeFrequency) * 2f - 1f;

                if (biomeNoise < biomeThresholdCave)
                {
                    tilemapUnderground.SetTile(pos3, caveTile); // ��
                    int[] options = { 40 }; //��(�n�����A)
                    tileMapDataUnderground [x, y] = options[Random.Range(0, options.Length)];
                }
                else
                {
                    tilemapUnderground.SetTile(pos3, deepstoneTile); // ��
                    int[] options = { 50 }; //�[�w��
                    tileMapDataUnderground[x, y] = options[Random.Range(0, options.Length)];
                }
                    

            }
        }
    }

    TileBase ChooseBiomeTile(float val)
    {
        if (val < biomeThresholdDesert)
            return desertTile;
        else if (val < biomeThresholdGrassland)
            return grasslandTile[Random.Range(0,grasslandTile.Length)];
        else if (val < biomeThresholdForest)
            return forestTile;
        else
            return mountainTile;
    }

    List<Vector2> GenerateIslandPolygon()
    {
        List<Vector2> points = new List<Vector2>();
        float cellW = regionWidth / gridCols;
        float cellH = regionHeight / gridRows;

        for (int col = 1; col < gridCols - 1; col++)
            points.Add(RandomPointInCell(col, gridRows - 1, cellW, cellH));
        for (int row = gridRows - 2; row >= 1; row--)
            points.Add(RandomPointInCell(gridCols - 1, row, cellW, cellH));
        for (int col = gridCols - 2; col >= 1; col--)
            points.Add(RandomPointInCell(col, 0, cellW, cellH));
        for (int row = 1; row < gridRows - 1; row++)
            points.Add(RandomPointInCell(0, row, cellW, cellH));

        List<Vector2> poly = new List<Vector2>(points);
        for (int i = 0; i < iterations; i++)
        {
            float disp = initialDisplacement * Mathf.Pow(displacementReduction, i);
            poly = SubdividePolygon(poly, disp);
        }
        return poly;
    }

    Vector2 RandomPointInCell(int col, int row, float cellW, float cellH)
    {
        float marginX = cellW * 0.1f;
        float marginY = cellH * 0.1f;
        float x = col * cellW + Random.Range(marginX, cellW - marginX);
        float y = row * cellH + Random.Range(marginY, cellH - marginY);
        return new Vector2(x, y);
    }

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
