using UnityEngine;
using UnityEngine.Tilemaps;

public class AlchemyTile : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemapAlchemy;

    [Header("Tile Types")]
    public TileBase commonTile;     // 青
    public TileBase rareTile;       // 紫
    public TileBase superRareTile;  // 黄

    [Header("Map Size")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Noise Settings")]
    public float scale = 0.05f;
    public int noiseSeed = 1234;

    [Header("Control Keys")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode hideKey = KeyCode.T;

    public int[,] tileRarityData; // 0 = common, 1 = rare, 2 = super rare


    [Header("Rarity Threshold Multipliers")]
    public float commonThresholdMultiplier = 1.0f;
    public float rareThresholdMultiplier = 1.0f;

    public int crystalRarity;//0がコモン1がレア2がSR

    private void Start()
    {
        tileRarityData = new int[mapWidth, mapHeight];

        //起動時にオン
        noiseSeed = Random.Range(0, 10000);
        tilemapAlchemy.gameObject.SetActive(true);
        GenerateAlchemyMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            noiseSeed = Random.Range(0, 10000);
            tilemapAlchemy.gameObject.SetActive(true);
            GenerateAlchemyMap();
        }

        if (Input.GetKeyDown(hideKey))
        {
            tilemapAlchemy.gameObject.SetActive(false);
        }
    }

    public void GenerateAlchemyMap()
    {
        SetRarityMode(crystalRarity);
        tilemapAlchemy.ClearAllTiles();
        Vector2 offset = GetNoiseOffset(noiseSeed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float nx = (x + offset.x) * scale;
                float ny = (y + offset.y) * scale;
                float noiseVal = Mathf.PerlinNoise(nx, ny);

                float maxDist = Mathf.Sqrt(Mathf.Pow(mapWidth / 2f, 2) + Mathf.Pow(mapHeight / 2f, 2));
                float dx = x - mapWidth / 2f;
                float dy = y - mapHeight / 2f;
                float distFactor = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;

                int tileRarity;
                TileBase selectedTile = GetTileByNoise(noiseVal, distFactor, out tileRarity);
                tilemapAlchemy.SetTile(new Vector3Int(x, y, 0), selectedTile);
                tileRarityData[x, y] = tileRarity;
            }
        }
    }

    public void SetRarityMode(int rarityLevel)
    {
        switch (rarityLevel)
        {
            case 0: // 通常
                commonThresholdMultiplier = 1.0f;
                rareThresholdMultiplier = 1.0f;
                break;
            case 1: // レア
                commonThresholdMultiplier = 0.9f;
                rareThresholdMultiplier = 0.9f;
                break;
            case 2: // 超レア
                commonThresholdMultiplier = 0.75f;
                rareThresholdMultiplier = 0.75f;
                break;
        }
    }
    private TileBase GetTileByNoise(float val, float distFactor, out int rarityCode)
    {
        float baseCommon = Mathf.Lerp(0.6f, 0.45f, distFactor);
        float baseRare = Mathf.Lerp(0.975f, 0.65f, distFactor);

        float commonThreshold = baseCommon * commonThresholdMultiplier;
        float rareThreshold = baseRare * rareThresholdMultiplier;

        if (val < commonThreshold)
        {
            rarityCode = 0;
            return commonTile;
        }
        else if (val < rareThreshold)
        {
            rarityCode = 1;
            return rareTile;
        }
        else
        {
            rarityCode = 2;
            return superRareTile;
        }
    }

    private Vector2 GetNoiseOffset(int seed)
    {
        float offsetX = (seed * 12345 % 1000) / 1000f * 100f;
        float offsetY = (seed * 67890 % 1000) / 1000f * 100f;
        return new Vector2(offsetX, offsetY);
    }

    public int GetRarityAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) return -1;
        return tileRarityData[x, y];
    }
}
