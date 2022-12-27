using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public float minHeight;
    public float maxHeight;
    public float rangeX; // 9000 * 2
    public float rangeZ; // 9000 * 2
    public Texture2D heightMap;

    public int samplingRes;
    public int numSubdivisions;
    private int finalResolution;

    const int MAX_MESH_RES = 128;

    void Start() {
        finalResolution = samplingRes * (int)Mathf.Pow(2f, numSubdivisions);

        // generate vertices
        Vector3[] terrainVertices = GenerateVerticesFromTerrain();
        for (int i = 0; i < numSubdivisions; i++) {
            terrainVertices = Subdivisor.GenerateNewVertices(terrainVertices, samplingRes * (int)Mathf.Pow(2f, i), samplingRes * (int)Mathf.Pow(2f, i));
        }

        // create meshes from vertices
        DivideVerticesIntoMeshes(terrainVertices);
    }

    private void GenerateMeshFromVertices(Vector3[] meshVertices) {
        int[] triangles = GenerateTriangles(MAX_MESH_RES);
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.uv = GetUVs(meshVertices);
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<MeshFilter>().mesh = mesh;
        plane.transform.position = transform.position;
        plane.transform.parent = transform;
    }

    private Vector3[] GenerateVerticesFromTerrain() {
        Vector3[] vertices = new Vector3[samplingRes * samplingRes];
        for (int i = 0; i < samplingRes; i++) {
            for (int j = 0; j < samplingRes; j++) {
                float x = rangeX * i / (samplingRes - 1f);
                float z = rangeZ * j / (samplingRes - 1f);
                Vector2Int pixel = ComputePixel(x, z);
                float normalizedHeight = heightMap.GetPixel(pixel.x, pixel.y).grayscale;
                float height = minHeight + normalizedHeight * maxHeight - minHeight;
                vertices[i * samplingRes + j] = new Vector3(x, height, z);
            }
        }
        return vertices;
    }

    private Vector2Int ComputePixel(float x, float z) {
        int xPixel = Mathf.Clamp(Mathf.RoundToInt(x / rangeX * heightMap.width), 0, heightMap.width);
        int zPixel = Mathf.Clamp(Mathf.RoundToInt(z / rangeZ * heightMap.height), 0, heightMap.height);
        return new Vector2Int(xPixel, zPixel);
    }

    private void DivideVerticesIntoMeshes(Vector3[] terrainVertices) {
        int xIndex = 0;
        while (xIndex < finalResolution) {
            int zIndex = 0;
            while (zIndex < finalResolution) {
                Vector3[] meshVertices = MeshVerticesAtAnchor(terrainVertices, xIndex, zIndex);
                GenerateMeshFromVertices(meshVertices);
                zIndex += MAX_MESH_RES - 1;
            }
            xIndex += MAX_MESH_RES - 1;
        }
    }

    private Vector3[] MeshVerticesAtAnchor(Vector3[] terrainVertices, int xIndex, int zIndex) {
        Vector3[] meshVertices = new Vector3[MAX_MESH_RES * MAX_MESH_RES];
        int vertexIndex = xIndex * finalResolution + zIndex;
        for (int i = 0; i < MAX_MESH_RES; i++) {
            if (xIndex + i >= finalResolution) break;
            for (int j = 0; j < MAX_MESH_RES; j++) {
                if (zIndex + j >= finalResolution) {
                    vertexIndex += finalResolution - j - (finalResolution - MAX_MESH_RES);
                    break;
                }
                int meshIndex = i * MAX_MESH_RES + j;
                meshVertices[meshIndex] = terrainVertices[vertexIndex++];
            }
            vertexIndex += finalResolution - MAX_MESH_RES;
        }
        return meshVertices;
    }

    private int[] GenerateTriangles(int meshRes) {
        int numGridSquares = (meshRes - 1) * (meshRes - 1);
        int numTriangles = 2 * numGridSquares;
        int[] triangles = new int[numTriangles * 3];
        for (int i = 0; i < meshRes - 1; i++) {
            for (int j = 0; j < meshRes - 1; j++) {
                int gridSquareIndex = i * (meshRes - 1) + j;
                int vertexIndex = i * meshRes + j;

                // bottom triangle
                triangles[gridSquareIndex * 6] = vertexIndex + meshRes + 1;
                triangles[gridSquareIndex * 6 + 1] = vertexIndex + meshRes;
                triangles[gridSquareIndex * 6 + 2] = vertexIndex;

                // top triangle
                triangles[gridSquareIndex * 6 + 3] = vertexIndex + meshRes + 1;
                triangles[gridSquareIndex * 6 + 4] = vertexIndex;
                triangles[gridSquareIndex * 6 + 5] = vertexIndex + 1;
            }
        }
        return triangles;
    }

    private Vector2[] GetUVs(Vector3[] vertices) {
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        return uvs;
    }
}
