using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Chunk {
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public int resUV;
    public int lod;
    public TerrainGenerator terrainGenerator;

    public Chunk(Vector2 location, int resUV, TerrainGenerator terrainGenerator, int lod) {
        this.minX = location.x;
        this.maxX = location.x + terrainGenerator.chunkRangeX;
        this.minZ = location.y;
        this.maxZ = location.y + terrainGenerator.chunkRangeZ;
        this.resUV = resUV;
        this.lod = lod;
        this.terrainGenerator = terrainGenerator;
        GenerateMeshes();
    }

    public void GenerateMeshes() {
        Vector3[] terrainVertices = GenerateVerticesFromTerrain();
        for (int i = 0; i < lod - 1; i++) {
            terrainVertices = Subdivisor.GenerateNewVertices(terrainVertices, resUV, resUV);
        }

        for (int i = 0; i < lod; i++) {
            for (int j = 0; j < lod; j++) {
                Vector3[] meshVertices = GetMeshVertices(terrainVertices, i, j);
                int[] triangles = GenerateTriangles();
                Mesh mesh = new Mesh();
                mesh.vertices = meshVertices;
                mesh.uv = GetUVs(meshVertices);
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.GetComponent<MeshFilter>().mesh = mesh;
                plane.transform.position = terrainGenerator.transform.position;
                plane.transform.parent = terrainGenerator.transform;
            }
        }
    }

    private Vector3[] GenerateVerticesFromTerrain() {
        Vector3[] vertices = new Vector3[resUV * resUV];
        for (int i = 0; i < resUV; i++) {
            for (int j = 0; j < resUV; j++) {
                float x = minX + (maxX - minX) * i / (resUV - 1);
                float z = minZ + (maxZ - minZ) * j / (resUV - 1);
                Vector2 uv = new Vector2(x, z);
                Vector2Int pixel = terrainGenerator.ComputePixel(uv);
                float normalizedHeight = terrainGenerator.heightMap.GetPixel(pixel.x, pixel.y).grayscale;
                float height = terrainGenerator.minHeight + normalizedHeight * (terrainGenerator.maxHeight - terrainGenerator.minHeight);
                vertices[i * resUV + j] = new Vector3(x, height, z);
            }
        }
        return vertices;
    }

    private int[] GenerateTriangles() {
        int numGridSquares = (resUV - 1) * (resUV - 1);
        int numTriangles = 2 * numGridSquares;
        int[] triangles = new int[numTriangles * 3];
        for (int i = 0; i < resUV - 1; i++) {
            for (int j = 0; j < resUV - 1; j++) {
                int gridSquareIndex = i * (resUV - 1) + j;
                int vertexIndex = i * resUV + j;

                // bottom triangle
                triangles[gridSquareIndex * 6] = vertexIndex + resUV + 1;
                triangles[gridSquareIndex * 6 + 1] = vertexIndex + resUV;
                triangles[gridSquareIndex * 6 + 2] = vertexIndex;

                // top triangle
                triangles[gridSquareIndex * 6 + 3] = vertexIndex + resUV + 1;
                triangles[gridSquareIndex * 6 + 4] = vertexIndex;
                triangles[gridSquareIndex * 6 + 5] = vertexIndex + 1;
            }
        }
        return triangles;
    }

    private Vector3[] GetMeshVertices(Vector3[] chunkVertices, int ix, int iz) {
        Vector3[] meshVertices = new Vector3[resUV * resUV];
        int vertexIndex = ix * resUV + iz * resUV;
        for (int i = 0; i < resUV; i++) {
            for (int j = 0; j < resUV; j++) {
                int meshIndex = i * resUV + j;
                meshVertices[meshIndex] = chunkVertices[vertexIndex++];
            }
            vertexIndex += resUV * lod - resUV;
        }
        return meshVertices;
    }

    private Vector2[] GetUVs(Vector3[] vertices) {
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        return uvs;
    }

}
