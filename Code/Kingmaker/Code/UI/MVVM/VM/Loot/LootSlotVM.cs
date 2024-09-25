using System;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class LootSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite Icon;

	public readonly int Count;

	private readonly LootEntry m_LootEntity;

	public LootSlotVM(LootEntry lootEntry)
	{
		m_LootEntity = lootEntry;
		Icon = lootEntry.Item.Icon ?? UIConfig.Instance.UIIcons.DefaultItemIcon;
		Count = lootEntry.Count;
	}

	protected override void DisposeImplementation()
	{
	}

	public TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateLootEntity(m_LootEntity);
	}
}
