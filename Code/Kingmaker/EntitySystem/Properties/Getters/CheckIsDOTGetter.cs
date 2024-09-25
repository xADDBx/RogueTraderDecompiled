using System;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using UnityEngine.Serialization;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5fb4a7ae99fd47f99ef012e9f4700ba0")]
public class CheckIsDOTGetter : PropertyGetter, PropertyContextAccessor.IOptionalRule, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptionalAbility
{
	[FormerlySerializedAs("ByType")]
	public bool CheckType;

	[ShowIf("CheckType")]
	public DOT Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!CheckType)
		{
			return "Is DOT";
		}
		return $"Is {Type} DOT";
	}

	protected override int GetBaseValue()
	{
		DOTLogic dOTLogic = (this.GetRule()?.Reason.Fact?.Blueprint ?? this.GetMechanicContext()?.AssociatedBlueprint ?? this.GetAbility()?.Blueprint)?.GetComponent<DOTLogic>();
		if (dOTLogic == null || (CheckType && dOTLogic.Type != Type))
		{
			return 0;
		}
		return 1;
	}
}
