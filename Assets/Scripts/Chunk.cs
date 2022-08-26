using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public int resUV;
    public TerrainGenerator terrainGenerator;
    private GameObject plane;

    public Chunk(Vector2 location, int resUV, TerrainGenerator terrainGenerator) {
        this.minX = location.x;
        this.maxX = location.x + terrainGenerator.chunkRangeX;
        this.minZ = location.y;
        this.maxZ = location.y + terrainGenerator.chunkRangeZ;
        this.resUV = resUV;
        this.terrainGenerator = terrainGenerator;
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<MeshFilter>().mesh = GenerateMesh();
        plane.transform.position = terrainGenerator.transform.position;
        plane.transform.parent = terrainGenerator.transform;
    }

    public Mesh GenerateMesh() {
        VertexData vertexData = GenerateVertexData();
        int[] triangles = GenerateTriangles();
        Mesh mesh = new Mesh();
        mesh.vertices = vertexData.vertices;
        mesh.uv = vertexData.uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private VertexData GenerateVertexData() {
        Vector3[] vertices = new Vector3[resUV * resUV];
        Vector2[] uvs = new Vector2[resUV * resUV];
        for (int i = 0; i < resUV; i++) {
            for (int j = 0; j < resUV; j++) {
                float x = minX + (maxX - minX) * i / (resUV - 1);
                float z = minZ + (maxZ - minZ) * j / (resUV - 1);
                Vector2 uv = new Vector2(x, z);
                Vector2Int pixel = terrainGenerator.ComputePixel(uv);
                float normalizedHeight = terrainGenerator.heightMap.GetPixel(pixel.x, pixel.y).grayscale;
                float height = terrainGenerator.minHeight + normalizedHeight * (terrainGenerator.maxHeight - terrainGenerator.minHeight);
                vertices[i * resUV + j] = new Vector3(x, height, z);
                uvs[i * resUV + j] = uv;
            }
        }
        return new VertexData(vertices, uvs);
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

    private class VertexData {
        public readonly Vector3[] vertices;
        public readonly Vector2[] uvs;

        public VertexData(Vector3[] vertices, Vector2[] uvs) {
            this.vertices = vertices;
            this.uvs = uvs;
        }
    }

}
