using System;
using Kingmaker.Code.UI.MVVM.VM.ChangeAppearance;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public class CharGenContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICharGenInitiateUIHandler, ISubscriber, IChangeAppearanceHandler
{
	public readonly ReactiveProperty<CharGenVM> CharGenVM = new ReactiveProperty<CharGenVM>();

	public readonly ReactiveProperty<ChangeAppearanceVM> ChangeAppearanceVM = new ReactiveProperty<ChangeAppearanceVM>();

	private Action m_EnterNewGameAction;

	private Action m_CloseWithoutCompleteAction;

	private Action m_CloseSoundAction;

	private Action m_ShowNewGameAction;

	private Action<BaseUnitEntity> m_CompleteAction;

	public CharGenContextVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleStartCharGen(CharGenConfig config, bool isCustomCompanionChargen)
	{
		DisposeAndRemove(CharGenVM);
		m_CloseWithoutCompleteAction = config.OnClose;
		m_CompleteAction = config.OnComplete;
		m_CloseSoundAction = config.OnCloseSoundAction;
		m_ShowNewGameAction = config.OnShowNewGameAction;
		if (config.Mode == CharGenConfig.CharGenMode.NewGame && !config.Unit.IsCustomCompanion() && !config.Unit.IsPet)
		{
			m_EnterNewGameAction = config.EnterNewGameAction;
			Game.Instance.Player.SetMainCharacter(config.Unit);
		}
		CharGenVM disposable = (CharGenVM.Value = new CharGenVM(config, CloseWithoutComplete, CompleteCharGen, isCustomCompanionChargen));
		AddDisposable(disposable);
	}

	private void CloseWithoutComplete()
	{
		m_ShowNewGameAction?.Invoke();
		CloseCharGen();
		m_CloseWithoutCompleteAction?.Invoke();
	}

	private void CompleteCharGen(BaseUnitEntity resultUnit)
	{
		m_CompleteAction?.Invoke(resultUnit);
		m_EnterNewGameAction?.Invoke();
		CloseCharGen();
	}

	private void CloseCharGen()
	{
		DisposeAndRemove(CharGenVM);
		m_CloseSoundAction?.Invoke();
	}

	private void CloseChangeAppearance()
	{
		DisposeAndRemove(ChangeAppearanceVM);
		m_CloseSoundAction?.Invoke();
	}

	private void CompleteChangeAppearance(BaseUnitEntity resultUnit)
	{
		m_CompleteAction?.Invoke(resultUnit);
		CloseChangeAppearance();
		EventBus.RaiseEvent(delegate(IChangeAppearanceCloseHandler h)
		{
			h.HandleCloseChangeAppearance();
		});
	}

	public void HandleShowChangeAppearance(CharGenConfig config)
	{
		DisposeAndRemove(ChangeAppearanceVM);
		m_CloseWithoutCompleteAction = config.OnClose;
		m_CompleteAction = config.OnComplete;
		m_CloseSoundAction = config.OnCloseSoundAction;
		m_ShowNewGameAction = config.OnShowNewGameAction;
		ChangeAppearanceVM disposable = (ChangeAppearanceVM.Value = new ChangeAppearanceVM(config, CloseChangeAppearance, CompleteChangeAppearance));
		AddDisposable(disposable);
	}
}
