using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipTorpedoVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IRoundStartHandler, ISubscriber
{
	public readonly ReactiveProperty<int> RoundsLeft = new ReactiveProperty<int>();

	public readonly UnitState UnitState;

	public OvertipTorpedoVM(UnitState unitState)
	{
		UnitState = unitState;
		UpdateProperties();
		AddDisposable(EventBus.Subscribe(this));
	}

	private void UpdateProperties()
	{
		Buff buff = UnitState.Unit.Buffs?.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedTorpedoesBuff);
		if (buff != null)
		{
			RoundsLeft.Value = buff.Rank;
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		UpdateProperties();
	}
}
