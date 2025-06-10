using System;
using System.Text;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("82715097141d42d3a53731c856cc83d8")]
public class ContextActionWeakenDOT : ContextAction
{
	public DOT Type;

	public ContextValue WeakeningValue;

	protected override void RunAction()
	{
		if (base.Target.Entity is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.GetOptional<DOTLogic.PartDOTDirector>()?.WeakenDotsOfType(Type, WeakeningValue.Calculate(base.Context));
		}
	}

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Weaken DOT");
		builder.Append(" [");
		builder.Append(Type.ToString());
		builder.Append("] by ");
		builder.Append(WeakeningValue.ToString());
		return builder.ToString();
	}
}
