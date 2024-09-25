using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("73e5afdeaf554e848bf55ee88da3ffcd")]
public class CheckItemOriginGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility
{
	public ItemsItemOrigin Origin;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check origin of item is {Origin}";
	}

	protected override int GetBaseValue()
	{
		if (GetItem()?.Blueprint.Origin != Origin)
		{
			return 0;
		}
		return 1;
	}

	[CanBeNull]
	private ItemEntity GetItem()
	{
		object obj = this.GetAbilityWeapon();
		if (obj == null)
		{
			AbilityData ability = this.GetAbility();
			if ((object)ability == null)
			{
				return null;
			}
			obj = ability.SourceItem;
		}
		return (ItemEntity)obj;
	}
}
