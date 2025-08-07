using UnityEngine;
using UnityEngine.Tilemaps;

public class ChartTile : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemapChart;

    [Header("Sea Area Tile Types")]
    public TileBase tropicalTile;    // â∑íg
    public TileBase frigidTile;      // ä¶ó‚
    public TileBase foggyTile;       // îZñ∂
    public TileBase tempestTile;     // ñ\ïó
    public TileBase organicTile;     // óLã@
    public TileBase mysticTile;      // ê_îÈ

    [Header("Map Size")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Noise Settings")]
    public float scale = 0.05f;
    public int noiseSeed = 5678;

    [Header("Control Keys")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode hideKey = KeyCode.T;

    public int[,] seaRegionData; // 0 = tropical, 1 = frigid, 2 = foggy, 3 = tempest, 4 = organic, 5 = mystic


    private void Start()
    {
        seaRegionData = new int[mapWidth, mapHeight];

        //ç≈èâÇ…çÏÇÈ

        noiseSeed = gameObject.GetComponent<IslandTile>().noiseSeed;//ìáàÀë∂ÇÃìπ
        tilemapChart.gameObject.SetActive(true);
        GenerateChartMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            noiseSeed = Random.Range(0, 10000);
            tilemapChart.gameObject.SetActive(true);
            GenerateChartMap();
        }

        if (Input.GetKeyDown(hideKey))
        {
            tilemapChart.gameObject.SetActive(false);
        }
    }

    public void GenerateChartMap()
    {
        tilemapChart.ClearAllTiles();
        Vector2 offset = GetNoiseOffset(noiseSeed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float nx = (x + offset.x) * scale;
                float ny = (y + offset.y) * scale;
                float noiseVal = Mathf.PerlinNoise(nx, ny);

                int regionCode;
                TileBase selectedTile = GetTileByNoise(noiseVal, out regionCode);
                tilemapChart.SetTile(new Vector3Int(x, y, 0), selectedTile);
                seaRegionData[x, y] = regionCode;
            }
        }
    }
    public int GetZoneTypeAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
            return -1;
        return seaRegionData[x, y]; // Å© tileZoneData Ç≈ÇÕÇ»Ç≠ seaRegionData Ç…èCê≥
    }
    private TileBase GetTileByNoise(float val, out int regionCode)
    {
        if (val < 0.3f)
        {
            regionCode = 0;
            return tropicalTile;
        }
        else if (val < 0.5f)
        {
            regionCode = 1;
            return frigidTile;
        }
        else if (val < 0.65f)
        {
            regionCode = 2;
            return foggyTile;
        }
        else if (val < 0.8f)
        {
            regionCode = 3;
            return tempestTile;
        }
        else if (val < 0.95f)
        {
            regionCode = 4;
            return organicTile;
        }
        else
        {
            regionCode = 5;
            return mysticTile;
        }
    }

    private Vector2 GetNoiseOffset(int seed)
    {
        float offsetX = (seed * 12345 % 1000) / 1000f * 100f;
        float offsetY = (seed * 67890 % 1000) / 1000f * 100f;
        return new Vector2(offsetX, offsetY);
    }

    public int GetRegionCodeAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) return -1;
        return seaRegionData[x, y];
    }
}
