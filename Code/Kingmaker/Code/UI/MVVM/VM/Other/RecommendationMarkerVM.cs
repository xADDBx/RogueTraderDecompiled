using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Other;

public class RecommendationMarkerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public RecommendationType Recommendation { get; }

	public RecommendationMarkerVM(RecommendationType recommendation)
	{
		Recommendation = recommendation;
	}

	public RecommendationMarkerVM(int recommendation)
	{
		Recommendation = (RecommendationType)recommendation;
	}

	public RecommendationMarkerVM(bool recommend)
	{
		Recommendation = (recommend ? RecommendationType.Recommended : RecommendationType.Neutral);
	}

	protected override void DisposeImplementation()
	{
	}
}
