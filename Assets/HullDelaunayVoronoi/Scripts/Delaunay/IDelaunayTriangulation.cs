using System.Collections.Generic;

namespace ET
{
	
	public interface IDelaunayTriangulation<VERTEX>
        where VERTEX : class, IVertex, new()
	{

        int Dimension { get; }
		
		IList<VERTEX> Vertices { get; }

		IList<DelaunayCell<VERTEX>> Cells { get; }

        VERTEX Centroid { get; }

		void Clear();

        void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false);
		
	}
	
}












