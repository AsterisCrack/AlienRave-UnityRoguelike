using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixDilation
{
    // Function to perform morphological dilation on a binary matrix with a custom kernel
    public bool[,] Dilate(bool[,] inputMatrix, bool[,] kernel)
    {
        int width = inputMatrix.GetLength(0);
        int height = inputMatrix.GetLength(1);
        int kernelSize = kernel.GetLength(0);
        int kernelCenter = kernelSize / 2;
        bool[,] outputMatrix = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (inputMatrix[x, y])
                {
                    for (int i = -kernelCenter; i <= kernelCenter; i++)
                    {
                        for (int j = -kernelCenter; j <= kernelCenter; j++)
                        {
                            int newX = x + i;
                            int newY = y + j;
                            if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                            {
                                if (kernel[j + kernelCenter, i + kernelCenter])
                                {
                                    outputMatrix[newX, newY] = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return outputMatrix;
    }

    public bool[,] DilateOnlyFrontier(bool[,] inputMatrix, bool[,] kernel)
    {
        int width = inputMatrix.GetLength(0);
        int height = inputMatrix.GetLength(1);
        int kernelSize = kernel.GetLength(0);
        int kernelCenter = kernelSize / 2;
        bool[,] outputMatrix = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (inputMatrix[x, y])
                {
                    for (int i = -kernelCenter; i <= kernelCenter; i++)
                    {
                        for (int j = -kernelCenter; j <= kernelCenter; j++)
                        {
                            int newX = x + i;
                            int newY = y + j;
                            if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                            {
                                if (!inputMatrix[newX, newY])
                                {
                                    if (kernel[j + kernelCenter, i + kernelCenter])
                                    {
                                        outputMatrix[newX, newY] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return outputMatrix;
    }

    public bool[,] Padding(bool[,] matrix, int padding)
    {
        int width = matrix.GetLength(0) + 2 * padding;
        int height = matrix.GetLength(1) + 2 * padding;
        bool[,] result = new bool[width, height];
        //First, we need padding on the matrix. A padding of 2 for enabling the 5x5 kernel
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                result[i + padding, j + padding] = matrix[i, j];
            }
        }
        return result;
    }

    public (bool[,], bool[,], bool[,], bool[,]) GetWallDilations(bool[,] roomToDilate)
    {
        //Now, we will apply the kernel and obttain the walls
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

        bool[,] wallsBottom = DilateOnlyFrontier(roomToDilate, bottomKernel);
        bool[,] wallsTop = DilateOnlyFrontier(roomToDilate, topKernel);
        bool[,] wallsLeft = DilateOnlyFrontier(roomToDilate, leftKernel);
        bool[,] wallsRight = DilateOnlyFrontier(roomToDilate, rightKernel);

        return (wallsTop, wallsBottom, wallsLeft, wallsRight);
    }
}
