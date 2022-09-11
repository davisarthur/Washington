using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class Subdivisor {

    private static Vector3 EvenVertexHeight(int index, Vector3[] oldVertices, int oldNx, float beta) {
        Vector3 finalHeight = new Vector3();
        int k = 6;
        finalHeight += beta * oldVertices[index - oldNx - 1];
        finalHeight += beta * oldVertices[index - oldNx];
        finalHeight += beta * oldVertices[index - 1];
        finalHeight += beta * oldVertices[index + 1];
        finalHeight += beta * oldVertices[index + oldNx];
        finalHeight += beta * oldVertices[index + oldNx + 1];
        finalHeight += (1 - k * beta) * oldVertices[index];
        return finalHeight;
    }

    private static Vector3 OddVertexEvenRowHeight(int indexLeft, Vector3[] oldVertices, int oldNx) {
        Vector3 finalHeight = new Vector3();
        finalHeight += 1.0f / 8.0f * oldVertices[indexLeft - oldNx];
        finalHeight += 1.0f / 8.0f * oldVertices[indexLeft + oldNx + 1];
        finalHeight += 3.0f / 8.0f * oldVertices[indexLeft];
        finalHeight += 3.0f / 8.0f * oldVertices[indexLeft + 1];
        return finalHeight;
    }

    private static Vector3 OddVertexOddRowEvenColumnHeight(int indexUp, Vector3[] oldVertices, int oldNx) {
        Vector3 finalHeight = new Vector3();
        finalHeight += 1.0f / 8.0f * oldVertices[indexUp - 1];
        finalHeight += 1.0f / 8.0f * oldVertices[indexUp + oldNx + 1];
        finalHeight += 3.0f / 8.0f * oldVertices[indexUp];
        finalHeight += 3.0f / 8.0f * oldVertices[indexUp + oldNx];
        return finalHeight;
    }

    private static Vector3 OddVertexOddRowOddColumnHeight(int indexUpLeft, Vector3[] oldVertices, int oldNx) {
        Vector3 finalHeight = new Vector3();
        finalHeight += 1.0f / 8.0f * oldVertices[indexUpLeft + 1];
        finalHeight += 1.0f / 8.0f * oldVertices[indexUpLeft + oldNx];
        finalHeight += 3.0f / 8.0f * oldVertices[indexUpLeft];
        finalHeight += 3.0f / 8.0f * oldVertices[indexUpLeft + oldNx + 1];
        return finalHeight;
    }

    // Assumes old vertices are structured in grid pattern
    public static Vector3[] GenerateNewVertices(Vector3[] oldVertices, int oldNx, int oldNz) {
        float beta = 0.1f;
        Vector3[] newVertices = new Vector3[oldVertices.Length * 4];
        for (int i = 0; i < 2 * oldNx; i++) {
            for (int j = 0; j < 2 * oldNz; j++) {
                int newIndex = i * oldNx * 2 + j;
                int oldIndex = oldNx * (i / 2) + j / 2;
                try {
                    // TODO: Account for edge rules
                    if (oldIndex < oldNx || oldIndex >= oldNx * (oldNz - 1) || oldIndex % oldNx == 0 || (oldIndex + 1) % oldNx == 0) {
                        newVertices[newIndex] = oldVertices[oldIndex];
                        continue;
                    }


                    if (i % 2 == 0 && j % 2 == 0) {
                        newVertices[newIndex] = EvenVertexHeight(oldIndex, oldVertices, oldNx, beta);
                    } else if (i % 2 == 0 && j % 2 == 1) {
                        newVertices[newIndex] = OddVertexEvenRowHeight(oldIndex, oldVertices, oldNx);
                    } else if (i % 2 == 1 && j % 2 == 0) {
                        newVertices[newIndex] = OddVertexOddRowEvenColumnHeight(oldIndex, oldVertices, oldNx);
                    } else if (i % 2 == 1 && j % 2 == 1) {
                        newVertices[newIndex] = OddVertexOddRowOddColumnHeight(oldIndex, oldVertices, oldNx);
                    }
                } catch (Exception e) {
                    Debug.Log("i: " + i + ", j: " + j);
                    Debug.Log("oldNx: " + oldNx);
                    Debug.Log("newIndex: " + newIndex + ", " + newVertices.Length);
                    Debug.Log("oldIndex: " + oldIndex + ", " + oldVertices.Length);
                    Debug.Log(e);
                }
            }
        }

        return newVertices;
    }

    
}
