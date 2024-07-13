using System.Collections;
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
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private RoomType roomType;
    [SerializeField] private ExitPoint[] exitPoints;
   
    public ExitPoint[] ExitPoints { get => exitPoints; private set { } }
    public Transform EntryPoint { get => entryPoint; private set { } }



    public bool IsColliding(DungeonRoom other) {
        return boxCollider.bounds.Intersects(other.GetBounds());
    }


    public void SetMaterialColor(UnityEngine.Color col) {
        renderer.material.color = col;
    }

    public Bounds GetBounds() {
        return boxCollider.bounds;
    }

}
