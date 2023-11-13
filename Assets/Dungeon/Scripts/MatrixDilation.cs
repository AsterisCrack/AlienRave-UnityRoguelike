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
}
