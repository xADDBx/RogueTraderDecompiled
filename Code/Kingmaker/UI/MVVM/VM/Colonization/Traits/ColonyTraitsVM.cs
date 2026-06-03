using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Traits;

public class ColonyTraitsVM : ColonyUIComponentVM, IColonizationTraitHandler, ISubscriber
{
	public readonly AutoDisposingReactiveCollection<ColonyTraitVM> TraitsVMs = new AutoDisposingReactiveCollection<ColonyTraitVM>();

	public readonly ReactiveCommand UpdateTraits = new ReactiveCommand();

	public ColonyTraitsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void SetColonyImpl(Colony colony)
	{
		RebuildTraits();
	}

	public void HandleTraitStarted(Colony colony, BlueprintColonyTrait trait)
	{
		if (colony == m_Colony)
		{
			RebuildTraits();
		}
	}

	public void HandleTraitEnded(Colony colony, BlueprintColonyTrait trait)
	{
		if (colony == m_Colony)
		{
			RebuildTraits();
		}
	}

	private void RebuildTraits()
	{
		TraitsVMs.Clear();
		if (m_Colony == null)
		{
			return;
		}
		int num = 0;
		foreach (var (blueprintColonyTrait2, _) in m_Colony.ColonyTraits)
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
