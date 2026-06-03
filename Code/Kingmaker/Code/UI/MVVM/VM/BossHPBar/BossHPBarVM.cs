using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.BossHPBar;

public class BossHPBarVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBossHPBarHandler, ISubscriber
{
	public readonly StringReactiveProperty BossName = new StringReactiveProperty();

	public readonly StringReactiveProperty HPLabel = new StringReactiveProperty();

	public readonly FloatReactiveProperty Progress = new FloatReactiveProperty(0f);

	public readonly BoolReactiveProperty IsShowing = new BoolReactiveProperty();

	private readonly Dictionary<string, ShowBossHPBarUIStruct> m_Configs = new Dictionary<string, ShowBossHPBarUIStruct>();

	public BossHPBarVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(UpdateValues));
	}

	protected override void DisposeImplementation()
	{
	}

	void IBossHPBarHandler.ShowBossHPBar(ShowBossHPBarUIStruct bossHPBarUIStruct)
	{
		m_Configs[bossHPBarUIStruct.Id] = bossHPBarUIStruct;
	}

	void IBossHPBarHandler.HideBossHPBar(string id)
	{
		m_Configs.Remove(id);
	}

	private void UpdateValues()
	{
		if (m_Configs.Empty() || Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.TurnController.IsPreparationTurn || !Game.Instance.TurnController.InCombat)
		{
			IsShowing.Value = false;
			return;
		}
		ShowBossHPBarUIStruct showBossHPBarUIStruct = m_Configs.Values.First();
		BossName.Value = showBossHPBarUIStruct.BossName;
		int num = showBossHPBarUIStruct.CurrentHPGetter?.Invoke() ?? 0;
		int num2 = showBossHPBarUIStruct.MaxHPGetter?.Invoke() ?? 0;
		Progress.Value = ((num2 > 0) ? ((float)num / (float)num2) : 0f);
		HPLabel.Value = ((num2 > 0) ? $"{num}/{num2}" : $"{num}");
		IsShowing.Value = true;
	}
}
