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
[TypeId("57014b52b0dff6745859ceb1103d9739")]
public class ContextActionIntensifyDOT : ContextAction
{
	public DOT Type;

	public ContextValue IntensifyingValue;

	protected override void RunAction()
	{
		if (base.Target.Entity is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.GetOptional<DOTLogic.PartDOTDirector>()?.IntensifyDotsOfType(Type, IntensifyingValue.Calculate(base.Context));
		}
	}

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Intensify DOT");
		builder.Append(" [");
		builder.Append(Type.ToString());
		builder.Append("] by ");
		builder.Append(IntensifyingValue.ToString());
		return builder.ToString();
	}
}
