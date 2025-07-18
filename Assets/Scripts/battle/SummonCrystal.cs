using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonCrystal : MonoBehaviour
{
    [Header("�֘AUI")]
    public GameObject alchemyUI;           // �J���Ώۂ̘B��UI�i��A�N�e�B�u��Ԃ���؂�ւ��j
    public KeyCode interactKey = KeyCode.E; // �C���^���N�g�p�L�[�i��FE�L�[�j

    private bool playerInRange = false;     // �v���C���[���͈͓��ɂ��邩�ǂ���

    private void Update()
    {
        // �͈͓��Ƀv���C���[�����āA���C���^���N�g�L�[�������ꂽ��UI���J��
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            OpenAlchemyUI();
        }
    }

    // UI���J������
    private void OpenAlchemyUI()
    {
        alchemyUI.SetActive(true);
        Debug.Log("Alchemy UI opened");
    }

    // �v���C���[���g���K�[�͈͂ɓ������Ƃ�
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // �v���C���[���͈͂���o���Ƃ�
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
