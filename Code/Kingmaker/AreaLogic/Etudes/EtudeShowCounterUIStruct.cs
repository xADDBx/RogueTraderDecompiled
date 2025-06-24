using System;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.AreaLogic.Etudes;

public struct EtudeShowCounterUIStruct
{
	public string Id;

	public EtudeUICounterTypes Type;

	public string Label;

	public bool ShowSubLabel;

	[ConditionalShow("ShowSubLabel")]
	public string SubLabel;

	public Func<int> ValueGetter;

	public Func<int> TargetValueGetter;
}
