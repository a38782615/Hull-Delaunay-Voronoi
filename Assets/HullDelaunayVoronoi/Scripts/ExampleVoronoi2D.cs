using UnityEngine;
using System.Collections.Generic;
using CreativeSpore.SuperTilemapEditor;

namespace ET
{
    public class ExampleVoronoi2D : MonoBehaviour
    {
        public int NumberOfVertices = 1000;

        public float size = 10;

        public int seed = 0;

        private VoronoiMesh2 voronoi;

        private void Draw()
        {
            List<Vertex2> vertices = new List<Vertex2>();

            Random.InitState(seed);
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float x = size * Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1.0f, 1.0f);

                vertices.Add(new Vertex2(x, y));
            }

            voronoi = new VoronoiMesh2();
            voronoi.Generate(vertices);
        }

        void OnDrawGizmos()
        {
            Draw();
            int i = 0;
            
            foreach (VoronoiRegion<Vertex2> region in voronoi.Regions)
            {
                if (i > 1)
                {
                    break;
                }
                i++;
                bool draw = true;

                foreach (DelaunayCell<Vertex2> cell in region.Cells)
                {
                    if (!InBound(cell.CircumCenter))
                    {
                        draw = false;
                        break;
                    }

                    DrawPoint(cell.CircumCenter, Color.red);
                }

                if (!draw) continue;

                foreach (VoronoiEdge<Vertex2> edge in region.Edges)
                {
                    Vertex2 v0 = edge.From.CircumCenter;
                    Vertex2 v1 = edge.To.CircumCenter;
                    DrawLine(v0, v1);
                }
            }
 
        }

        private void DrawLine(Vertex2 v0, Vertex2 v1)
        {
            Gizmos.DrawLine(new Vector3(v0.X, v0.Y, 0), new Vector3(v1.X, v1.Y, 0));
        }

        private void DrawPoint(Vertex2 v, Color c )
        {
            float x = v.X;
            float y = v.Y;
            float s = 0.01f;
            GizmosEx.DrawDot(this.transform, new Vector3(x, y), s, c);
        }

        private bool InBound(Vertex2 v)
        {
            if (v.X < -size || v.X > size) return false;
            if (v.Y < -size || v.Y > size) return false;

            return true;
        }
    }
}