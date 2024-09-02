using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Traits;

public class ColonyTraitsVM : ColonyUIComponentVM
{
	public readonly AutoDisposingReactiveCollection<ColonyTraitVM> TraitsVMs = new AutoDisposingReactiveCollection<ColonyTraitVM>();

	public readonly ReactiveCommand UpdateTraits = new ReactiveCommand();

	protected override void SetColonyImpl(Colony colony)
	{
		if (colony == null)
		{
			TraitsVMs.Clear();
			return;
		}
		int num = 0;
		foreach (var (blueprintColonyTrait2, _) in colony.ColonyTraits)
		{
			if (blueprintColonyTrait2.IsHistorical)
			{
				AddTraitVM(blueprintColonyTrait2, num);
				num++;
			}
			else
			{
				AddTraitVM(blueprintColonyTrait2);
			}
		}
		UpdateTraits.Execute();
	}

	private void AddTraitVM(BlueprintColonyTrait trait)
	{
		ColonyTraitVM colonyTraitVM = new ColonyTraitVM(trait);
		AddDisposable(colonyTraitVM);
		TraitsVMs.Add(colonyTraitVM);
	}

	private void AddTraitVM(BlueprintColonyTrait trait, int index)
	{
		ColonyTraitVM colonyTraitVM = new ColonyTraitVM(trait, index);
		AddDisposable(colonyTraitVM);
		TraitsVMs.Add(colonyTraitVM);
	}
}
