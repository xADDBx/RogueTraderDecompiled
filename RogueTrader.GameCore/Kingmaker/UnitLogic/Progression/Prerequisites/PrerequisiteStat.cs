using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Progression.Prerequisites;

[Serializable]
[TypeId("3acf9db791c042a4b9588426022193a8")]
public class PrerequisiteStat : Prerequisite
{
	public StatType Stat;

	public int MinValue;

	protected override bool MeetsInternal(IBaseUnitEntity unit)
	{
		return unit.MeetsPrerequisite(this);
	}

	protected override string GetCaptionInternal()
	{
		return $"{Stat} at least {MinValue}";
	}
}
