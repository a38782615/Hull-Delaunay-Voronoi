using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace MyMap
{
    public struct UVTile
    {
        public static int tileSize = 136;
        public static float atlasWidth = 1024f;
        public int Id;
        public int2 position;
        public Rect uvRect;

        public UVTile(int id, int2 pos)
        {
            Id = id;
            position = pos;
            var tx = pos.x * tileSize;
            var ty = atlasWidth - (pos.y + 1) * tileSize;
            uvRect = new Rect(tx / atlasWidth, ty / atlasWidth, tileSize / atlasWidth, tileSize / atlasWidth);
        }

        public override int GetHashCode()
        {
            return position.x * 10000 + position.y;
        }
    }

    public struct UVTile2
    {
        public static int tileSize = 64;
        public static float atlasWidth = 512f;
        public int2 position;
        public Rect uvRect;

        public UVTile2(int2 pos)
        {
            position = pos % 7;
            var tx = pos.x * tileSize;
            var ty = pos.y * tileSize;
            uvRect = new Rect(tx / atlasWidth, ty / atlasWidth, tileSize / atlasWidth, tileSize / atlasWidth);
        }

        public override int GetHashCode()
        {
            return position.x * 10000 + position.y;
        }
    }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class DrawMap : MonoBehaviour
    {
        public Material material;
        public MeshRenderer m_meshRenderer;
        public MeshFilter meshFilter;
        public Texture2D mainTexture;
        public Texture2D overlayTexture;
        private MaterialPropertyBlock m_matPropBlock;

        private List<Vector3> s_vertices;

        private List<Vector2>
            m_uv; //NOTE: this is the only one not static because it's needed to update the animated tiles

        private List<Vector2>
            m_uv2; //NOTE: this is the only one not static because it's needed to update the animated tiles

        private List<int> s_triangles;
        public Dictionary<int, UVTile> m_uvMap;

        public Dictionary<int, int> Masks = new Dictionary<int, int>()
        {
            { 2, 1 }, { 8, 2 }, { 10, 3 }, { 11, 7 }, { 16, 5 },
            { 18, 6 }, { 22, 4 }, { 24, 8 }, { 26, 9 }, { 27, 10 },
            { 30, 11 }, { 31, 3 }, { 64, 13 }, { 66, 14 }, { 72, 15 },
            { 74, 16 }, { 75, 17 }, { 80, 18 }, { 82, 19 }, { 86, 20 },
            { 88, 21 }, { 90, 22 }, { 91, 23 }, { 94, 24 }, { 95, 25 },
            { 104, 13 }, { 106, 27 }, { 107, 5 }, { 120, 29 }, { 122, 30 },
            { 123, 31 }, { 126, 32 }, { 127, 33 }, { 208, 10 }, { 210, 35 },
            { 214, 2 }, { 216, 37 }, { 218, 38 }, { 219, 39 }, { 222, 40 },
            { 223, 41 }, { 248, 9 }, { 250, 43 }, { 251, 44 }, { 254, 45 },
            { 255, 49 }, { 0, 46 }
        };

        private void CreateMesh()
        {
            m_uvMap = new Dictionary<int, UVTile>();
            meshFilter = GetComponent<MeshFilter>();
            m_meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.hideFlags = HideFlags.HideAndDontSave;
                meshFilter.sharedMesh.name = name + "_mesh";
            }

            m_meshRenderer.material = material;
            if (m_matPropBlock == null)
            {
                m_matPropBlock = new MaterialPropertyBlock();
            }

            m_meshRenderer.GetPropertyBlock(m_matPropBlock);
            if (mainTexture != null)
            {
                m_matPropBlock.SetTexture("_MainTex", mainTexture);
                m_matPropBlock.SetTexture("_Texture2DCover", overlayTexture);
            }

            m_meshRenderer.SetPropertyBlock(m_matPropBlock);

            if (s_vertices == null)
            {
                s_vertices = new List<Vector3>();
                s_triangles = new List<int>();
                m_uv = new List<Vector2>();
                m_uv2 = new List<Vector2>();
            }

            s_vertices.Clear();
            s_triangles.Clear();
            m_uv.Clear();
            m_uv2.Clear();
        }
        //根据数据获取图块
        //在维诺图上画地图的图块

        Vector2[] s_tileUV = new Vector2[4];

        void DrawOne(Vector2Int pos, Rect tileUV0, Rect tileUV1)
        {
            //顶点位置
            float px0 = pos.x * cellSize.x;
            float py0 = pos.y * cellSize.y;
            float px1 = pos.x * cellSize.x + cellSize.x;
            float py1 = pos.y * cellSize.y + cellSize.y;

            int vertexIdx = s_vertices.Count;
            s_vertices.Add(new Vector3(px0, py0, 0));
            s_vertices.Add(new Vector3(px1, py0, 0));
            s_vertices.Add(new Vector3(px0, py1, 0));
            s_vertices.Add(new Vector3(px1, py1, 0));
            //三角形
            s_triangles.Add(vertexIdx + 3);
            s_triangles.Add(vertexIdx + 0);
            s_triangles.Add(vertexIdx + 2);
            s_triangles.Add(vertexIdx + 0);
            s_triangles.Add(vertexIdx + 3);
            s_triangles.Add(vertexIdx + 1);

            //UV贴图坐标
            float u00 = tileUV0.xMin;
            float v00 = tileUV0.yMin;
            float u01 = tileUV0.xMax;
            float v01 = tileUV0.yMax;
            s_tileUV[0] = new Vector2(u00, v00);
            s_tileUV[1] = new Vector2(u01, v00);
            s_tileUV[2] = new Vector2(u00, v01);
            s_tileUV[3] = new Vector2(u01, v01);
            for (int i = 0; i < 4; ++i)
            {
                m_uv.Add(s_tileUV[i]);
            }

            float u10 = tileUV1.xMin;
            float v10 = tileUV1.yMin;
            float u11 = tileUV1.xMax;
            float v11 = tileUV1.yMax;
            s_tileUV[0] = new Vector2(u10, v10);
            s_tileUV[1] = new Vector2(u11, v10);
            s_tileUV[2] = new Vector2(u10, v11);
            s_tileUV[3] = new Vector2(u11, v11);

            for (int i = 0; i < 4; ++i)
            {
                m_uv2.Add(s_tileUV[i]);
            }
        }

        public int TileCount = 7;
        public int cellW = 136;
        private Vector2 cellSize;
        public int width = 10;
        public int height = 10;

        public List<UVTile> tiles = new List<UVTile>();

        public int GetId(int x, int y)
        {
            x = x % TileCount;
            y = y % TileCount;
            return x + y * TileCount + 1;
        }

        void Draw()
        {
            m_uvMap.Clear();
            for (int j = 0; j < TileCount; j++)
            {
                for (int i = 0; i < TileCount; i++)
                {
                    var uv = new UVTile(GetId(i, j), new int2(i, j));
                    m_uvMap.Add(uv.Id, uv);
                }
            }

            cellSize = new Vector2(cellW / UVTile.atlasWidth, cellW / UVTile.atlasWidth);

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    var mask = GetMaskFromMap(i, j);
                    Debug.Log($"{i} : {j} : mask:" + mask);
                    var id = Masks[mask];
                    var uvtile = new UVTile2(new int2(i, j));
                    DrawOne(new Vector2Int(i, j), uvtile.uvRect, m_uvMap[id].uvRect);
                }
            }
        }

        int[] maski = new int[] { 1, 2, 4, 8, 0, 16, 32, 64, 128 };

        private int GetMaskFromMap(int x, int y)
        {
            var mask = 0;
            for (int j = -1; j < 2; j++)
            {
                for (int i = -1; i < 2; i++)
                {
                    var xi = i + 1;
                    var yi = j + 1;
                    var id = xi + yi * 3;
                    var msk = maski[id];
                    var xx = x + i;
                    var yy = y + j;
                    mask += msk * (xx < 0 || yy < 0 || xx >= width || yy >= height ? 0 : 1);
                }
            }

            return mask;
        }

        private void Render()
        {
            var mesh = meshFilter.sharedMesh;
            mesh.SetVertices(s_vertices);
            mesh.SetTriangles(s_triangles, 0);
            mesh.SetUVs(0, m_uv);
            mesh.SetUVs(1, m_uv2);
        }

        private void OnValidate()
        {
            CreateMesh();
            Draw();
            Render();
        }
    }
}