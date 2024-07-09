using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class DungeonRoomListSO : ScriptableObject {
    public Transform[] roomList;
    public Transform[] corridorList;
}
