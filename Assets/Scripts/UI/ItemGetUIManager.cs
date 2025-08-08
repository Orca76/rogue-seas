using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGetUIManager : MonoBehaviour
{

    [Header("�z�u")]
    [SerializeField] RectTransform stackParent;   // �� Canvas���̒u����(�e)
    [SerializeField] RectTransform spawnAnchor;   // �� �V�K���o����ʒu(Empty)

    [Header("������")]
    [SerializeField] GameObject toastPrefab;      // �� ItemGetUI���t�����v���n�u
    [SerializeField] float slotHeight = 48f;      // �� 1�i���̍����i�����グ�����j
    [SerializeField] int maxActive = 3;           // �� �����\�����
    [SerializeField] float pushAnimTime = 0.1f;   // �� �����グ�A�j������

    // ��(0) �� �� �̏��ŕێ�
    readonly List<RectTransform> active = new();

    // �O������ĂԁF�A�C�e���擾����1�s��OK
    public void CreateUI(Sprite uiSprite, string uiText)
    {
        CleanupActive(); // �j���ς݂�����

        // 1) ���������ɉ�������
        for (int i = 0; i < active.Count; i++)
        {
            var rt = active[i];
            if (!rt) continue;
            StartCoroutine(ShiftY(rt, rt.anchoredPosition.y - slotHeight, pushAnimTime));
        }

        // 2) �V�K�𐶐����Ċ�ʒu�ɔz�u
        var go = Instantiate(toastPrefab, stackParent);
        var rtNew = go.GetComponent<RectTransform>();
        rtNew.anchoredPosition = spawnAnchor.anchoredPosition;

        // 3) �����ڃZ�b�g
        var toast = go.GetComponent<ItemGetUI>();
        toast.UIImage.sprite = uiSprite;
        toast.UIText.text = uiText;

        // 4) �����ɒǉ��i��i�����j
        active.Add(rtNew);

        // 5) ������������ԉ����폜
        if (active.Count > maxActive)
        {
            var bottom = active[0];
            if (bottom) Destroy(bottom.gameObject);
            active.RemoveAt(0);
        }
    }

    // �j�������null�ɂȂ��Ă�v�f��|��
    void CleanupActive()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i] == null) active.RemoveAt(i);
        }
    }

    // Y���W������Z���Ԃł��炷�ȈՃA�j��
    IEnumerator ShiftY(RectTransform rt, float targetY, float duration)
    {
        float t = 0f;
        var pos = rt.anchoredPosition;
        float startY = pos.y;
        while (t < duration && rt != null)
        {
            t += Time.unscaledDeltaTime; // UI�Ȃ̂Ŏ��Ԓ�~�̉e�����󂯂ɂ���
            float y = Mathf.Lerp(startY, targetY, t / duration);
            rt.anchoredPosition = new Vector2(pos.x, y);
            yield return null;
        }
        if (rt != null)
            rt.anchoredPosition = new Vector2(pos.x, targetY);
    }
}
