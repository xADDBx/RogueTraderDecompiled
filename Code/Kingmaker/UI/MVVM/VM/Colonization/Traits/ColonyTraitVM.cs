using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Traits;

public class ColonyTraitVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> MechanicString = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ColonyTraitVM(BlueprintColonyTrait trait)
	{
		Name.Value = trait.Name;
		MechanicString.Value = trait.MechanicString;
		Icon.Value = trait.Icon;
		Tooltip.Value = new TooltipTemplateColonyTrait(trait.Name, trait.MechanicString, trait.Description, trait.EfficiencyModifier, trait.ContentmentModifier, trait.SecurityModifier);
	}

	public ColonyTraitVM(BlueprintColonyTrait trait, int index)
	{
		Name.Value = $"{trait.Name} #{index + 1}";
		MechanicString.Value = trait.MechanicString;
		Icon.Value = trait.Icon;
		Tooltip.Value = new TooltipTemplateColonyTrait($"{trait.Name} #{index + 1}", trait.MechanicString, trait.Description, trait.EfficiencyModifier, trait.ContentmentModifier, trait.SecurityModifier);
	}

	protected override void DisposeImplementation()
	{
	}
}
