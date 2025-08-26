using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SummonCrystal : MonoBehaviour
{
    [Header("関連UI")]
    public GameObject alchemyUI;           // 開く対象の錬成UI（非アクティブ状態から切り替え）
    public KeyCode interactKey = KeyCode.E; // インタラクト用キー（例：Eキー）

    private bool playerInRange = false;     // プレイヤーが範囲内にいるかどうか

    GameObject InventoryUIObj;
    GameObject CreateUIObj;
    public AlchemyVectorManager alchemySystem;//ベクトル出してる奴　リセットの為に呼ぶ
    
    GameObject VectorSystem;//上の奴の実体
    public GameObject FieldText;//何が生成されるかのテキスト



    int[] points=new int[3];//ステータス割り振り　0 hp 1power 2atspeed 3prefab

    public GameObject defaultSentry;//生成時に使うデフォ

    GameObject Player;
    private bool hasBeenUsed = false;
    SentryManager sentryManagerSC;//セントリー生成時に配列に登録用

    public Sprite usedSprite;//使用後のスプライト
    GameObject map;//錬成盤リロード用　マップデータ
    AlchemyTile tiledata;


    public Sprite[] normalList;
    public Sprite[] RareList;
    public Sprite[] SuperRareList;
    private void Start()
    {
        VectorSystem = GameObject.Find("VectorManager");
        alchemySystem=VectorSystem.GetComponent<AlchemyVectorManager>();
        Player = GameObject.FindWithTag("Player");
        sentryManagerSC=Player.GetComponent<SentryManager>();
        map = GameObject.Find("map");
        tiledata=map.GetComponent<AlchemyTile>();
    }
    private void Update()
    {
        // 範囲内にプレイヤーがいて、かつインタラクトキーが押されたらUIを開く
        if (playerInRange && Input.GetKeyDown(interactKey) && hasBeenUsed == false)
        {
            // 時間を完全に停止
            Time.timeScale = 0f;
            OpenAlchemyUI();
        }
       
    }

    // UIを開く処理
    private void OpenAlchemyUI()
    {
        alchemyUI.SetActive(true);
        alchemySystem.RegionText = FieldText.GetComponent<TextMeshProUGUI>(); ;//テキストを今出てるクリスタルのものに指定
        if(InventoryUIObj == null)
        {
            InventoryUIObj = GameObject.Find("HotbarPanel");//起動時にインベントリ取得
        }

        //インベントリの位置を上に
        SetPosY(30);
        tiledata.CreationTile(GetSeedFromCoords(gameObject.transform.position.x, gameObject.transform.position.y));
        Debug.Log("Alchemy UI opened");
    }
    int GetSeedFromCoords(float x, float y)//座標から乱数生成
    {
        int xi = Mathf.FloorToInt(x * 1000f);  // 必要ならスケーリング
        int yi = Mathf.FloorToInt(y * 1000f);
        return xi * 1000000 + yi; // xの桁を広げて足すだけ
    }
    // プレイヤーがトリガー範囲に入ったとき
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // プレイヤーが範囲から出たとき
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    public void CloseAlchemyUI()//戻るボタン　UIを閉じる
    {
        //インベントリの位置を下に
        SetPosY(-100);
        alchemySystem.ResetVectors();//閉じたらベクトルリセット


        // 再開
        Time.timeScale = 1f;
        alchemyUI.SetActive(false);
        Debug.Log("Alchemy UI closed");
    }

    void SetPosY(float y)//インベントリの位置変更
    {
        var rect =InventoryUIObj.GetComponent<RectTransform>();
        var pos = rect.anchoredPosition;
        pos.y = y;
        rect.anchoredPosition = pos;
    }
    public void CreateSentry()//生成ボタンを押した時
    {
      
        GameObject created=Instantiate(defaultSentry,gameObject.transform.position, Quaternion.identity);

        switch (alchemySystem.rarityCode)
        {
            case 0://コモン sp10

                //セントリーのステータス決定
                DistributePoints(7);
                created.GetComponent<SentryBase>().Rarity = 0;
                created.GetComponent<SpriteRenderer>().sprite = normalList[Random.Range(0,normalList.Length)];

                break;

            case 1://レア sp20
                DistributePoints(14);
                created.GetComponent<SentryBase>().Rarity = 1;
                created.GetComponent<SpriteRenderer>().sprite = RareList[Random.Range(0, RareList.Length)];
                break;
            case 2://スーパーレア sp30

                DistributePoints(21);
                created.GetComponent<SentryBase>().Rarity = 2;
                created.GetComponent<SpriteRenderer>().sprite = SuperRareList[Random.Range(0, SuperRareList.Length)];
                break;

            default:

                break;
        }
        sentryManagerSC.sentryList.Add(created);//リストに登録

        created.transform.parent=Player.transform;//プレイヤーの子オブジェクトにする
        float minScale = 0.5f;
        float maxScale = 2.0f;

        float t = points[0] / 10f;  // 0〜1 に正規化

        float scale = Mathf.Lerp(minScale, maxScale, t);

        // スプライトのTransformのスケールを変更
        created.transform.localScale = new Vector3(scale, scale, 1f);
        //その他のステータス登録
        created.GetComponent<SentryBase>().BaseHP = points[0] * Random.Range(40, 60);
        created.GetComponent<SentryBase>().BasePower = points[1] * Random.Range(4, 6);
        created.GetComponent<SentryBase>().BaseAttackSpeed = points[2] * Random.Range(0.12f,0.16f);
        created.GetComponent<SentryBase>().HP = created.GetComponent<SentryBase>().BaseHP;

        //int val = points[3];
        created.GetComponent<SentryBase>().BulletPrefab = SentryDatabase.instance.bulletTier1[Random.Range(0, SentryDatabase.instance.bulletTier1.Length)];
        //if (val >= 0 && val <= 3)//ここで弾丸をセット
        //{
        //    // 0〜3の処理
          
           
        //}
        //else if (val >= 4 && val <= 7)
        //{
        //    // 4〜7の処理
        //    created.GetComponent<SentryBase>().BulletPrefab = SentryDatabase.instance.bulletTier2[Random.Range(0, SentryDatabase.instance.bulletTier2.Length)];
        //}
        //else if (val >= 8 && val <= 10)
        //{
        //    // 8〜10の処理
        //    created.GetComponent<SentryBase>().BulletPrefab = SentryDatabase.instance.bulletTier1[Random.Range(0, SentryDatabase.instance.bulletTier1.Length)];
        //}
        //else
        //{
        //    // 範囲外の処理（必要なら）
        //}
        hasBeenUsed = true;//もう使えない
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && usedSprite != null)
        {
            spriteRenderer.sprite = usedSprite;
        }//画像差し替え処理　クリスタル
        CloseAlchemyUI();

    }
    public void DistributePoints(int skillPoints)
    {
        if (points == null || points.Length != 3)
        {
            Debug.LogError("points配列は3要素で初期化してください");
            return;
        }

        // 一旦全部0クリア
        for (int i = 0; i < 3; i++) points[i] = 0;

        // まず最低保証: 各パラに1振る
        for (int i = 0; i < 3; i++)
        {
            points[i] = 1;
        }

        // 残りポイント計算
        int remaining = skillPoints - 3; // 3消費済み

        while (remaining > 0)
        {
            // 0~3のどれか選ぶ
            int idx = Random.Range(0, 3);

            if (points[idx] < 10)
            {
                points[idx]++;
                remaining--;
            }
            // 10超えはスキップして別インデックス選び直し
        }
    }


}
