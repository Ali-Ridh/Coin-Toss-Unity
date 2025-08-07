using UnityEngine;

public class Station : MonoBehaviour
{
    public enum StationType { Queue, Kitchen, Dishes }
    public StationType type;

    public void OnMouseDown()
    {
        DinerManager.Instance.OnStationClicked(this);
    }
}
