namespace Owlcat.Runtime.Visual.Waaagh.BilateralUpsample;

public class BilateralUpsample
{
	internal static float[] DistanceBasedWeights_3x3 = new float[36]
	{
		0.324652f, 0.535261f, 0.119433f, 0.535261f, 0.882497f, 0.196912f, 0.119433f, 0.196912f, 0.0439369f, 0.119433f,
		0.535261f, 0.324652f, 0.196912f, 0.882497f, 0.535261f, 0.0439369f, 0.196912f, 0.119433f, 0.119433f, 0.196912f,
		0.0439369f, 0.535261f, 0.882497f, 0.196912f, 0.324652f, 0.535261f, 0.119433f, 0.0439369f, 0.196912f, 0.119433f,
		0.196912f, 0.882497f, 0.535261f, 0.119433f, 0.535261f, 0.324652f
	};

	internal static float[] DistanceBasedWeights_2x2 = new float[16]
	{
		0.324652f, 0.535261f, 0.535261f, 0.882497f, 0.535261f, 0.324652f, 0.882497f, 0.535261f, 0.535261f, 0.882497f,
		0.324652f, 0.535261f, 0.882497f, 0.535261f, 0.535261f, 0.324652f
	};

	internal static float[] TapOffsets_2x2 = new float[32]
	{
		-1f, -1f, 0f, -1f, -1f, 0f, 0f, 0f, 0f, -1f,
		1f, -1f, 0f, 0f, 1f, 0f, -1f, 0f, 0f, 0f,
		-1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,
		1f, 1f
	};
}
