using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public float minHeight;
    public float maxHeight;
    public float rangeX; // 9000 * 2
    public float rangeZ; // 9000 * 2
    public Texture2D heightMap;

    public float chunkRangeX;
    public float chunkRangeZ;
    public int samplingRes;
    public int chunkLOD;
    private List<Chunk> chunks = new List<Chunk>();

    void Start() {
        CreateChunks(new Vector2(transform.position.x, transform.position.z));
    }

    public void CreateChunks(Vector2 location) {
        Vector2[] anchors = GetChunkAnchors(location);
        foreach (Vector2 anchor in anchors) {
            chunks.Add(new Chunk(anchor, samplingRes, this, chunkLOD));
        }
    }

    public Vector2Int ComputePixel(Vector2 location) {
        int xPixel = Mathf.Clamp(Mathf.RoundToInt(location.x / rangeX * heightMap.width), 0, heightMap.width);
        int zPixel = Mathf.Clamp(Mathf.RoundToInt(location.y / rangeZ * heightMap.height), 0, heightMap.height);
        return new Vector2Int(xPixel, zPixel);
    }

    private Vector2[] GetChunkAnchors(Vector2 location) {
        Vector2 anchor = GetChunkAnchor(location);
        Vector2[] neighbors = new Vector2[9];
        int count = 0;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                neighbors[count] = anchor + new Vector2(i * chunkRangeX, j * chunkRangeZ);
                count++;
            }
        }
        return neighbors;
    }

    private Vector2 GetChunkAnchor(Vector2 location) {
        float x = (int) (location.x / chunkRangeX) * chunkRangeX;
        float z = (int) (location.y / chunkRangeZ) * chunkRangeZ;
        return new Vector2(x, z);
    }
}
