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




    int[] points=new int[4];//ステータス割り振り　0 hp 1power 2atspeed 3prefab

    public GameObject defaultSentry;//生成時に使うデフォ


    private void Start()
    {
        VectorSystem = GameObject.Find("VectorManager");
        alchemySystem=VectorSystem.GetComponent<AlchemyVectorManager>();
    }
    private void Update()
    {
        // 範囲内にプレイヤーがいて、かつインタラクトキーが押されたらUIを開く
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
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
        Debug.Log("Alchemy UI opened");
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
        switch(alchemySystem.rarityCode)
        {
            case 0://コモン sp10

                //セントリーのステータス決定
                DistributePoints(10);

                break;

            case 1://レア sp20

                break;
            case 2://スーパーレア sp30

                break;

            default:

                break;
        }
        GameObject created=Instantiate(defaultSentry,gameObject.transform.position, Quaternion.identity);
        float minScale = 0.5f;
        float maxScale = 2.0f;

        float t = points[0] / 10f;  // 0〜1 に正規化

        float scale = Mathf.Lerp(minScale, maxScale, t);

        // スプライトのTransformのスケールを変更
        created.transform.localScale = new Vector3(scale, scale, 1f);
        //その他のステータス登録
        created.GetComponent<SentryBase>().BaseHP = points[0] * Random.Range(90, 110);
        created.GetComponent<SentryBase>().BasePower = points[1] * Random.Range(9, 11);
        created.GetComponent<SentryBase>().BaseAttackSpeed = points[2] * Random.Range(0.9f,1.1f);

        int val = points[3];

        if (val >= 0 && val <= 3)//ここで弾丸をセット
        {
            // 0〜3の処理
            Debug.Log(
    $"created: {(created != null ? "OK" : "missing")}, " +
    $"SentryBase: {(created?.GetComponent<SentryBase>() != null ? "OK" : "missing")}, " +
    $"SentryDatabase.instance: {(SentryDatabase.instance != null ? "OK" : "missing")}, " +
    $"bulletTier3: {(SentryDatabase.instance?.bulletTier3 != null ? "OK" : "missing")}, " +
    $"bulletTier3.Length: {(SentryDatabase.instance?.bulletTier3 != null ? SentryDatabase.instance.bulletTier3.Length.ToString() : "N/A")}"
);
            created.GetComponent<SentryBase>().BulletPrefab = SentryDatabase.instance.bulletTier3[Random.Range(0, SentryDatabase.instance.bulletTier3.Length)];
        }
        else if (val >= 4 && val <= 7)
        {
            // 4〜7の処理
            created.GetComponent<SentryBase>().BulletPrefab = SentryDatabase.instance.bulletTier2[Random.Range(0, SentryDatabase.instance.bulletTier2.Length)];
        }
        else if (val >= 8 && val <= 10)
        {
            // 8〜10の処理
            created.GetComponent<SentryBase>().BulletPrefab = SentryDatabase.instance.bulletTier1[Random.Range(0, SentryDatabase.instance.bulletTier1.Length)];
        }
        else
        {
            // 範囲外の処理（必要なら）
        }

    }
    public void DistributePoints(int skillPoints)
    {
        if (points == null || points.Length != 4)
        {
            Debug.LogError("points配列は4要素で初期化してください");
            return;
        }

        // 一旦全部0クリア
        for (int i = 0; i < 4; i++) points[i] = 0;

        int remaining = skillPoints;

        while (remaining > 0)
        {
            // 0~3のどれか選ぶ
            int idx = Random.Range(0, 4);

            if (points[idx] < 10)
            {
                points[idx]++;
                remaining--;
            }
            // 10超えはスキップして別インデックス選び直し
        }
    }


}
