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
            var ty = atlasWidth - (pos.y+1) * tileSize;
            uvRect = new Rect(tx/atlasWidth, ty/atlasWidth, tileSize / atlasWidth, tileSize / atlasWidth);
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

        private List<int> s_triangles;
        public Dictionary<int, UVTile> m_uvMap;

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
            }

            s_vertices.Clear();
            s_triangles.Clear();
            m_uv.Clear();
        }
        //根据数据获取图块
        //在维诺图上画地图的图块

        Vector2[] s_tileUV = new Vector2[4];

        void DrawOne(Vector2Int pos, Rect tileUV)
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
            float u0 = tileUV.xMin;
            float v0 = tileUV.yMin;
            float u1 = tileUV.xMax;
            float v1 = tileUV.yMax;
            s_tileUV[0] = new Vector2(u0, v0);
            s_tileUV[1] = new Vector2(u1, v0);
            s_tileUV[2] = new Vector2(u0, v1);
            s_tileUV[3] = new Vector2(u1, v1);

            for (int i = 0; i < 4; ++i)
            {
                m_uv.Add(s_tileUV[i]);
            }
        }

        public int TileCount = 8;
        public int cellW = 136;
        private Vector2 cellSize;
        public int width = 10;
        public int height = 10;

        public List<UVTile> tiles = new List<UVTile>();

        public int x = 0;
        public int y = 0;
        public int Id => GetId(x, y);

        public int GetId(int x, int y)
        {
            x = x%TileCount;
            y = y%TileCount;
            return x * 10000 + y;
        }

        void Draw()
        {
            m_uvMap.Clear();
            for (int i = 0; i < TileCount; i++)
            {
                for (int j = 0; j < TileCount; j++)
                {
                    var id  = GetId(i, j);
                    var uv = new UVTile(id, new int2(i, j));
                    m_uvMap.Add(uv.Id, uv);
                }
            }
            cellSize = new Vector2(cellW / UVTile.atlasWidth, cellW / UVTile.atlasWidth);
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    DrawOne(new Vector2Int(i, j), m_uvMap[Id].uvRect);
                }
            }
        }

        private void Render()
        {
            var mesh = meshFilter.sharedMesh;
            mesh.SetVertices(s_vertices);
            mesh.SetTriangles(s_triangles, 0);
            mesh.SetUVs(0, m_uv);
        }

        private void OnValidate()
        {
            CreateMesh();
            Draw();
            Render();
        }
    }
}