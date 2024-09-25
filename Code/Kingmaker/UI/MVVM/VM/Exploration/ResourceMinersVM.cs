using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ResourceMinersVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IExplorationUIHandler, ISubscriber, IItemsCollectionHandler
{
	public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>();

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<bool> HasColony;

	public ResourceMinersVM(ReactiveProperty<bool> hasColony)
	{
		HasColony = hasColony;
		AddDisposable(EventBus.Subscribe(this));
		Tooltip.Value = new TooltipTemplateSimple(UIStrings.Instance.ExplorationTexts.ResourceMiner.Text, UIStrings.Instance.ExplorationTexts.ResourceMinerDesc.Text);
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateMinersCount()
	{
		Count.Value = Game.Instance.ColonizationController.ResourceMinersCount();
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		UpdateMinersCount();
	}

	public void CloseExplorationScreen()
	{
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint.ItemType == ItemsItemType.ResourceMiner)
		{
			UpdateMinersCount();
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint.ItemType == ItemsItemType.ResourceMiner)
		{
			UpdateMinersCount();
		}
	}
}
