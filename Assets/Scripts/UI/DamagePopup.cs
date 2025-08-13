// DamagePopup.cs
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;        // プレハブで割り当て推奨（未設定でもAwakeで拾う）
    public float lifetime = 0.8f;   // 生存時間
    public float gravity = -8f;     // 下向き加速度
    public Vector2 initialVel = new Vector2(0f, 2.2f); // 初速（上向き）

    float age;
    Vector2 vel;
    Color baseColor;

    void Awake()
    {
        if (!text) text = GetComponent<TextMeshPro>();
    }

    public void Init(int amount, bool crit = false, Color? color = null)
    {
        if (!text) text = GetComponent<TextMeshPro>();

        text.text = amount.ToString();
        baseColor = color ?? (crit ? new Color(1f, 0.85f, 0.2f, 1f) : Color.red);
        text.color = baseColor;

        // ちょい強調
        transform.localScale = Vector3.one * (crit ? 0.13f : 0.050f);

        // 位置ブレと初速
        transform.position += (Vector3)(Random.insideUnitCircle * 0.08f);
        vel = new Vector2(Random.Range(-0.4f, 0.4f) + initialVel.x,
                          initialVel.y * (crit ? 1.2f : 1f));

        age = 0f;
    }

    void Update()
    {
        age += Time.deltaTime;

        // 擬似物理：重力で落下＋Xは減衰
        vel.y += gravity * Time.deltaTime;
        vel.x = Mathf.Lerp(vel.x, 0f, 3f * Time.deltaTime);
        transform.position += (Vector3)(vel * Time.deltaTime);

        // 後半でフェードアウト
        float fadeStart = lifetime - 0.3f;
        if (age > fadeStart)
        {
            float a = 1f - Mathf.InverseLerp(fadeStart, lifetime, age);
            var c = text.color; c.a = a; text.color = c;
        }

        if (age >= lifetime) Destroy(gameObject);
    }

    // 便利スポーン
    public static DamagePopup Spawn(DamagePopup prefab, Vector3 worldPos, int amount, bool crit = false, Color? color = null)
    {
        var go = Instantiate(prefab, worldPos, Quaternion.identity);
        var dp = go.GetComponent<DamagePopup>();
        dp.Init(amount, crit, color);
        return dp;
    }
}
