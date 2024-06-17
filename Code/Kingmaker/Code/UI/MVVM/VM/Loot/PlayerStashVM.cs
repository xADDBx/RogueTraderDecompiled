using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class PlayerStashVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string LootDisplayName;

	private readonly LootVM m_Loot;

	public string SkillCheckText { get; }

	public LootContextVM.LootWindowMode Mode => m_Loot.Mode;

	public ReactiveCollection<LootObjectVM> ContextLoot => m_Loot?.ContextLoot;

	public PlayerStashVM(LootVM lootVM)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_Loot = lootVM;
		LootDisplayName = UIStrings.Instance.LootWindow.GetLootNameByContext(Mode);
		SkillCheckText = UIUtilityTexts.GetLootSkillCheck(lootVM.SkillCheckResult);
	}

	protected override void DisposeImplementation()
	{
	}

	public void Close()
	{
		m_Loot.Close();
	}

	public void ChangeView()
	{
		m_Loot.ChangeView();
	}
}
