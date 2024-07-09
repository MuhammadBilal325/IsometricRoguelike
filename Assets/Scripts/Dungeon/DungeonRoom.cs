using System.Collections;
using UnityEngine;

public enum RoomType
{
    PlayerRoom,
    Corridor,
    Normal
}

public class ExitPoint
{
    public Transform transform;
    public RoomType roomType;

    public ExitPoint(Transform t, RoomType r)
    {
        transform = t;
        roomType = r;
    }
}

public class DungeonRoom : MonoBehaviour {
   
    [SerializeField] private Transform[] exitPointTransforms;
    [SerializeField] private Transform entryPoint;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] private RoomType roomType;
    private ExitPoint[] exitPoints;

    private void Awake()
    {
        exitPoints = new ExitPoint[exitPointTransforms.Length];
        for(int i = 0; i < exitPointTransforms.Length; i++)
        {
            exitPoints[i] = new ExitPoint(exitPointTransforms[i],roomType);
        }
    }
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
