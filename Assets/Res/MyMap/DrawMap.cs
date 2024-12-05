using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MyMap
{
    public struct MapNode
    {
        public int2 position;
    }
    
    public struct UVTile
    {
        //1 2 4
        //8 0 16
        //32 64 128 
        public static int[] maski = new int[] { 1 << 5, 1 << 6, 1 << 7, 1 << 3, 0, 1 << 4, 1, 1 << 1, 1 << 2 };
        public static Dictionary<int, int> Masks = new Dictionary<int, int>(){
            { 214, 1 }, { 248, 2 }, { 208, 3 }, { 107, 4 }, { 66, 5 }, { 104, 6 }, { 64, 7 },
            { 31, 8 }, { 22, 9 }, { 24, 10 }, { 16, 11 }, { 11, 12 }, { 2, 13 }, { 8, 14 }, { 0, 15 },
            { 255, 16 }, { 254, 17 }, { 251, 18 }, { 250, 19 }, { 127, 20 }, { 126, 21 }, { 123, 22 }, { 122, 23 },
            { 223, 24 }, { 224, 25 }, { 219, 26 }, { 218, 27 }, { 95, 28 }, { 94, 29 }, { 91, 30 }, { 90, 31 },
            { 120, 32 }, { 75, 33 }, { 32, 34 }, { 210, 35 }, { 88, 36 }, { 74, 37 }, { 26, 38 }, { 82, 39 },
            { 216, 40 }, { 106, 41 }, { 27, 42 }, { 86, 43 }, { 80, 44 }, { 72, 45 }, { 18, 46 }, { 10, 47 }
        };
        public static int TileCount = 7;
        public static int tileSize = 136;
        public static int atlasWidth = 1024;
        public static float cellSize = tileSize*1f / atlasWidth;
        public int Id;
        public int2 position;
        public Rect uvRect;

        public UVTile(int2 pos)
        {
            Id = GetId(pos.x, pos.y);
            position = pos;
            var tx = position.x * cellSize;
            var ty = 1 - (position.y + 1) * cellSize;
            uvRect = new Rect(tx , ty , cellSize, cellSize);
        }

        public static int GetId(int x, int y)
        {
            x %= TileCount;
            y %= TileCount;
            return x + y * TileCount;
        }
        public override int GetHashCode()
        {
            return position.x * 10000 + position.y;
        }
    }

    public struct UVTile2
    {
        public static int tileSize = 64;
        public static int atlasWidth = 512;
        public static int count = atlasWidth / tileSize;
        public static float cellSize = tileSize*1f / atlasWidth;
        public int2 position;
        public Rect uvRect;

        public UVTile2(int2 pos)
        {
            position = pos % count;
            var tx = position.x * cellSize;
            var ty = position.y * cellSize;
            uvRect = new Rect(tx , ty , cellSize, cellSize);
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

        private void CreateMesh()
        {
            m_uvMap = new Dictionary<int, UVTile>();
            s_vertices = new List<Vector3>();
            s_triangles = new List<int>();
            m_uv = new List<Vector2>();
            m_uv2 = new List<Vector2>();
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = name + "_mesh"
            };
            meshFilter.sharedMesh.Clear();
            
            m_meshRenderer = GetComponent<MeshRenderer>();
            if (m_matPropBlock == null)
            {
                m_matPropBlock = new MaterialPropertyBlock();
            }
            m_matPropBlock.SetTexture("_MainTex", mainTexture);
            m_matPropBlock.SetTexture("_Texture2DCover", overlayTexture);
            m_meshRenderer.SetPropertyBlock(m_matPropBlock);
            
            s_vertices.Clear();
            s_triangles.Clear();
            m_uv.Clear();
            m_uv2.Clear();
        }
        //根据数据获取图块
        //在维诺图上画地图的图块

        Vector2[] s_tileUV = new Vector2[4];
        void DrawOne(Rect posRect, Rect tileUV0, Rect tileUV1)
        {
            //顶点位置
            float px0 = posRect.xMin;
            float py0 = posRect.yMin;
            float px1 = posRect.xMax;
            float py1 = posRect.yMax;

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

        public int width = 10;
        public int height = 10;

        void Draw()
        {
            
            m_uvMap.Clear();
            for (int j = 0; j < UVTile.TileCount; j++)
            {
                for (int i = 0; i < UVTile.TileCount; i++)
                {
                    var uv = new UVTile(new int2(i, j));
                    m_uvMap.Add(uv.Id, uv);
                }
            }

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    var hasNode = HasNode(i, j);
                    if (hasNode)
                    {
                        var uvtile2 = new UVTile2(new int2(i, j));
                        var mask = GetMaskFromMap(i, j);
                        var id = UVTile.Masks[mask];
                        DrawOne(new Rect(i * UVTile.cellSize, j * UVTile.cellSize, UVTile.cellSize, UVTile.cellSize), 
                            uvtile2.uvRect,
                            m_uvMap[id].uvRect);
                    }
                }
            }
        }

        private Dictionary<int2,bool> Map = new Dictionary<int2, bool>();

        private void CreateMap()
        {
            Map.Clear();
            Map = new Dictionary<int2, bool>(){
                {new int2(3, 2), true},
                {new int2(3, 3), true},
                {new int2(4, 2), true},
                {new int2(4, 3), true},
                {new int2(4, 4), true},
                {new int2(4, 5), true},
                {new int2(5, 4), true},
                {new int2(5, 5), true},
                {new int2(5, 7), true},
                {new int2(6, 2), true},
                {new int2(6, 3), true},
                {new int2(6, 5), true},
                {new int2(6, 7), true},
                {new int2(7, 2), true},
                {new int2(7, 3), true},
                {new int2(7, 5), true},
                {new int2(7, 6), true},
                {new int2(7, 7), true},
            };
        }
        
        private bool HasNode(int x, int y)
        {
            var ret = false;
            Map.TryGetValue(new int2(x, y), out ret);
            return ret;
        }
        
        private int GetMaskFromMap(int x, int y)
        {
            var mask = 0;
            for (int j = -1; j < 2; j++)
            {
                for (int i = -1; i < 2; i++)
                {
                    //坐标
                    var xx = x + i;
                    var yy = y + j;
                    
                    //mask坐标
                    var xi = i + 1;
                    var yi = j + 1;
                    var mskid = xi + yi * 3;
                    int msk = 0;
                    if (i == 0 || j == 0)
                    {
                        // 正方向
                        msk = (HasNode(xx,yy) ? UVTile.maski[mskid]: 0);
                    }
                    else
                    {
                        //角落如果有 需要判断 角落2边HasNode
                        msk = ((HasNode(xx, yy) && HasNode(xx, y) && HasNode(x, yy)) ? UVTile.maski[mskid] : 0);
                    }
                    
                    mask +=  msk;
                    if (msk > 0)
                    {
                        Debug.Log($"msk1 {i} : {j} :" + msk);
                    }
                }
            }
            Debug.Log($"{x} : {y} : mask1:" + mask);
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
            CreateMap();
            CreateMesh();
            Draw();
            Render();
        }
    }
}