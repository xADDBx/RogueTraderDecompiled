using System;
using System.Text;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("201fdfba40fb4b01878e7c56cf37d3cc")]
public class ContextActionDealDoTDamage : ContextAction
{
	public bool AllTypes;

	[HideIf("AllTypes")]
	public DOT Type;

	protected override void RunAction()
	{
		if (AllTypes)
		{
			if (base.Target.Entity == null)
			{
				return;
			}
			{
				foreach (DOT value in Enum.GetValues(typeof(DOT)))
				{
					DOTLogic.DealDamageByDOTImmediately(base.Target.Entity, base.Target.Entity, value);
				}
				return;
			}
		}
		if (base.Target.Entity != null)
		{
			DOTLogic.DealDamageByDOTImmediately(base.Target.Entity, base.Target.Entity, Type);
		}
	}

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		if (AllTypes)
		{
			builder.Append("Deal damage on target by all DoTs");
		}
		else
		{
			builder.Append("Deal damage on target by");
			builder.Append(" [");
			builder.Append(Type.ToString());
			builder.Append("]");
			builder.Append(" DOT type");
		}
		return builder.ToString();
	}
}
