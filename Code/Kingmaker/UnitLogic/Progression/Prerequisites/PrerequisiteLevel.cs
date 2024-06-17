using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Progression.Prerequisites;

[Serializable]
[TypeId("b9efad78f1e00e746b00c18088382c93")]
public class PrerequisiteLevel : Prerequisite
{
	public int Level;

	protected override bool MeetsInternal(IBaseUnitEntity unit)
	{
		return unit.ToBaseUnitEntity().Progression.CharacterLevel >= Level;
	}

	protected override string GetCaptionInternal()
	{
		return $"has at least {Level} levels";
	}
}
