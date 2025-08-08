using TMPro;
using UnityEngine;
using UnityEngine.UI; // �� UGUI���g���̂ł�����

public class ItemGetUI : MonoBehaviour
{
    public Image UIImage;             // �A�C�e���A�C�R����Image
    public TextMeshProUGUI UIText;    // �A�C�e������Text
    public float MoveSpeed = 40f;     // �������ւ̈ړ����x(px/�b)
    public float FadeSpeed = 2f;      // �������̑���(��/�b)

    private CanvasGroup canvasGroup;  // ���l����p
    private RectTransform rect;       // UI�ʒu����p

    void Awake()
    {
        // ����UI�̃����܂Ƃ߂Đ��䂷�邽�߂�CanvasGroup���擾/�ǉ�
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // RectTransform�Q�Ƃ��L���b�V��
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // �@ ���Ɉړ��iUI�Ȃ̂�anchoredPosition�𑀍�j
        rect.anchoredPosition += Vector2.down * MoveSpeed * Time.deltaTime;

        // �A ���X�Ƀt�F�[�h�A�E�g
        canvasGroup.alpha -= FadeSpeed * Time.deltaTime;

        // �B ���S�ɓ����ɂȂ�����폜
        if (canvasGroup.alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
