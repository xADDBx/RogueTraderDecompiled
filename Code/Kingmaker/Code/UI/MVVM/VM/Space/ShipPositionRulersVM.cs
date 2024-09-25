using System;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class ShipPositionRulersVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber
{
	public readonly ReactiveProperty<bool> IsSystemMap = new ReactiveProperty<bool>();

	private GameModeType m_PreviousMode;

	public ShipPositionRulersVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsSystemMap.Value = gameMode == GameModeType.StarSystem || (gameMode == GameModeType.Dialog && m_PreviousMode == GameModeType.StarSystem);
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		m_PreviousMode = gameMode;
		IsSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.Dialog || (Game.Instance.CurrentMode == GameModeType.Dialog && m_PreviousMode == GameModeType.StarSystem);
	}
}
