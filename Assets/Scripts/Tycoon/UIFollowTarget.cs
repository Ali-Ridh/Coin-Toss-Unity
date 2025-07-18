using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    // Target di dunia game yang akan diikuti oleh UI ini
    public Transform targetToFollow;

    // Offset untuk mengatur posisi UI (misalnya, sedikit di atas kepala target)
    public Vector3 offset = new Vector3(0, 40, 0);

    private RectTransform rectTransform;
    private Camera mainCamera;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    // Gunakan LateUpdate agar posisi UI diupdate setelah target bergerak, untuk menghindari getaran (jitter).
    void LateUpdate()
    {
        if (targetToFollow == null)
        {
            // Jika target sudah dihancurkan (misalnya customer pergi), hancurkan juga UI ini.
            Destroy(gameObject);
            return;
        }

        // Konversi posisi target dari dunia game ke posisi di layar.
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(targetToFollow.position);

        // Atur posisi UI ini ke posisi layar yang sudah dikonversi + offset.
        rectTransform.position = screenPoint + (Vector2)offset;
    }
}