using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

public class TooltipsDataCache : BaseDisposable
{
	private readonly Dictionary<ItemEntity, ItemTooltipData> m_ItemTooltipDataCache = new Dictionary<ItemEntity, ItemTooltipData>();

	private readonly Dictionary<BlueprintItem, ItemTooltipData> m_BlueprintItemTooltipDataCache = new Dictionary<BlueprintItem, ItemTooltipData>();

	public static TooltipsDataCache Instance => Game.Instance.RootUiContext.CommonVM.TooltipsDataCache;

	public TooltipsDataCache()
	{
		AddDisposable(Game.Instance.SelectionCharacter.SelectedUnitInUI.Subscribe(delegate
		{
			Clear();
		}));
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	public void Clear()
	{
		m_ItemTooltipDataCache.Clear();
		m_BlueprintItemTooltipDataCache.Clear();
	}

	public ItemTooltipData GetItemTooltipData(ItemEntity item, bool forceUpdate = false, bool replenishing = false)
	{
		if (!m_ItemTooltipDataCache.TryGetValue(item, out var value) || forceUpdate)
		{
			value = UIUtilityItem.GetItemTooltipData(item, replenishing);
			m_ItemTooltipDataCache[item] = value;
		}
		return value;
	}

	public ItemTooltipData GetItemTooltipData(BlueprintItem blueprintItem, bool forceUpdate = false)
	{
		if (!m_BlueprintItemTooltipDataCache.TryGetValue(blueprintItem, out var value) || forceUpdate)
		{
			value = UIUtilityItem.GetItemTooltipData(blueprintItem);
			m_BlueprintItemTooltipDataCache[blueprintItem] = value;
		}
		return value;
	}
}
