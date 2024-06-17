using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Utility;

[Serializable]
public class RandomWeights
{
}
[Serializable]
public class RandomWeights<T> : RandomWeights where T : BlueprintReferenceBase
{
	public WeightPair<T>[] Weights;
}
