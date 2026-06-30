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
		foreach (var (trait, _) in m_Colony.ColonyTraits)
		{
			AddTraitVM(trait);
		}
		UpdateTraits.Execute();
	}

	private void AddTraitVM(BlueprintColonyTrait trait)
	{
		ColonyTraitVM colonyTraitVM = new ColonyTraitVM(trait);
		AddDisposable(colonyTraitVM);
		TraitsVMs.Add(colonyTraitVM);
	}
}
