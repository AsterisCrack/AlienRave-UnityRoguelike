using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    //Script to handle a single room

    private int startingPosX;
    private int startingPosY;
    //setter and getter for the starting position
    public int StartingPosX
    {
        get { return startingPosX; }
        set { startingPosX = value; }
    }
    public int StartingPosY
    {
        get { return startingPosY; }
        set { startingPosY = value; }
    }

    private int width;
    private int height;

    private bool isSquare;

    private float randomwalkStepsMultiplier = 0.1f;
    private int brushSize = 3;

    private MatrixDilation matrixDilation = new MatrixDilation();

    bool[,] room;
    bool[,] wallsTop;
    bool[,] wallsBottom;
    bool[,] wallsLeft;
    bool[,] wallsRight;

    public Room(int startingPosX, int startingPosY, int width, int height, bool isSquare=true)
    {
        this.startingPosX = startingPosX;
        this.startingPosY = startingPosY;
        this.width = width;
        this.height = height;
        this.isSquare = isSquare;
    }

    public void GenerateRoom()
    {
        room = isSquare ? GenerateRoomSquare() : GenerateRoomRandom();
    }

    public bool[,] GetRoomMatrix()
    {
        return room;
    }

    //This function will return a matrix with the room shape if the room is square
    private bool[,] GenerateRoomSquare()
    {
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

        //lets try to do it with a morphological operation (dilation)
        //We need a padding of 2 for the 5x5 kernel

        bool[,] roomToDilate = matrixDilation.Padding(room, 2);

        //return the walls
        return matrixDilation.GetWallDilations(roomToDilate);
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
                //If the position is true and the tile in the tilemap is empty, we will paint it
                if (room[i, j] && tilemap.GetTile(new Vector3Int(startingPosX + i, startingPosY + j, 0)) == null)
                {
                    //We will create a new vector3Int with the position
                    Vector3Int position = new Vector3Int(startingPosX + i, startingPosY + j, 0);
                    //We will set the tile
                    tilemap.SetTile(position, ruletile);
                }
            }
        }
    }

    public void PaintWholeRoom(Tilemap groundMap, Tilemap CollidableMap, RuleTile groundRuletile, RuleTile leftRuletile, RuleTile rightRuletile, RuleTile topRuletile, RuleTile bottomRuletile, int startingPosX = 0, int startingPosY = 0)
    {
        (wallsTop, wallsBottom, wallsLeft, wallsRight) = GenerateRoomWalls(room);
        //Paint the room
        PaintRoom(room, groundMap, groundRuletile, startingPosX, startingPosY);
        //Paint the walls
        PaintRoom(wallsTop, CollidableMap, topRuletile, startingPosX, startingPosY);
        PaintRoom(wallsBottom, CollidableMap, bottomRuletile, startingPosX, startingPosY);
        PaintRoom(wallsLeft, CollidableMap, leftRuletile, startingPosX, startingPosY);
        PaintRoom(wallsRight, CollidableMap, rightRuletile, startingPosX, startingPosY);
    }
}
