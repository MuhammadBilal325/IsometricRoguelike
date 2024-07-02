using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ProceduralDungeon : MonoBehaviour {
    // Start is called before the first frame update

    public enum TileType {
        Empty,
        Origin,
        Floor,
        Padding,
        Corridor,
    }

    [SerializeField] private GameObject[] floors;
    [SerializeField] private int arrayX;
    [SerializeField] private int arrayY;
    [SerializeField, Range(0, 1)] private float roomChance;
    [SerializeField] private int maxRoomXSize;
    [SerializeField] private int maxRoomYSize;
    [SerializeField] private float cellSize;
    [SerializeField] private TileType[,] array;
    [SerializeField] private int seed;
    private bool hasGenerated = false;
    [SerializeField] private TextMeshProUGUI DebugText;

    private List<Vector2Int> roomOrigins = new List<Vector2Int>();
    void Start() {
        Random.InitState(seed);
        array = new TileType[arrayX, arrayY];
        AddRoomAtPoint(0, 0);
        Generate();
        MakeMap();
        GenerateFloorCollider();
        GenerateEdgeColliders();
        PrintArray();
    }


    private void AddRoomAtPoint(int pointX, int pointY) {
        if (!hasGenerated) {
            AddRoom(pointX, pointY, Random.Range(1, maxRoomXSize), Random.Range(1, maxRoomYSize));
            array[pointX, pointY] = TileType.Origin;
            roomOrigins.Add(new Vector2Int(pointX, pointY));
        }
    }
    private void Generate() {
        //Set origin and make room
        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                if (Random.value < roomChance && array[i, j] == TileType.Empty) {
                    int roomXsize = Random.Range(1, maxRoomXSize);
                    int roomYsize = Random.Range(1, maxRoomYSize);
                    AddRoom(i, j, roomXsize, roomYsize);
                    array[i, j] = TileType.Origin;
                    roomOrigins.Add(new Vector2Int(i, j));
                }
            }
        }
        ConnectRooms();
        hasGenerated = true;
    }

    private void AddRoom(int originX, int originY, int roomWidthX, int roomWidthY) {
        for (int i = 0; i <= roomWidthX; i++) {
            for (int j = 0; j <= roomWidthY; j++) {
                if (originX + i < arrayX && originY + j < arrayY) {
                    array[originX + i, originY + j] = TileType.Floor;
                    //Add padding on edges
                    //If we are on bottom row
                    if (i == roomWidthX && originX + i + 1 < arrayX) {
                        array[originX + i + 1, originY + j] = TileType.Padding;
                    }
                    ////If we are on top row
                    if (i == 0 && originX + i - 1 >= 0) {
                        array[originX + i - 1, originY + j] = TileType.Padding;
                    }
                    ////If we are on right side
                    if (j == roomWidthY && originY + j + 1 < arrayY) {
                        array[originX + i, originY + j + 1] = TileType.Padding;
                    }
                    ////if we are on left side
                    if (j == 0 && originY + j - 1 >= 0) {
                        array[originX + i, originY + j - 1] = TileType.Padding;
                    }
                    ////If we are on a corner
                    //if (i == roomWidthX && j == roomWidthY && originX + i + 1 < arrayX && originY + j + 1 < arrayY) {
                    //    array[originX + i + 1, originY + j + 1] = TileType.Padding;
                    //}
                    //if (i == 0 && j == 0 && originX + i - 1 >= 0 && originY + j - 1 >= 0) {
                    //    array[originX + i - 1, originY + j - 1] = TileType.Padding;
                    //}
                    //if (i == 0 && j == roomWidthY && originX + i - 1 >= 0 && originY + j + 1 < arrayY) {
                    //    array[originX + i - 1, originY + j + 1] = TileType.Padding;
                    //}
                    //if (i == roomWidthX && j == 0 && originX + i + 1 < arrayX && originY + j - 1 >= 0) {
                    //    array[originX + i + 1, originY + j - 1] = TileType.Padding;
                    //}
                }
            }
        }
    }
    private class Edge {
        public Vector2Int Start;
        public Vector2Int End;
        public float Weight;

        public Edge(Vector2Int start, Vector2Int end, float weight) {
            Start = start;
            End = end;
            Weight = weight;
        }
    }
    private void ConnectRooms() {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Edge> edges = new List<Edge>();
        foreach (var origin in roomOrigins) {
            foreach (var neighbor in roomOrigins) {
                if (origin != neighbor) {
                    edges.Add(new Edge(origin, neighbor, Vector2Int.Distance(origin, neighbor)));
                }
            }
        }
        edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

        visited.Add(roomOrigins[0]);

        while (visited.Count < roomOrigins.Count) {
            foreach (var edge in edges) {
                if (visited.Contains(edge.Start) && !visited.Contains(edge.End)) {
                    CreateCorridor(edge.Start, edge.End);
                    visited.Add(edge.End);
                    edges.Remove(edge);
                    break;
                }
                if (visited.Contains(edge.End) && !visited.Contains(edge.Start)) {
                    CreateCorridor(edge.End, edge.Start);
                    visited.Add(edge.Start);
                    edges.Remove(edge);
                    break;
                }
            }
        }
    }
    private void CreateCorridor(Vector2Int start, Vector2Int end) {
        Vector2Int current = start;
        while (current != end) {
            if (current.x != end.x) {
                current.x += (int)Mathf.Sign(end.x - current.x);
            }
            else if (current.y != end.y) {
                current.y += (int)Mathf.Sign(end.y - current.y);
            }
            if (current.x >= 0 && current.x < arrayX && current.y >= 0 && current.y < arrayY
                && (array[current.x, current.y] == TileType.Empty
                || array[current.x, current.y] == TileType.Padding
                )) {
                array[current.x, current.y] = TileType.Corridor;
            }

        }
    }

    private void MakeMap() {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                if (array[i, j] == TileType.Floor || array[i, j] == TileType.Origin
                    || array[i, j] == TileType.Corridor) {
                    GameObject floor = Instantiate(floors[Random.Range(0, floors.Length)], new Vector3(i * cellSize, 0, j * cellSize), Quaternion.identity);
                    floor.SetActive(false);

                    MeshFilter meshFilter = floor.GetComponent<MeshFilter>();
                    if (meshFilter != null) {
                        meshFilters.Add(meshFilter);
                    }
                    Destroy(floor);
                }
            }
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        GameObject combinedFloor = new GameObject("CombinedFloor");
        combinedFloor.transform.SetParent(this.transform);

        // Add a mesh filter and mesh renderer to the combined floor GameObject
        MeshFilter combinedMeshFilter = combinedFloor.AddComponent<MeshFilter>();
        combinedMeshFilter.mesh = new Mesh();
        combinedMeshFilter.mesh.CombineMeshes(combine);

        MeshRenderer combinedMeshRenderer = combinedFloor.AddComponent<MeshRenderer>();
        combinedMeshRenderer.sharedMaterial = floors[0].GetComponent<MeshRenderer>().sharedMaterial;

    }

    private void PrintArray() {
        StringBuilder sb = new StringBuilder();
        sb.Append("<mspace=10px>");
        sb.AppendLine();
        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                //sb.Append((int)array[i, j]);
                switch (array[i, j]) {
                    case TileType.Empty:
                        sb.Append('-');
                        break;
                    case TileType.Origin:
                        sb.Append('○');
                        break;
                    case TileType.Floor:
                        sb.Append('□');
                        break;
                    case TileType.Padding:
                        sb.Append('■');
                        break;
                    case TileType.Corridor:
                        sb.Append('I');
                        break;
                }
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        sb = sb.Replace("\0", string.Empty);
        DebugText.text = sb.ToString();

    }

    private void GenerateFloorCollider() {
        GameObject floorCollider = new GameObject("FloorCollider");
        floorCollider.transform.SetParent(this.transform);

        // Add a MeshCollider component to the floor collider GameObject
        MeshCollider meshCollider = floorCollider.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(arrayX * cellSize, 0, 0);
        vertices[2] = new Vector3(0, 0, arrayY * cellSize);
        vertices[3] = new Vector3(arrayX * cellSize, 0, arrayY * cellSize);

        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
    }

    private void GenerateEdgeColliders() {
    }

    private void AddCollider(GameObject parent, Vector3 position, Vector3 size) {
    }

}
