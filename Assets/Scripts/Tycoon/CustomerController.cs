using UnityEngine;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    public enum State { WaitingToOrder, WaitingForFood, Eating, Leaving }
    public State currentState;

    public float patience = 100f;
    public float maxPatience = 100f;

    // --- PERUBAHAN ---
    [Header("UI Settings")]
    public GameObject patienceBarPrefab; // Assign prefab Slider di sini
    private Slider patienceBarInstance; // Referensi ke slider yang dibuat

    public GameObject orderPrefab;
    public Table seatedTable;

    void Start()
    {
        // Cari Canvas utama di scene
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null && patienceBarPrefab != null)
        {
            // Buat instance dari prefab slider sebagai child dari canvas utama
            GameObject sliderObj = Instantiate(patienceBarPrefab, mainCanvas.transform);

            // Dapatkan komponen Slider dan UIFollowTarget dari instance tersebut
            patienceBarInstance = sliderObj.GetComponent<Slider>();
            UIFollowTarget followScript = sliderObj.GetComponent<UIFollowTarget>();

            // Beritahu script follow untuk mengikuti customer ini
            followScript.targetToFollow = this.transform;
        }
    }

    void Update()
    {
        if (currentState != State.Eating && currentState != State.Leaving)
        {
            patience -= Time.deltaTime * 5;
            if (patienceBarInstance != null)
            {
                patienceBarInstance.value = patience / maxPatience;
            }

            if (patience <= 0)
            {
                LeaveUnhappy();
            }
        }
    }

    public void OnSeated(Table table)
    {
        seatedTable = table;
        currentState = State.WaitingToOrder;
        transform.position = table.customerSeat.position;
    }

    public void OnOrderTaken()
    {
        currentState = State.WaitingForFood;
    }

    public void OnFoodDelivered()
    {
        currentState = State.Eating;
        patience = maxPatience;
        if (patienceBarInstance != null) patienceBarInstance.gameObject.SetActive(false); // Sembunyikan bar saat makan
        Invoke(nameof(FinishEating), 5f);
    }

    void FinishEating()
    {
        currentState = State.Leaving;
        seatedTable.OnCustomerLeave();
        DinerManager.Instance.AddScore(25);
        Destroy(gameObject, 1f);
    }

    void LeaveUnhappy()
    {
        currentState = State.Leaving;
        if (seatedTable != null)
        {
            seatedTable.OnCustomerLeave();
        }
        DinerManager.Instance.AddScore(-10);
        Destroy(gameObject);
    }

    // Pastikan slider juga hancur saat customer hancur
    void OnDestroy()
    {
        if (patienceBarInstance != null)
        {
            Destroy(patienceBarInstance.gameObject);
        }
    }
}