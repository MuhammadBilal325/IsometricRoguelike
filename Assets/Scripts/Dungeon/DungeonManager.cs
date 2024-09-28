using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class DungeonManager : MonoBehaviour {


    private class RoomNode {
        public Transform Transform { get; private set; }

        public DungeonRoom roomScript { get; private set; }
        public List<RoomNode> Neighbors { get; private set; }

        public RoomNode(Transform roomTransform) {
            this.Transform = roomTransform;
            roomScript = roomTransform.GetComponent<DungeonRoom>();
            Neighbors = new List<RoomNode>();
        }

        public void AddNeighbor(RoomNode neighbour) {
            if (!Neighbors.Contains(neighbour)) {
                Neighbors.Add(neighbour);
                neighbour.Neighbors.Add(this);
            }
        }

        public void RemoveNeighbor(RoomNode neighbor) {
            if (Neighbors.Contains(neighbor)) {
                Neighbors.Remove(neighbor);
                neighbor.Neighbors.Remove(this);
            }
        }

    }


    private class Exit {
        public ExitPoint point;
        public int roomID;

        public Exit(ExitPoint point, int roomID) {
            this.point = point;
            this.roomID = roomID;
        }
    }


    public static DungeonManager Instance { get; private set; }

    [SerializeField] private DungeonRoomListSO dungeonRoomList;
    [SerializeField] private EnemyListSO enemyListSo;
    [SerializeField] private int seed;
    [SerializeField] private int maxRoomCount = 10;
    [SerializeField] private int maxDFSDepthRoomCulling = 4;
    private int tries = 0;
    private List<RoomNode> rooms;
    private List<Exit> availableExitPoints;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
        availableExitPoints = new List<Exit>();
        rooms = new List<RoomNode>();
        Random.InitState(seed);
        GenerateDungeon();
        refreshRoomIDs();
        SpawnEnemies();
        for (int i = 0; i < maxRoomCount; i++) {
            rooms[i].roomScript.DisableRenderers();
        }
        UpdateRooms(0);
    }

    //Function to generate the dungeon and add all the nodes into the rooms array with their appropiate IDs
    private void GenerateDungeon() {
        //Get The startPoint of the room
        int i = 0;
        int roomIndex;
        DungeonRoom dungeonRoom;

        do {
            if (i == 0) {
                //Instantiate the first room
                roomIndex = Random.Range(0, dungeonRoomList.roomList.Length);
                rooms.Add(new RoomNode(Instantiate(dungeonRoomList.roomList[roomIndex], transform.position, transform.rotation).transform));
                //Get the dungeonRoom
                dungeonRoom = rooms[i].Transform.GetComponent<DungeonRoom>();
                rooms[i].Transform.position += transform.position - dungeonRoom.EntryPoint.position;
                //Add its exit points to the available exit points
                for (int j = 0; j < dungeonRoom.ExitPoints.Length; j++) {
                    availableExitPoints.Add(new Exit(dungeonRoom.ExitPoints[j], i));
                }
                i++;
                continue;
            }
            //Choose a random exit point to pop
            int randomExit = Random.Range(0, availableExitPoints.Count);
            Exit exit = availableExitPoints[randomExit];
            //Instantiate the room
            //Choose which type of room to instantiate based on last room
            //If last room was corridor we have 90% chance of non corridor room
            //If last room was normal room we have 80% chance of corridor room
            RoomType roomToSpawn = RoomType.Normal;
            if (exit.point.RoomType == RoomType.Corridor) {
                if (Random.Range(0, 10) < 9f) {
                    roomToSpawn = RoomType.Normal;
                }
                else {
                    roomToSpawn = RoomType.Corridor;
                }
            }
            else if (exit.point.RoomType == RoomType.Normal) {
                if (Random.Range(0, 10) < 8f) {
                    roomToSpawn = RoomType.Corridor;
                }
                else {
                    roomToSpawn = RoomType.Normal;
                }

            }
            roomIndex = 0;
            switch (roomToSpawn) {
                case RoomType.Corridor:
                    roomIndex = Random.Range(0, dungeonRoomList.corridorList.Length);
                    rooms.Add(new RoomNode(Instantiate(dungeonRoomList.corridorList[roomIndex], exit.point.transform.position, exit.point.transform.rotation).transform));
                    break;
                case RoomType.Normal:
                    roomIndex = Random.Range(0, dungeonRoomList.roomList.Length);
                    rooms.Add(new RoomNode(Instantiate(dungeonRoomList.roomList[roomIndex], exit.point.transform.position, exit.point.transform.rotation).transform));
                    break;

                default:
                    break;
            }

            //Get the dungeonRoom
            dungeonRoom = rooms[i].Transform.GetComponent<DungeonRoom>();

            //Move the room so the start of the room and exit point that it was instantiated are at the same point

            rooms[i].Transform.position += exit.point.transform.position - dungeonRoom.EntryPoint.position;
            Physics.SyncTransforms();
            //Check if the room is colliding with any other room
            bool isColliding = false;
            for (int j = 0; j < rooms.Count; j++) {
                if (j != i && rooms[j].Transform.GetComponent<DungeonRoom>().IsColliding(dungeonRoom)) {
                    isColliding = true;
                    break;
                }
            }
            if (!isColliding) {
                //Add this room to the neighbours of the room where we got the exitPoint from
                dungeonRoom.SetID(i);
                rooms[exit.roomID].AddNeighbor(rooms[i]);
                //Remove the exit point from the available exit points
                availableExitPoints.RemoveAt(randomExit);
                //Add its exit points to the available exit points
                for (int j = 0; j < dungeonRoom.ExitPoints.Length; j++) {
                    availableExitPoints.Add(new Exit(dungeonRoom.ExitPoints[j], i));
                }
                i++;
            }
            else {
                Destroy(rooms[i].Transform.gameObject);
                rooms.RemoveAt(i);
            }
            tries++;
        }
        while (availableExitPoints.Count > 0 && i < maxRoomCount && tries < (maxRoomCount * maxRoomCount * maxRoomCount) / 2);


        //Once all rooms are built go through remaining exit points and close them
        for (int j = 0; j < availableExitPoints.Count; j++) {
            availableExitPoints[j].point.SetClosed();
        }

        for (int j = 0; j < rooms.Count; j++) {
            rooms[j].Transform.SetParent(transform);
        }



    }


    private void refreshRoomIDs() {
        for (int i = 0; i < rooms.Count; i++) {
            rooms[i].Transform.GetComponent<DungeonRoom>().SetID(i);

        }
    }


    public void UpdateRooms(int id) {
        if (id >= rooms.Count) return;
        int depth = 0;
        bool[] visited = new bool[rooms.Count];
        for (int i = 0; i < rooms.Count; i++) {
            visited[i] = false;
        }
        //Use room ids to access their array locations
        DFSUpdateRoomCulling(rooms[id], depth, ref visited);
        for (int i = 0; i < rooms.Count; i++) {
            visited[i] = false;
        }
        depth = 0;
    }

    private void SpawnEnemies() {
        //Go through each enemyPoint of each room and spawn enemies with a random chance
        for (int i = 0; i < rooms.Count; i++) {
            Transform[] enemyPoints = rooms[i].Transform.GetComponent<DungeonRoom>().EnemyPoints;
            for (int j = 0; j < enemyPoints.Length; j++) {
                if (Random.Range(0, 10) < 5) {
                    int enemyIndex = Random.Range(0, enemyListSo.enemies.Count);
                    Instantiate(enemyListSo.enemies[enemyIndex].enemyPrefab, enemyPoints[j].position, enemyPoints[j].rotation);
                }
            }
        }

    }
    private void DFSUpdateRoomCulling(RoomNode parent, int depth, ref bool[] visited) {
        //Dont need to handle rooms more than 1 room beyond
        if (depth > maxDFSDepthRoomCulling) {
            return;
        }
        //Make current node visited
        visited[parent.roomScript.ID] = true;
        if (depth < maxDFSDepthRoomCulling) {
            //Enable room if its in dfs
            parent.roomScript.EnableRenderers();
            //Visit all neighbours that haven't been visited
            for (int i = 0; i < parent.Neighbors.Count; i++) {
                if (!visited[parent.Neighbors[i].roomScript.ID]) {
                    DFSUpdateRoomCulling(parent.Neighbors[i], depth + 1, ref visited);
                }
            }
        }
        else if (depth == maxDFSDepthRoomCulling) {
            //Disable room if its on border of dfs
            parent.roomScript.DisableRenderers();
        }

    }


}
