using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class LootCollectorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string LootDisplayName;

	public readonly bool HasSkillCheck;

	private readonly LootVM m_Loot;

	public string SkillCheckText { get; }

	public LootVM Loot => m_Loot;

	private LootContextVM.LootWindowMode Mode => m_Loot.Mode;

	public ReactiveCollection<LootObjectVM> ContextLoot => m_Loot?.ContextLoot;

	public BoolReactiveProperty ExtendedView => m_Loot.ExtendedView;

	public BoolReactiveProperty NoLoot => m_Loot.NoLoot;

	public LootCollectorVM(LootVM lootVM)
	{
		m_Loot = lootVM;
		LootDisplayName = UIStrings.Instance.LootWindow.GetLootNameByContext(Mode);
		HasSkillCheck = lootVM.SkillCheckResult != null;
		SkillCheckText = UIUtilityTexts.GetLootSkillCheck(lootVM.SkillCheckResult);
	}

	protected override void DisposeImplementation()
	{
	}

	public void AddAllToCargoPart()
	{
		m_Loot.AddAllToCargoPart();
	}

	public void AddAllToInventoryPart()
	{
		m_Loot.AddAllToInventoryPart();
	}

	public void CollectAll()
	{
		if (Loot.Mode == LootContextVM.LootWindowMode.ZoneExit)
		{
			m_Loot.HandleOpenExitWindow(TryCollect);
		}
		else
		{
			TryCollect();
		}
	}

	private void TryCollect()
	{
		m_Loot.TryCollectLoot();
		Close();
	}

	public void Close()
	{
		m_Loot.Close();
		TooltipHelper.HideTooltip();
		m_Loot.BaseLeaveZone();
	}

	public void ChangeView()
	{
		m_Loot.ChangeView();
	}
}
