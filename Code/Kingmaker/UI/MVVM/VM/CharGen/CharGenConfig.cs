using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public class CharGenConfig
{
	public enum CharGenMode
	{
		NewGame,
		NewCompanion,
		Appearance
	}

	public enum CharGenCompanionType
	{
		Common,
		Navigator
	}

	public readonly BaseUnitEntity Unit;

	public readonly CharGenMode Mode;

	public readonly CharGenCompanionType CompanionType;

	private bool m_IsCustomCompanionChargen;

	public Action OnClose { get; private set; }

	public Action<BaseUnitEntity> OnComplete { get; private set; }

	public Action EnterNewGameAction { get; private set; }

	public Action OnCloseSoundAction { get; private set; }

	public Action OnShowNewGameAction { get; private set; }

	private bool IsUIForbidden => false;

	public static CharGenConfig Create(BaseUnitEntity unit, CharGenMode mode, CharGenCompanionType companionType = CharGenCompanionType.Common, bool isCustomCompanionChargen = false)
	{
		return new CharGenConfig(unit, mode, companionType, isCustomCompanionChargen);
	}

	private CharGenConfig(BaseUnitEntity unit, CharGenMode mode, CharGenCompanionType companionType, bool isCustomCompanionChargen)
	{
		Unit = unit;
		Mode = mode;
		CompanionType = companionType;
		m_IsCustomCompanionChargen = isCustomCompanionChargen;
	}

	public CharGenConfig SetOnClose(Action onClose)
	{
		OnClose = onClose;
		return this;
	}

	public CharGenConfig SetOnCloseSoundAction(Action onCloseSoundAction)
	{
		OnCloseSoundAction = onCloseSoundAction;
		return this;
	}

	public CharGenConfig SetOnShowNewGameAction(Action onShowNewGameAction)
	{
		OnShowNewGameAction = onShowNewGameAction;
		return this;
	}

	public CharGenConfig SetOnComplete(Action<BaseUnitEntity> onComplete)
	{
		OnComplete = onComplete;
		return this;
	}

	public CharGenConfig SetEnterNewGameAction(Action enterNewGameAction)
	{
		EnterNewGameAction = enterNewGameAction;
		return this;
	}

	public void OpenUI()
	{
		if (IsUIForbidden)
		{
			return;
		}
		if (Mode == CharGenMode.Appearance)
		{
			EventBus.RaiseEvent(delegate(IChangeAppearanceHandler h)
			{
				h.HandleShowChangeAppearance(this);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ICharGenInitiateUIHandler h)
			{
				h.HandleStartCharGen(this, m_IsCustomCompanionChargen);
			});
		}
	}
}
