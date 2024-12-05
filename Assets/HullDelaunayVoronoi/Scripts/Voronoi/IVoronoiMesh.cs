using System.Collections.Generic;
namespace ET
{
	
	public interface IVoronoiMesh<VERTEX>
        where VERTEX : class, IVertex, new()
    {

		int Dimension { get; }

        IList<DelaunayCell<VERTEX>> Cells { get; }

        IList<VoronoiRegion<VERTEX>> Regions { get; }

        void Clear();
		
		void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false);
		
	}
	
}












