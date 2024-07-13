using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum RoomType
{
    PlayerRoom,
    Corridor,
    Normal
}



public class DungeonRoom : MonoBehaviour {
   
    [SerializeField] private Transform entryPoint;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private RoomType roomType;
    [SerializeField] private ExitPoint[] exitPoints;
    private int id;
   
    public ExitPoint[] ExitPoints { get => exitPoints; private set { } }
    public Transform EntryPoint { get => entryPoint; private set { } }

    public int ID { get => id; private set { } }
    public void SetID(int num) {
        id = num;
    }

    public bool IsColliding(DungeonRoom other) {
        return boxCollider.bounds.Intersects(other.GetBounds());
    }

    
    public Bounds GetBounds() {
        return boxCollider.bounds;
    }


}
