using System;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SystemMap;

public class SystemTitleVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber
{
	public readonly ReactiveProperty<bool> IsOnSystemMap = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsInSpaceCombat = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: true);

	public SystemTitleVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsOnSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
		IsInSpaceCombat.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
