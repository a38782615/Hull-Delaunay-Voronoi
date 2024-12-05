using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{

	public abstract class Vertex : IVertex
    {
		public int Dimension { get { return (Position != null) ? Position.Length : 0; } }
		
		public int Id { get; set; }

        public int Tag { get; set; }
		
		public float[] Position { get; set; }

        public Vertex()
        {

        }

        public Vertex(int dimension)
		{
			Position = new float[dimension];
		}
		
		public Vertex(int dimension, int id)
		{
			Position = new float[dimension];
			Id = id;
		}

        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(SqrMagnitude);
            }
        }

        public float SqrMagnitude
        {
            get
            {
                float sum = 0.0f;

                for (int i = 0; i < Dimension; i++)
                    sum += Position[i] * Position[i];

                return sum;

            }
        }

        public float Distance(IVertex v)
        {
            return (float)math.sqrt(SqrDistance(v));
        }

        public float SqrDistance(IVertex v)
        {
            int dimension = math.min(Dimension, v.Dimension);
            float sum = 0.0f;

            for (int i = 0; i < dimension; i++)
            {
                float x = Position[i] - v.Position[i];
                sum += x * x;
            }

            return sum;
        }

    }

    /// <summary>
    /// Compare vertices based on their indices.
    /// </summary>
    public class VertexIdComparer<VERTEX> : IComparer<VERTEX>
        where VERTEX : IVertex, new()
    {
        public int Compare(VERTEX v0, VERTEX v1)
        {
            return v0.Id.CompareTo(v1.Id);
        }
    }
}
