using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.TryGetComponent<DungeonRoom>(out DungeonRoom room)){
            DungeonManager.Instance.UpdateRooms(room.ID);
        }
    }
}
