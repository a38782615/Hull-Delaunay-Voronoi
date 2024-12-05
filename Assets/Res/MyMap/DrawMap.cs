﻿using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ET
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class DrawMap : MonoBehaviour
    {
        public MeshRenderer m_meshRenderer;
        public MeshFilter meshFilter;
        public Texture2D mainTexture;
        public Texture2D overlayTexture;
        private MaterialPropertyBlock m_matPropBlock;
        private MapLogic m_mapLogic;

        private void CreateMesh()
        {
            if (m_mapLogic == null)
            {
                m_mapLogic = new MapLogic();
                m_mapLogic.Init();
            }

            m_mapLogic.Clear();

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
        }

        List<Vector3> s_vertices;
        List<Vector2> m_uv;
        List<Vector2> m_uv2;

        private void Render()
        {
            var mesh = meshFilter.sharedMesh;
            mesh.SetVertices(ToList(m_mapLogic.s_vertices, s_vertices));
            mesh.SetTriangles(m_mapLogic.s_triangles, 0);
            mesh.SetUVs(0, ToList(m_mapLogic.m_uv, m_uv));
            mesh.SetUVs(1, ToList(m_mapLogic.m_uv2, m_uv2));
        }

        public List<Vector2> ToList(List<float2> points, List<Vector2> ps)
        {
            if (ps == null)
            {
                ps = new List<Vector2>(points.Count);
            }

            ps.Clear();
            foreach (var v in points)
            {
                ps.Add((Vector2)v);
            }

            return ps;
        }

        public List<Vector3> ToList(List<float3> points, List<Vector3> ps)
        {
            if (ps == null)
            {
                ps = new List<Vector3>(points.Count);
            }

            ps.Clear();
            foreach (var v in points)
            {
                ps.Add((Vector3)v);
            }

            return ps;
        }

        public uint seed = 10;

        private Dictionary<int2, bool> GenMap()
        {
            Random random = Random.CreateFromIndex(seed);
            var map = new Dictionary<int2, bool>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    map[new int2(i, j)] = random.NextBool();
                }
            }

            return map;
        }

        private void OnValidate()
        {
            CreateMesh();
            var map = GenMap();
            m_mapLogic.CreateMap(map);
            Render();
        }
    }
}