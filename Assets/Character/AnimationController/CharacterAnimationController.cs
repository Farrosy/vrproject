using UnityEngine;

public class CharacterAnimationHelper : MonoBehaviour
{
    // Referensi ke komponen Animator yang ada di GameObject
    private Animator myAnimator;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    // Fungsi utama untuk mengubah animasi seperti Radio Button
    public void TriggerRadioAnimation(string targetTrigger)
    {
        // 1. Reset/Hilangkan antrean semua trigger yang lain terlebih dahulu
        myAnimator.ResetTrigger("IsStartWalking");
        myAnimator.ResetTrigger("IsWalking");
        myAnimator.ResetTrigger("IsStopWalking");

        // 2. Baru aktifkan trigger yang sedang dipilih
        myAnimator.SetTrigger(targetTrigger);
    }

    // Contoh cara memanggilnya saat tombol ditekan atau input masuk
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            // Tombol J ditekan -> Aktifkan Attack, batalkan Defend/Skill
            TriggerRadioAnimation("IsStartWalking");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Tombol K ditekan -> Aktifkan Defend, batalkan Attack/Skill
            TriggerRadioAnimation("IsStartWalking");
        }
    }
}