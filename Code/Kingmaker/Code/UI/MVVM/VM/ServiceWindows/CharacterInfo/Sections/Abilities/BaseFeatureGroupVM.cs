using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class BaseFeatureGroupVM<TFeatureVM> : BaseDisposable, IViewModel, IBaseDisposable, IDisposable where TFeatureVM : CharInfoFeatureVM
{
	public readonly List<TFeatureVM> FeatureList;

	public readonly string Label;

	public readonly string TooltipKey;

	public bool IsEmpty
	{
		get
		{
			List<TFeatureVM> featureList = FeatureList;
			if (featureList == null)
			{
				return true;
			}
			return !featureList.Any();
		}
	}

	public BaseFeatureGroupVM([NotNull] List<TFeatureVM> featuresListGroup, string label = null, string tooltipKey = null)
	{
		FeatureList = featuresListGroup;
		Label = label;
		TooltipKey = tooltipKey;
	}

	protected override void DisposeImplementation()
	{
		FeatureList?.ForEach(delegate(TFeatureVM f)
		{
			f.Dispose();
		});
		FeatureList?.Clear();
	}
}
