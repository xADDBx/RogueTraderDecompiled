using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;

public class CharInfoHitPointsVM : CharInfoComponentVM
{
	public readonly ReactiveProperty<string> HpText = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<float> CurrentHpRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> TempHpRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> MaxHpRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public CharInfoHitPointsVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateValues();
		}));
		UpdateTooltip();
		UpdateValues();
	}

	protected override void DisposeImplementation()
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateTooltip();
		UpdateValues(force: true);
	}

	private void UpdateTooltip()
	{
		if (Unit.Value != null)
		{
			ModifiableValue modifiableValue = UnitUIWrapper.Stats?.GetStat(StatType.HitPoints);
			if (modifiableValue != null)
			{
				Tooltip.Value = new TooltipTemplateStat(new StatTooltipData(modifiableValue));
			}
		}
	}

	protected virtual void UpdateValues(bool force = false)
	{
		if (Unit.Value == null || Unit.Value.IsDisposed)
		{
			return;
		}
		PartHealth health = UnitUIWrapper.Health;
		PartLifeState lifeState = UnitUIWrapper.LifeState;
		if (health == null || lifeState == null)
		{
			return;
		}
		if (Unit.Value.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			CurrentHpRatio.Value = 1f;
			TempHpRatio.Value = 1f;
			MaxHpRatio.Value = 1f;
			HpText.Value = "???";
			return;
		}
		int temporaryHitPoints = health.TemporaryHitPoints;
		int num = health.MaxHitPoints + temporaryHitPoints;
		int hitPointsLeft = health.HitPointsLeft;
		float num2 = (float)hitPointsLeft / (float)health.MaxHitPoints;
		float num3 = (float)(hitPointsLeft + temporaryHitPoints) / (float)num;
		float num4 = (float)health.MaxHitPoints / (float)num;
		if (force || num2 != CurrentHpRatio.Value || num3 != TempHpRatio.Value || num4 != TempHpRatio.Value)
		{
			CurrentHpRatio.Value = num2;
			TempHpRatio.Value = num3;
			MaxHpRatio.Value = num4;
			HpText.Value = UIUtility.GetHpText(UnitUIWrapper, lifeState.IsDead);
		}
	}
}
