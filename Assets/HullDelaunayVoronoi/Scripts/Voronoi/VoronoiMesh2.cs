using System.Collections.Generic;

namespace ET
{

    public class VoronoiMesh2 : VoronoiMesh2<Vertex2>
    {

    }

    public class VoronoiMesh2<VERTEX> : VoronoiMesh<VERTEX>
    where VERTEX : class, IVertex, new() 
    {

        public VoronoiMesh2() : base(2) { }

        public override void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false)
        {
            IDelaunayTriangulation<VERTEX> delaunay = new DelaunayTriangulation2<VERTEX>();
            Generate(input, delaunay, assignIds, checkInput);
        }

    }

}












