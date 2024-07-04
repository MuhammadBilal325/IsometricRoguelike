using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Build;
using UnityEngine;

public class ProceduralDungeon : MonoBehaviour {
    // Start is called before the first frame update

    public enum TileType {
        Empty,
        Origin,
        Floor,
        Corridor,
        Wall,
    }
    private class Edge {
        public Vector2Int Start;
        public Vector2Int End;
        public float Weight;

        public Edge(Vector2Int start, Vector2Int end, float weight = 0) {
            Start = start;
            End = end;
            Weight = weight;
        }
    }

    private class Border {
        public Vector3 Start;
        public Vector3 End;
        public Vector2Int arrayStartCoords;
        public Vector2Int arrayEndCoords;
        public bool isHorizontal;


        public Border(Vector3 start, Vector3 end, Vector2Int arrayStart, Vector2Int arrayEnd, bool isHorizontal) {
            Start = start;
            End = end;
            arrayStartCoords = new Vector2Int((int)arrayStart.x, (int)arrayStart.y);
            arrayEndCoords = new Vector2Int((int)arrayEnd.x, (int)arrayEnd.y);
            this.isHorizontal = isHorizontal;
        }
    }
    [SerializeField] private GameObject[] floors;
    [SerializeField] private GameObject[] walls1x1;
    [SerializeField] private GameObject[] walls1x2;
    [SerializeField] private GameObject[] wallsCorners;
    [SerializeField] private GameObject colliderPrefab;
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
    private List<Border> Borders = new List<Border>();
    void Start() {
        Random.InitState(seed);
        array = new TileType[arrayX, arrayY];
        AddRoomAtPoint(0, 0);
        Generate();
        MakeMap();
        GenerateFloorCollider();
        GenerateEdgesAndSetWallDirections();
        GenerateBorderColliders();
        MakeWalls();
        // PrintArray();
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
        PatchHoles();
        AddWalls();
        hasGenerated = true;
    }

    private void AddRoom(int originX, int originY, int roomWidthX, int roomWidthY) {
        for (int i = 0; i <= roomWidthX; i++) {
            for (int j = 0; j <= roomWidthY; j++) {
                if (originX + i < arrayX && originY + j < arrayY) {
                    array[originX + i, originY + j] = TileType.Floor;

                }
            }
        }
    }


