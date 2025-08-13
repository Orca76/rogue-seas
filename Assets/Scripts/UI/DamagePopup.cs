// DamagePopup.cs
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;        // �v���n�u�Ŋ��蓖�Đ����i���ݒ�ł�Awake�ŏE���j
    public float lifetime = 0.8f;   // ��������
    public float gravity = -8f;     // �����������x
    public Vector2 initialVel = new Vector2(0f, 2.2f); // �����i������j

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

        // ���傢����
        transform.localScale = Vector3.one * (crit ? 0.13f : 0.050f);

        // �ʒu�u���Ə���
        transform.position += (Vector3)(Random.insideUnitCircle * 0.08f);
        vel = new Vector2(Random.Range(-0.4f, 0.4f) + initialVel.x,
                          initialVel.y * (crit ? 1.2f : 1f));

        age = 0f;
    }

    void Update()
    {
        age += Time.deltaTime;

        // �[�������F�d�͂ŗ����{X�͌���
        vel.y += gravity * Time.deltaTime;
        vel.x = Mathf.Lerp(vel.x, 0f, 3f * Time.deltaTime);
        transform.position += (Vector3)(vel * Time.deltaTime);

        // �㔼�Ńt�F�[�h�A�E�g
        float fadeStart = lifetime - 0.3f;
        if (age > fadeStart)
        {
            float a = 1f - Mathf.InverseLerp(fadeStart, lifetime, age);
            var c = text.color; c.a = a; text.color = c;
        }

        if (age >= lifetime) Destroy(gameObject);
    }

    // �֗��X�|�[��
    public static DamagePopup Spawn(DamagePopup prefab, Vector3 worldPos, int amount, bool crit = false, Color? color = null)
    {
        var go = Instantiate(prefab, worldPos, Quaternion.identity);
        var dp = go.GetComponent<DamagePopup>();
        dp.Init(amount, crit, color);
        return dp;
    }
}
