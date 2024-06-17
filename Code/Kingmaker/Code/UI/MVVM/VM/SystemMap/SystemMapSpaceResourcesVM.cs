using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SystemMap;

public class SystemMapSpaceResourcesVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, IColonizationResourcesHandler
{
	public readonly AutoDisposingList<ColonyResourceVM> ResourcesVMs = new AutoDisposingList<ColonyResourceVM>();

	public JournalOrderProfitFactorVM JournalOrderProfitFactorVM;

	public readonly ReactiveCommand RefreshData = new ReactiveCommand();

	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<bool> IsInSpaceCombat = new ReactiveProperty<bool>();

	public List<BlueprintResourceReference> BasicResources => BlueprintWarhammerRoot.Instance.ColonyRoot.BasicResources;

	private List<BlueprintResourceReference> AllResources => BlueprintWarhammerRoot.Instance.ColonyRoot.AllResources;

	private static bool IsSpaceArea
	{
		get
		{
			if (!(Game.Instance.CurrentMode == GameModeType.StarSystem))
			{
				return Game.Instance.CurrentMode == GameModeType.GlobalMap;
			}
			return true;
		}
	}

	public SystemMapSpaceResourcesVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(JournalOrderProfitFactorVM = new JournalOrderProfitFactorVM());
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	private void Clear()
	{
		ResourcesVMs.Clear();
	}

	protected void UpdateData()
	{
		Clear();
		AddResources(Game.Instance.ColonizationController.AllResourcesInPool());
	}

	private void AddResources(Dictionary<BlueprintResource, int> resources)
	{
		foreach (BlueprintResourceReference allResource in AllResources)
		{
			GetOrCreateResource(allResource);
		}
		foreach (KeyValuePair<BlueprintResource, int> resource in resources)
		{
			GetOrCreateResource(resource.Key).UpdateCount(resource.Value);
		}
		JournalOrderProfitFactorVM.UpdateCount(Game.Instance.Player.ProfitFactor.Total);
		AddResourcesImpl(resources);
		RefreshData.Execute();
	}

	public void SetAdditionalProfitFactor(int value)
	{
		JournalOrderProfitFactorVM.SetCountAdditional(value);
		UpdateData();
	}

	protected virtual void AddResourcesImpl(Dictionary<BlueprintResource, int> resources)
	{
	}

	private ColonyResourceVM GetOrCreateResource(BlueprintResource blueprintResource)
	{
		foreach (ColonyResourceVM resourcesVM in ResourcesVMs)
		{
			if (resourcesVM.BlueprintResource.Value == blueprintResource)
			{
				return resourcesVM;
			}
		}
		ColonyResourceVM colonyResourceVM = new ColonyResourceVM(blueprintResource, 0);
		AddDisposable(colonyResourceVM);
		ResourcesVMs.Add(colonyResourceVM);
		return colonyResourceVM;
	}

	protected override void DisposeImplementation()
	{
		ResourcesVMs.Clear();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsInSpaceCombat.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
		UpdateData();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		IsInSpaceCombat.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
		UpdateData();
	}

	public void HandleColonyResourcesUpdated(BlueprintResource resource, int count)
	{
		UpdateData();
	}

	public void HandleNotFromColonyResourcesUpdated(BlueprintResource resource, int count)
	{
		UpdateData();
	}
}
