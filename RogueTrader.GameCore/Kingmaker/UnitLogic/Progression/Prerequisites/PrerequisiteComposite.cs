using System;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Progression.Prerequisites;

[Serializable]
[TypeId("3ebda9716ea24139a3a4d3f7c49dc90d")]
public class PrerequisiteComposite : Prerequisite
{
	public PrerequisitesList Prerequisites;

	public override bool IsRelyingOnFeature(BlueprintFact featureToAnalyze)
	{
		if (Not)
		{
			return false;
		}
		return (Prerequisites?.List?.Any((Prerequisite p) => p.IsRelyingOnFeature(featureToAnalyze))).GetValueOrDefault();
	}

	protected override bool MeetsInternal(IBaseUnitEntity unit)
	{
		return Prerequisites.Meet(unit);
	}

	protected override string GetCaptionInternal()
	{
		return Prerequisites.ToString();
	}
}
