using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Other;

public class EntityVM : VMBase
{
	private Action m_EntityIconClick;

	private Action m_UnitNameClick;

	public Sprite Icon { get; private set; }

	public string Name { get; private set; }

	public TooltipBaseTemplate Tooltip { get; private set; }

	public EntityVM(BaseUnitEntity unit, ItemEntity item)
	{
		Icon = item?.Icon;
		Name = unit?.CharacterName;
		Tooltip = new TooltipTemplateItem(item);
		m_EntityIconClick = delegate
		{
		};
		m_UnitNameClick = delegate
		{
			UIUtility.EntityLinkActions.ShowUnit(unit);
		};
	}

	public EntityVM(BaseUnitEntity unit, AbilityData ability)
	{
		Icon = ability?.Icon;
		Name = unit?.CharacterName;
		Tooltip = new TooltipTemplateAbility(ability);
		m_EntityIconClick = delegate
		{
		};
		m_UnitNameClick = delegate
		{
			UIUtility.EntityLinkActions.ShowUnit(unit);
		};
	}

	public void OnEntityIconClick()
	{
		m_EntityIconClick();
	}

	public void OnUnitNameClick()
	{
		m_UnitNameClick();
	}

	protected override void DisposeImplementation()
	{
	}
}
