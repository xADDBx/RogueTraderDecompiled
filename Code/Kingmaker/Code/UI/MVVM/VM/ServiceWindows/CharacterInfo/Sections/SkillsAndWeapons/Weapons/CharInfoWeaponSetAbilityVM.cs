using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class CharInfoWeaponSetAbilityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>(null);

	public TooltipTemplateAbility Tooltip { get; }

	public CharInfoWeaponSetAbilityVM(BlueprintAbility ability, BlueprintItem weapon, MechanicEntity caster = null)
	{
		Icon.Value = ability.Icon.Or(UIConfig.Instance.UIIcons.DefaultAbilityIcon);
		Tooltip = new TooltipTemplateAbility(ability, weapon, caster);
	}

	protected override void DisposeImplementation()
	{
	}
}
