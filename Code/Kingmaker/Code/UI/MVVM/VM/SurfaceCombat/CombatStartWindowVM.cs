using System;
using Kingmaker.Blueprints.Root.Strings;
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
	private readonly Action m_StartBattle;

	public readonly bool CanDeploy;

	public readonly BoolReactiveProperty CanStartCombat = new BoolReactiveProperty();

	public readonly CombatStartCoopProgressVM CoopProgressVM;

	public readonly StringReactiveProperty CannotStartCombatReason = new StringReactiveProperty();

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
		bool flag = Game.Instance.TurnController.CanFinishDeploymentPhase();
		string value = ((!flag) ? UIStrings.Instance.TurnBasedTexts.CannotStartbattle.Text : string.Empty);
		if (Game.Instance.TurnController.GetStartBattleProgress(out var current, out var target, out var playerGroup) && playerGroup.Contains(NetworkingManager.LocalNetPlayer))
		{
			flag = false;
			value = string.Format(UIStrings.Instance.CommonTexts.WaitingOtherPlayer, current, target);
		}
		CannotStartCombatReason.Value = value;
		CanStartCombat.Value = flag;
	}

	public void HandleEntityPositionChanged()
	{
		UpdateCanStartCombat();
	}
}
