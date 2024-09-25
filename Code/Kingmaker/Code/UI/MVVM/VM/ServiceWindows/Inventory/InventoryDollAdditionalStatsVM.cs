using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class InventoryDollAdditionalStatsVM : CharInfoComponentWithLevelUpVM, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>
{
	public readonly ReactiveProperty<string> ArmorDeflection = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> ArmorAbsorption = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> Dodge = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> DodgeReduction = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> Resolve = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> Parry = new ReactiveProperty<string>();

	public readonly ReactiveProperty<TooltipBaseTemplate> DeflectionTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<TooltipBaseTemplate> AbsorptionTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<TooltipBaseTemplate> DodgeTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private ArmorSlot ArmorSlot => Unit.Value?.Body.Armor;

	public InventoryDollAdditionalStatsVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager)
		: base(unit, levelUpManager)
	{
		AddDisposable(PreviewUnit.Subscribe(delegate
		{
			HandleUpdatePreviewUnit();
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	private void HandleUpdatePreviewUnit()
	{
		UpdateData();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateData();
	}

	private void UpdateData()
	{
		if (PreviewUnit.Value is UnitEntity unitEntity)
		{
			RuleCalculateStatsArmor statsRule = Rulebook.Trigger(new RuleCalculateStatsArmor(unitEntity));
			DeflectionTooltip.Value = new TooltipTemplateDeflection(statsRule);
			AbsorptionTooltip.Value = new TooltipTemplateAbsorption(statsRule);
			int resultDeflection = Rulebook.Trigger(new RuleCalculateStatsArmor(unitEntity)).ResultDeflection;
			ArmorDeflection.Value = resultDeflection.ToString();
			int resultAbsorption = Rulebook.Trigger(new RuleCalculateStatsArmor(unitEntity)).ResultAbsorption;
			ArmorAbsorption.Value = UIConfig.Instance.PercentHelper.AddPercentTo(resultAbsorption);
			RuleCalculateDodgeChance ruleCalculateDodgeChance = Rulebook.Trigger(new RuleCalculateDodgeChance(unitEntity));
			Dodge.Value = UIConfig.Instance.PercentHelper.AddPercentTo(ruleCalculateDodgeChance.UncappedResult);
			DodgeTooltip.Value = new TooltipTemplateDodge(ruleCalculateDodgeChance);
			RuleCalculateDodgePenetration ruleCalculateDodgePenetration = Rulebook.Trigger(new RuleCalculateDodgePenetration(unitEntity));
			DodgeReduction.Value = ruleCalculateDodgePenetration.ResultDodgePenetration.ToString();
			ModifiableValue statOptional = Unit.Value.GetStatOptional(StatType.Resolve);
			Resolve.Value = ((statOptional != null) ? $"{statOptional.ModifiedValue}" : "—");
			RuleCalculateParryChance ruleCalculateParryChance = Rulebook.Trigger(new RuleCalculateParryChance(unitEntity));
			Parry.Value = ((ruleCalculateParryChance != null) ? UIConfig.Instance.PercentHelper.AddPercentTo(ruleCalculateParryChance.Result) : "—");
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (!Unit.Value.IsDisposed && slot == ArmorSlot)
		{
			RefreshData();
		}
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		RefreshData();
	}

	public override void HandleUICommitChanges()
	{
		base.HandleUICommitChanges();
		UpdateData();
	}
}
