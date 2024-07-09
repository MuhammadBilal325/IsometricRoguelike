using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class DungeonGenerator : MonoBehaviour
{




    [SerializeField] private DungeonRoomListSO dungeonRoomList;
    [SerializeField] private int seed;
    [SerializeField] private int maxRoomCount = 10;
    private int tries = 0;
    private List<Transform> rooms;
    private List<ExitPoint> availableExitPoints;
    private void Awake()
    {
        availableExitPoints = new List<ExitPoint>();
        rooms = new List<Transform>();
        Random.InitState(seed);
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        availableExitPoints.Add(new ExitPoint(transform, RoomType.PlayerRoom));
        //Get The startPoint of the room
        int i = 0;
        do
        {
            //Choose a random exit point to pop
            int randomExitPoint = Random.Range(0, availableExitPoints.Count);
            ExitPoint exitPoint = availableExitPoints[randomExitPoint];

            //Instantiate the room
            //Choose which type of room to instantiate based on last room
            //If last room was corridor we have 90% chance of non corridor room
            //If last room was normal room we have 80% chance of corridor room
            RoomType roomToSpawn = RoomType.Normal;
            if (exitPoint.roomType == RoomType.Corridor)
            {
                if (Random.Range(0, 10) < 9f)
                {
                    roomToSpawn = RoomType.Normal;
                }
                else
                {
                    roomToSpawn = RoomType.Corridor;
                }
            }
            else if (exitPoint.roomType == RoomType.Normal)
            {
                if (Random.Range(0, 10) < 8f)
                {
                    roomToSpawn = RoomType.Corridor;
                }
                else
                {
                    roomToSpawn = RoomType.Normal;
                }

            }
            int roomIndex = 0;
            switch (roomToSpawn)
            {
                case RoomType.Corridor:
                    roomIndex = Random.Range(0, dungeonRoomList.corridorList.Length);
                    rooms.Add(Instantiate(dungeonRoomList.corridorList[roomIndex], exitPoint.transform.position, exitPoint.transform.rotation));
                    break;
                case RoomType.Normal:
                    roomIndex = Random.Range(0, dungeonRoomList.roomList.Length);
                    rooms.Add(Instantiate(dungeonRoomList.roomList[roomIndex], exitPoint.transform.position, exitPoint.transform.rotation));
                    break;

                default:
                    break;
            }

            //Get the dungeonRoom
            DungeonRoom dungeonRoom = rooms[i].GetComponent<DungeonRoom>();

            //Move the room so the start of the room and exit point that it was instantiated are at the same point

            rooms[i].transform.position += exitPoint.transform.position - dungeonRoom.EntryPoint.position;
            Physics.SyncTransforms();
            //Check if the room is colliding with any other room
            bool isColliding = false;
            for (int j = 0; j < rooms.Count; j++)
            {
                if (j != i && rooms[j].GetComponent<DungeonRoom>().IsColliding(dungeonRoom))
                {
                    isColliding = true;
                    break;
                }
            }
            if (!isColliding)
            {

                //Remove the exit point from the available exit points
                availableExitPoints.RemoveAt(randomExitPoint);
                //Add its exit points to the available exit points
                for (int j = 0; j < dungeonRoom.ExitPoints.Length; j++)
                {
                    availableExitPoints.Add(dungeonRoom.ExitPoints[j]);
                }
                dungeonRoom.SetMaterialColor(Random.ColorHSV());
                i++;
            }
            else
            {
                Destroy(rooms[i].gameObject);
                rooms.RemoveAt(i);
            }
            tries++;
        }
        while (availableExitPoints.Count > 0 && i < maxRoomCount && tries < (maxRoomCount * maxRoomCount * maxRoomCount) / 2);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
