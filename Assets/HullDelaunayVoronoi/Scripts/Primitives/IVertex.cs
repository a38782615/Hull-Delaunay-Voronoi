﻿namespace ET
{
	public interface IVertex
	{

		int Dimension { get; }
		
		int Id { get; set; }

        int Tag { get; set; }
		
		float[] Position { get; set; }

        float Magnitude { get; }

        float SqrMagnitude { get; }

        float Distance(IVertex v);

        float SqrDistance(IVertex v);
		
	}
	
}
