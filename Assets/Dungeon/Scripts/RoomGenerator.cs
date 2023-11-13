using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    //Script to generate a single room
    //This script will be attached to the room prefab
    [Header("Room Settings")]
    [SerializeField] private int startingPosX;
    [SerializeField] private int startingPosY;

    [SerializeField] private int minWidth;
    [SerializeField] private int maxWidth;

    [SerializeField] private int minHeight;
    [SerializeField] private int maxHeight;

    [SerializeField] private bool isSquare;
    [SerializeField] private float randomwalkStepsMultiplier;
    [SerializeField] private int brushSize;

    [Header("Tiles")]
    //We will use tile rules to paint the room
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private RuleTile groundTiles;
    [SerializeField] private RuleTile leftWallTiles;
    [SerializeField] private RuleTile rightWallTiles;
    [SerializeField] private RuleTile topWallTiles;
    [SerializeField] private RuleTile bottomWallTiles;

    MatrixDilation matrixDilation = new MatrixDilation();

    bool[,] room;
    bool[,] wallsTop;
    bool[,] wallsBottom;
    bool[,] wallsLeft;
    bool[,] wallsRight;

    private void Awake()
    {
        GenerateRoom();

        PaintRoom(room, groundTilemap, groundTiles, startingPosX, startingPosY);
        PaintRoom(wallsTop, wallTilemap, topWallTiles, startingPosX, startingPosY);
        PaintRoom(wallsBottom, wallTilemap, bottomWallTiles, startingPosX, startingPosY);
        PaintRoom(wallsLeft, wallTilemap, leftWallTiles, startingPosX, startingPosY);
        PaintRoom(wallsRight, wallTilemap, rightWallTiles, startingPosX, startingPosY);
    }

    public void GenerateRoom()
    {
        room = isSquare ? GenerateRoomSquare() : GenerateRoomRandom();
        (wallsTop, wallsBottom, wallsLeft, wallsRight) = GenerateRoomWalls(room);
    }

    public (bool[,], bool[,], bool[,], bool[,], bool[,]) GetRoomMatrix()
    {
        return (room, wallsTop, wallsBottom, wallsLeft, wallsRight);
    }

    //This function will return a matrix with the room shape if the room is square
    private bool[,] GenerateRoomSquare()
    {
        //We will generate a random width and height
        int width = Random.Range(minWidth, maxWidth);
        int height = Random.Range(minHeight, maxHeight);

        //We will create a matrix with the width and height
        bool[,] roomShape = new bool[width, height];

        //We will fill the matrix with 1s
        for (int i = 0; i < roomShape.GetLength(0); i++)
        {
            for (int j = 0; j < roomShape.GetLength(1); j++)
            {
                roomShape[i, j] = true;
            }
        }
        //We will return the matrix
        return roomShape;
    }

    //This function will return a matrix with the room shape if the room is not square
    private bool[,] GenerateRoomRandom()
    {
        // Non-rectangular room, so it will be a random shape
        // We will generate a random width and height
        int width = Random.Range(minWidth, maxWidth);
        int height = Random.Range(minHeight, maxHeight);

        // We will use random walk to generate the room shape
        bool[,] roomShape = new bool[width, height];
        int y = height / 2;
        int x = width / 2;
        int randomwalkSteps = (int)Mathf.Round(width * height * randomwalkStepsMultiplier);

        for (int i = 0; i < randomwalkSteps; i++)
        {
            // Apply the circular brush (expand the room)
            for (int brushX = -brushSize; brushX <= brushSize; brushX++)
            {
                for (int brushY = -brushSize; brushY <= brushSize; brushY++)
                {
                    // Calculate the distance from the current position (x, y) to the brush position (newX, newY)
                    int newX = x + brushX;
                    int newY = y + brushY;
                    int distanceSquared = brushX * brushX + brushY * brushY;

                    // Check if the new position is within the room bounds and within the circular brush
                    if (newX >= 0 && newX < width && newY >= 0 && newY < height && distanceSquared <= brushSize * brushSize)
                    {
                        roomShape[newX, newY] = true;
                    }
                }
            }

            // Move to a new position using random walk
            int direction = Random.Range(0, 4);

            switch (direction)
            {
                case 0: // Up
                    if (y < height - 1)
                        y++;
                    break;
                case 1: // Down
                    if (y > 0)
                        y--;
                    break;
                case 2: // Left
                    if (x > 0)
                        x--;
                    break;
                case 3: // Right
                    if (x < width - 1)
                        x++;
                    break;
            }
        }

        // We will return the matrix
        return roomShape;
    }

    private (bool[,], bool[,], bool[,], bool[,]) GenerateRoomWalls(bool[,] room)
    {
        //We will generate 4 bool arrays with the walls, left right top and bottom
        //Walls have a height of 2 on top and 1 the rest of sides
        //The walls will be generated based on the room shape

        int height = room.GetLength(1) + 4;
        int width = room.GetLength(0) + 4;
        bool[,] wallsTop = new bool[width, height];
        bool[,] wallsBottom = new bool[width, height];
        bool[,] wallsLeft = new bool[width, height];
        bool[,] wallsRight = new bool[width, height];

        //lets try to do it with a morphological operation (dilation)
        //We need a padding of 2 for the 5x5 kernel
        bool[,] roomToDilate = matrixDilation.Padding(room, 2);

        //Set the walls to false
        for (int i = 0; i < wallsTop.GetLength(0); i++)
        {
            for (int j = 0; j < wallsTop.GetLength(1); j++)
            {
                wallsTop[i, j] = false;
                wallsBottom[i, j] = false;
                wallsLeft[i, j] = false;
                wallsRight[i, j] = false;
            }
        }

        //Now, we will apply the kernel and obteain the walls
        //We will use a 3x3 kernel for most of the walls, but the top wall will be a 5x5 kernel
        bool[,] bottomKernel = new bool[3, 3] { { true, true, true }, 
                                                { false, true, false }, 
                                                { false, false, false } };

        bool[,] topKernel = new bool[5, 5] {    { false, false, false, false, false }, 
                                                { false, false, false, false, false }, 
                                                { false, false, true, false, false }, 
                                                { false, true, true, true, false }, 
                                                { false, true, true, true, false } };

        bool[,] leftKernel = new bool[3, 3] {   { false, false, false }, 
                                                { false, true, true }, 
                                                { false, false, false } };

        bool[,] rightKernel = new bool[3, 3] {  { false, false, false }, 
                                                { true, true, false }, 
                                                { false, false, false } };

        wallsBottom = matrixDilation.DilateOnlyFrontier(roomToDilate, bottomKernel);
        wallsTop = matrixDilation.DilateOnlyFrontier(roomToDilate, topKernel);
        wallsLeft = matrixDilation.DilateOnlyFrontier(roomToDilate, leftKernel);
        wallsRight = matrixDilation.DilateOnlyFrontier(roomToDilate, rightKernel);

        //return the walls
        return (wallsTop, wallsBottom, wallsLeft, wallsRight);
    }

    

    private void PaintRoom(bool[,] room, Tilemap tilemap, RuleTile ruletile, int startingPosX = 0, int startingPosY = 0)
    {
        //First we need to look for the center of the room
        int centerX = room.GetLength(0) / 2;
        int centerY = room.GetLength(1) / 2;
        //Now we set starting position. The room starts painting from the bottom left corner
        startingPosX -= centerX;
        startingPosY -= centerY;
        //Function to paint the room
        //We will use the ground tilemap to paint it
        for (int i = 0; i < room.GetLength(0); i++)
        {
            for (int j = 0; j < room.GetLength(1); j++)
            {
                //If the position is true, we will paint it
                if (room[i, j])
                {
                    //We will create a new vector3Int with the position
                    Vector3Int position = new Vector3Int(startingPosX + i, startingPosY + j, 0);
                    //We will set the tile
                    tilemap.SetTile(position, ruletile);
                }
            }
        }
    }
}