    //This is such an ugly workaround , but it works for now
    private void PatchHoles() {
        for (int i = 1; i < arrayX - 1; i++) {
            for (int j = 1; j < arrayY - 1; j++) {
                if (array[i, j] == TileType.Empty && IsSolid(i - 1, j) && IsSolid(i + 1, j) && IsSolid(i, j - 1) && IsSolid(i, j + 1)) {
                    array[i, j] = TileType.Floor;
                }
            }
        }
    }
    private void AddWalls() {
        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                if (array[i, j] == TileType.Floor || array[i, j] == TileType.Origin || array[i, j] == TileType.Corridor) {
                    if (i > 0 && array[i - 1, j] == TileType.Empty) {
                        array[i - 1, j] = TileType.Wall;
                    }
                    if (i < arrayX - 1 && array[i + 1, j] == TileType.Empty) {
                        array[i + 1, j] = TileType.Wall;
                    }
                    if (j > 0 && array[i, j - 1] == TileType.Empty) {
                        array[i, j - 1] = TileType.Wall;
                    }
                    if (j < arrayY - 1 && array[i, j + 1] == TileType.Empty) {
                        array[i, j + 1] = TileType.Wall;
                    }
                    ////  Handle corner cases
                    if (i > 0 && j > 0 && array[i - 1, j - 1] == TileType.Empty) {
                        array[i - 1, j - 1] = TileType.Wall;
                    }
                    if (i < arrayX - 1 && j > 0 && array[i + 1, j - 1] == TileType.Empty) {
                        array[i + 1, j - 1] = TileType.Wall;
                    }
                    if (i > 0 && j < arrayY - 1 && array[i - 1, j + 1] == TileType.Empty) {
                        array[i - 1, j + 1] = TileType.Wall;
                    }
                    if (i < arrayX - 1 && j < arrayY - 1 && array[i + 1, j + 1] == TileType.Empty) {
                        array[i + 1, j + 1] = TileType.Wall;
                    }
                }
            }
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
                int direction = (int)Mathf.Sign(end.x - current.x);
                current.x += direction;
                CreateWideCorridor(current, direction, true);
            }
            else if (current.y != end.y) {
                int direction = (int)Mathf.Sign(end.y - current.y);
                current.y += direction;
                CreateWideCorridor(current, direction, false);
            }
        }
    }

    private void CreateWideCorridor(Vector2Int current, int direction, bool isHorizontal) {
        if (isHorizontal) {
            SetCorridorTile(current.x, current.y);
            SetCorridorTile(current.x, current.y - 1); // Ensure the corridor is 2 cells wide to the left
        }
        else {
            SetCorridorTile(current.x, current.y);
            SetCorridorTile(current.x - 1, current.y); // Ensure the corridor is 2 cells wide downwards
        }
    }

    private bool SetCorridorTile(int x, int y) {
        if (x >= 0 && x < arrayX && y >= 0 && y < arrayY && array[x, y] == TileType.Empty) {
            array[x, y] = TileType.Corridor;
            return true;
        }
        return false;
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
                    case TileType.Corridor:
                        sb.Append('I');
                        break;
                    case TileType.Wall:
                        sb.Append('■');
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
        //topLeft -= new Vector3(cellSize, 0, cellSize);
        //bottomLeft -= new Vector3(0, 0, cellSize);
        //topRight -= new Vector3(cellSize, 0, 0);
        vertices[0] = new Vector3(-cellSize, 0, -cellSize);
        vertices[1] = new Vector3(arrayX * cellSize, 0, -cellSize);
        vertices[2] = new Vector3(-cellSize, 0, arrayY * cellSize);
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

    private bool HorizontalWallSurroundCheck(int x, int y) {
        if (x == 0 || x == arrayX - 1) {
            return true;
        }
        //Check up and down
        if (x > 0
            && IsSolid(x - 1, y)) {
            return true;
        }
        if (x + 1 < arrayX
            && IsSolid(x + 1, y)) {
            return true;
        }


        return CornerSurroundCheck(x, y);
    }

    private bool VerticalWallSurroundCheck(int x, int y) {
        if (y == 0 || y == arrayY - 1) {
            return true;
        }
        if (y > 0
            && IsSolid(x, y - 1)) {
            return true;
        }
        if (y + 1 < arrayY
            && IsSolid(x, y + 1)
            ) {
            return true;
        }
        return CornerSurroundCheck(x, y);
    }

    private bool CornerSurroundCheck(int x, int y) {
        //Case 1
        // XW
        // WW
        if (x - 1 >= 0 && y - 1 >= 0 &&
            IsSolid(x - 1, y - 1) &&
            (array[x - 1, y] == TileType.Wall) &&
            (array[x, y - 1] == TileType.Wall)) {
            return true;
        }
        //Case 2
        // WX
        // WW
        if (x - 1 >= 0 && y + 1 < arrayY &&
            IsSolid(x - 1, y + 1) &&
            (array[x - 1, y] == TileType.Wall) &&
            (array[x, y + 1] == TileType.Wall)) {
            return true;
        }
        //Case 3
        // WW
        // XW
        if (x + 1 < arrayX && y - 1 >= 0 &&
            IsSolid(x + 1, y - 1) &&
            (array[x + 1, y] == TileType.Wall) &&
            (array[x, y - 1] == TileType.Wall)) {
            return true;
        }
        //Case 4
        // WW
        // WX
        if (x + 1 < arrayX && y + 1 < arrayY &&
            IsSolid(x + 1, y + 1) &&
            (array[x + 1, y] == TileType.Wall) &&
            (array[x, y + 1] == TileType.Wall)) {
            return true;
        }
        return false;
    }

    private bool IsSolid(int x, int y) {
        return (array[x, y] == TileType.Floor || array[x, y] == TileType.Origin || array[x, y] == TileType.Corridor);
    }
    private void GenerateEdgesAndSetWallDirections() {
        Vector3 edgeStart = Vector3.zero;
        Vector3 edgeEnd = Vector3.zero;
        Vector2Int edgeStartInts = Vector2Int.zero;
        Vector2Int edgeEndInts = Vector2Int.zero;
        //Add horizontal borders
        //Very simple implementation, we iterate through array until we find a wall that has a solid tile above or below,
        //when we find one we check if there is another wall to its right, if there is we start a loop going to the right until we find a non wall tile or a non surrounded wall tile
        //We then add a border from the start to the end of the wall

        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                if (array[i, j] == TileType.Wall) {
                    if (j < arrayY - 1 && array[i, j + 1] == TileType.Wall && HorizontalWallSurroundCheck(i, j)) {
                        edgeStart = new Vector3(i * cellSize, 0, j * cellSize);
                        edgeStartInts = new Vector2Int(i, j);
                        while (j < arrayY && array[i, j] == TileType.Wall && HorizontalWallSurroundCheck(i, j)) {
                            j++;
                        }
                        j--;
                        edgeEnd = new Vector3(i * cellSize, 0, j * cellSize);
                        edgeEndInts = new Vector2Int(i, j);
                        Borders.Add(new Border(edgeStart, edgeEnd, edgeStartInts, edgeEndInts, true));
                    }
                }
            }
        }
        //Add vertical borders
        for (int j = 0; j < arrayY; j++) {
            for (int i = 0; i < arrayX; i++) {
                if (array[i, j] == TileType.Wall) {
                    if (i < arrayX - 1 && array[i + 1, j] == TileType.Wall && VerticalWallSurroundCheck(i, j)) {
                        edgeStart = new Vector3(i * cellSize, 0, j * cellSize);
                        edgeStartInts = new Vector2Int(i, j);

                        while (i < arrayX && array[i, j] == TileType.Wall && VerticalWallSurroundCheck(i, j)) {
                            i++;
                        }
                        i--;
                        edgeEnd = new Vector3(i * cellSize, 0, j * cellSize);
                        edgeEndInts = new Vector2Int(i, j);
                        Borders.Add(new Border(edgeStart, edgeEnd, edgeStartInts, edgeEndInts, false));
                    }
                }
            }
        }

        //Edge case checking: Check if there are any walls surrounded by tiles

    }

    private void GenerateBorderColliders() {
        //Add four colliders for the outer walls
        Vector3 topLeft = new Vector3(0, 0, 0);
        Vector3 bottomLeft = new Vector3(arrayX * cellSize, 0, 0);
        Vector3 topRight = new Vector3(0, 0, arrayY * cellSize);
        Vector3 bottomRight = new Vector3(arrayX * cellSize, 0, arrayY * cellSize);
        topLeft -= new Vector3(cellSize, 0, cellSize);
        bottomLeft -= new Vector3(0, 0, cellSize);
        topRight -= new Vector3(cellSize, 0, 0);

        Borders.Add(new Border(topLeft, topRight, Vector2Int.zero, Vector2Int.zero, true));
        Borders.Add(new Border(topLeft, bottomLeft, Vector2Int.zero, Vector2Int.zero, false));
        Borders.Add(new Border(topRight, bottomRight, Vector2Int.zero, Vector2Int.zero, false));
        Borders.Add(new Border(bottomLeft, bottomRight, Vector2Int.zero, Vector2Int.zero, true));

        // Create an empty GameObject to act as the parent
        GameObject rectangleParent = new GameObject("Rectangle");
        rectangleParent.transform.SetParent(this.transform);

        foreach (Border border in Borders) {
            AddRectangle(border.Start, border.End, 10f, rectangleParent, border.isHorizontal);
        }
    }
    public void AddRectangle(Vector3 position1, Vector3 position2, float yScale, GameObject rectangleParent, bool isHorizontal) {

        // Create the rectangle (using a Quad or Cube as the base)
        GameObject rectangle = Instantiate(colliderPrefab, rectangleParent.transform);

        // Calculate the center position between position1 and position2
        Vector3 centerPosition = (position1 + position2) / 2;

        // Set the rectangle's position to the center
        rectangle.transform.position = centerPosition;

        // Calculate the distance between position1 and position2
        Vector3 distanceVector = position2 - position1;
        float distance = distanceVector.magnitude;
        distance += cellSize;
        // Calculate the scale
        Vector3 scale = new Vector3(distance, yScale, cellSize); // Assuming we are using a Quad with a depth of 1

        // Apply the scale to the rectangle
        rectangle.transform.localScale = scale;

        if (isHorizontal)
            rectangle.transform.Rotate(Vector3.up, 90);
    }

    private void MakeWalls() {
        //


        GameObject wallParent = new GameObject("Walls");
        List<MeshFilter> wallsMeshFilters = new List<MeshFilter>();
        wallParent.transform.SetParent(this.transform);
        bool[,] placedTiles = new bool[arrayX, arrayY];
        bool[,] directions = new bool[arrayX, arrayY]; // 0 means wall is horizontal, 1 means wall is vertical
        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                placedTiles[i, j] = false;
            }
        }
        //Set directions (corners can be any direction since they will always be made first)
        foreach (Border border in Borders) {
            if (border.isHorizontal) {
                for (int j = border.arrayStartCoords.y; j <= border.arrayEndCoords.y; j++) {
                    directions[border.arrayStartCoords.x, j] = false;
                }
            }
            else {

                for (int i = border.arrayStartCoords.x; i <= border.arrayEndCoords.x; i++) {
                    directions[i, border.arrayStartCoords.y] = true;
                }
            }
        }
        //Make walls
        //Add the corners first
        //foreach (Border border1 in Borders) { //Horizontal Borders
        //    foreach (Border border2 in Borders) { //Vertical Borders
        //        //Either their start or end is the same
        //        //Or ones start is equal to the others end and vice versa
        //        //Or they are the same
        //        //Check if they are the same
        //        if (!border1.isHorizontal) {
        //            continue;
        //        }
        //        if (border2.isHorizontal) {
        //            continue;
        //        }
        //        if (border1.arrayStartCoords == border1.arrayEndCoords && border1.arrayStartCoords == border2.arrayStartCoords && border1.arrayStartCoords == Vector2Int.zero) {
        //            continue;
        //        }
        //        Vector3 cornerPos = Vector3.zero;
        //        int cornerIndex = -1;
        //        Vector2Int arrayCoords = Vector2Int.zero;
        //        //Check if start is same
        //        if (border1.arrayEndCoords == border2.arrayEndCoords) {
        //            cornerPos = border1.End;
        //            arrayCoords = border1.arrayEndCoords;
        //            cornerIndex = 0;
        //        }
        //        else if (border1.arrayEndCoords == border2.arrayStartCoords) {
        //            cornerPos = border1.End;
        //            arrayCoords = border1.arrayEndCoords;
        //            cornerIndex = 1;
        //        }
        //        else if (border1.arrayStartCoords == border2.arrayStartCoords) {
        //            cornerPos = border1.Start;
        //            arrayCoords = border1.arrayStartCoords;
        //            cornerIndex = 2;
        //        }
        //        else if (border1.arrayStartCoords == border2.arrayEndCoords) {
        //            cornerPos = border1.Start;
        //            arrayCoords = border1.arrayStartCoords;
        //            cornerIndex = 3;
        //        }
        //        if (cornerIndex != -1) {
        //            GameObject obj = Instantiate(wallsCorners[cornerIndex], cornerPos, Quaternion.identity, wallParent.transform);
        //            obj.transform.SetParent(wallParent.transform);
        //            placedTiles[arrayCoords.x, arrayCoords.y] = true;
        //        }
        //    }
        //}

        //Add 2x1 walls by finding 2 tiles next to eachother in the same direction that are also not placed, place a 2x1 wall in between and mark them as placed
        //for (int i = 0; i < arrayX; i++) {
        //    for (int j = 0; j < arrayY; j++) {
        //        if (array[i, j] == TileType.Wall && !placedTiles[i, j]) { //WE are on a wall tile
        //            if (directions[i, j] == false) {//Wall is horizontal
        //                if (j < arrayY - 1 && array[i, j + 1] == TileType.Wall && !placedTiles[i, j + 1] && !directions[i, j + 1]) {
        //                    //If we have a 2nd tile in the same direction that is not placed and is also horizontal
        //                    Vector3 wallPos = new Vector3(i * cellSize, 0, j * cellSize + cellSize / 2);
        //                    GameObject obj = Instantiate(walls1x2[0], wallPos, Quaternion.identity, wallParent.transform);
        //                    obj.transform.SetParent(wallParent.transform);
        //                    placedTiles[i, j] = true;
        //                    placedTiles[i, j + 1] = true;
        //                }
        //            }
        //            else { //Wall is vertical
        //                if (i < arrayX - 1 && array[i + 1, j] == TileType.Wall && !placedTiles[i + 1, j] && directions[i + 1, j]) {
        //                    //If we have a 2nd tile in the same direction that is not placed and is also vertical
        //                    Vector3 wallPos = new Vector3(i * cellSize + cellSize / 2, 0, j * cellSize);
        //                    GameObject obj = Instantiate(walls1x2[0], wallPos, Quaternion.identity, wallParent.transform);
        //                    obj.transform.Rotate(Vector3.up, 90);
        //                    obj.transform.SetParent(wallParent.transform);
        //                    placedTiles[i, j] = true;
        //                    placedTiles[i + 1, j] = true;

        //                }
        //            }
        //        }
        //    }
        //}
        //Add 1x1 walls
        for (int i = 0; i < arrayX; i++) {
            for (int j = 0; j < arrayY; j++) {
                if (array[i, j] == TileType.Wall && !placedTiles[i, j]) {
                    Vector3 wallPos = new Vector3(i * cellSize, 0, j * cellSize);
                    GameObject obj = Instantiate(walls1x1[0], wallPos, Quaternion.identity, wallParent.transform);
                    if (directions[i, j])
                        obj.transform.Rotate(Vector3.up, 90);
                    obj.SetActive(false);
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null) {
                        wallsMeshFilters.Add(meshFilter);
                    }
                    Destroy(obj);
                    placedTiles[i, j] = true;

                }
            }
        }

        //Merge all walls into one gameObject
        CombineInstance[] combine = new CombineInstance[wallsMeshFilters.Count];
        for (int i = 0; i < wallsMeshFilters.Count; i++) {
            combine[i].mesh = wallsMeshFilters[i].sharedMesh;
            combine[i].transform = wallsMeshFilters[i].transform.localToWorldMatrix;
        }

        GameObject combinedWalls = new GameObject("CombinedWalls");
        combinedWalls.transform.SetParent(this.transform);
        MeshFilter combinedMeshFilter = combinedWalls.AddComponent<MeshFilter>();
        combinedMeshFilter.mesh = new Mesh();
        combinedMeshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMeshFilter.mesh.CombineMeshes(combine);

        MeshRenderer combinedMeshRenderer = combinedWalls.AddComponent<MeshRenderer>();
        combinedMeshRenderer.sharedMaterial = walls1x1[0].GetComponent<MeshRenderer>().sharedMaterial;

    }




    private void Update() {
        foreach (Border border in Borders) {
            Debug.DrawLine(border.Start, border.End, Color.red);
        }
    }


}
