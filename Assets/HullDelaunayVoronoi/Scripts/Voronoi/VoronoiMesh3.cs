﻿using System.Collections.Generic;

namespace ET
{
    public class VoronoiMesh3 : VoronoiMesh3<Vertex3>
    {

    }

    public class VoronoiMesh3<VERTEX> : VoronoiMesh<VERTEX>
        where VERTEX : class, IVertex, new() 
    {

        public VoronoiMesh3() : base(3) { }

        public override void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false)
        {
            IDelaunayTriangulation<VERTEX> delaunay = new DelaunayTriangulation3<VERTEX>();
            Generate(input, delaunay, assignIds, checkInput);
        }

    }

}












