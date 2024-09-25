using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;
using Kingmaker.EntitySystem.Entities;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class UnitHealthPartVM : CharInfoHitPointsVM
{
	public readonly ReactiveProperty<bool> IsDead = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsEnemy = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsPlayer = new ReactiveProperty<bool>(initialValue: false);

	public UnitHealthPartVM(IReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	public UnitHealthPartVM(BaseUnitEntity unit)
		: this(new ReactiveProperty<BaseUnitEntity>(unit))
	{
	}

	protected override void UpdateValues(bool force = false)
	{
		base.UpdateValues(force);
		BaseUnitEntity value = Unit.Value;
		bool flag = value != null && !value.IsDisposed;
		IsDead.Value = flag && UnitUIWrapper.IsFinallyDead;
		IsPlayer.Value = flag && UnitUIWrapper.IsPlayerFaction;
		IsEnemy.Value = flag && UnitUIWrapper.IsPlayerEnemy;
	}
}
