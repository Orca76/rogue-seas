using TMPro;
using UnityEngine;
using UnityEngine.UI; // ← UGUIを使うのでこちら

public class ItemGetUI : MonoBehaviour
{
    public Image UIImage;             // アイテムアイコンのImage
    public TextMeshProUGUI UIText;    // アイテム名のText
    public float MoveSpeed = 40f;     // 下方向への移動速度(px/秒)
    public float FadeSpeed = 2f;      // 透明化の速さ(α/秒)

    private CanvasGroup canvasGroup;  // α値制御用
    private RectTransform rect;       // UI位置制御用

    void Awake()
    {
        // このUIのαをまとめて制御するためにCanvasGroupを取得/追加
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // RectTransform参照をキャッシュ
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // ① 下に移動（UIなのでanchoredPositionを操作）
        rect.anchoredPosition += Vector2.down * MoveSpeed * Time.deltaTime;

        // ② 徐々にフェードアウト
        canvasGroup.alpha -= FadeSpeed * Time.deltaTime;

        // ③ 完全に透明になったら削除
        if (canvasGroup.alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
