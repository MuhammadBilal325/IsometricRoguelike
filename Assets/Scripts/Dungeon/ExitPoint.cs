using System.Collections;
using UnityEngine;

public class ExitPoint : MonoBehaviour {
    [SerializeField] private Transform open;
    [SerializeField] private Transform closed;
    [SerializeField] private RoomType roomType;
    private bool isOpen = false;
    public RoomType RoomType { get => roomType; private set { } }


    public void SetOpen() {
        open.gameObject.SetActive(true);
        closed.gameObject.SetActive(false);
        isOpen = true;
    }

    public void SetClosed() {
        open.gameObject.SetActive(false);
        closed.gameObject.SetActive(true);
    }

    public bool isRoomOpen() {
        return isOpen;
    }
}
