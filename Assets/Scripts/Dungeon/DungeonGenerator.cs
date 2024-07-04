using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class DungeonGenerator : MonoBehaviour {

    [SerializeField] private DungeonRoomListSO dungeonRoomList;
    [SerializeField] private int seed;
    [SerializeField] private int maxRoomCount = 10;
    private List<Transform> rooms;
    private List<Transform> availableExitPoints;
    private void Awake() {
        availableExitPoints = new List<Transform>();
        rooms = new List<Transform>();
        Random.InitState(seed);
        GenerateDungeon();
    }

    private void GenerateDungeon() {
        availableExitPoints.Add(transform);
        //Get The startPoint of the room
        int i = 0;
        int tries = 0;
        do {
            tries++;
            //Choose a random exit point to pop
            int randomExitPoint = Random.Range(0, availableExitPoints.Count);
            Transform exitPoint = availableExitPoints[randomExitPoint];

            //Instantiate the room
            int roomIndex = Random.Range(0, dungeonRoomList.roomList.Length);
            rooms.Add(Instantiate(dungeonRoomList.roomList[roomIndex], exitPoint.position, exitPoint.rotation));

            //Get the dungeonRoom
            DungeonRoom dungeonRoom = rooms[i].GetComponent<DungeonRoom>();

            //Move the room so the start of the room and exit point that it was instantiated are at the same point
            rooms[i].transform.position += exitPoint.position - dungeonRoom.EntryPoint.position;
            Physics.SyncTransforms();
            //Check if the room is colliding with any other room
            bool isColliding = false;
            for (int j = 0; j < rooms.Count; j++) {
                if (j != i && rooms[j].GetComponent<DungeonRoom>().IsColliding(dungeonRoom)) {
                    isColliding = true;
                    break;
                }
            }
            if (!isColliding) {

                //Remove the exit point from the available exit points
                availableExitPoints.RemoveAt(randomExitPoint);
                //Add its exit points to the available exit points
                for (int j = 0; j < dungeonRoom.ExitPoint.Length; j++) {
                    availableExitPoints.Add(dungeonRoom.ExitPoint[j]);
                }
                i++;
                //Set the material to a random color
                dungeonRoom.SetMaterialColor(Random.ColorHSV());
                tries = 0;
            }
            else {
                Destroy(rooms[i].gameObject);
                rooms.RemoveAt(i);
            }

        }
        while (availableExitPoints.Count > 0 && i < maxRoomCount);
    }

    // Update is called once per frame
    void Update() {

    }
}
