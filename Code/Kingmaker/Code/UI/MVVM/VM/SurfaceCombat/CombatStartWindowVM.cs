using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.SurfaceCombat;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;

public class CombatStartWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEntityPositionChangedHandler, ISubscriber<IEntity>, ISubscriber
{
	private Action m_StartBattle;

	public readonly bool CanDeploy;

	public BoolReactiveProperty CanStartCombat = new BoolReactiveProperty();

	public readonly CombatStartCoopProgressVM CoopProgressVM;

	public CombatStartWindowVM(Action startBattle, bool canDeploy)
	{
		AddDisposable(CoopProgressVM = new CombatStartCoopProgressVM());
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdateCanStartCombat();
		}));
		AddDisposable(EventBus.Subscribe(this));
		m_StartBattle = startBattle;
		CanDeploy = canDeploy;
		UpdateCanStartCombat();
	}

	protected override void DisposeImplementation()
	{
	}

	public void StartBattle()
	{
		m_StartBattle?.Invoke();
		CanStartCombat.Value = false;
	}

	private void UpdateCanStartCombat()
	{
		bool value = Game.Instance.TurnController.CanFinishDeploymentPhase();
		if (Game.Instance.TurnController.GetStartBattleProgress(out var _, out var _, out var playerGroup) && playerGroup.Contains(NetworkingManager.LocalNetPlayer))
		{
			value = false;
		}
		CanStartCombat.Value = value;
	}

	public void HandleEntityPositionChanged()
	{
		UpdateCanStartCombat();
	}
}
