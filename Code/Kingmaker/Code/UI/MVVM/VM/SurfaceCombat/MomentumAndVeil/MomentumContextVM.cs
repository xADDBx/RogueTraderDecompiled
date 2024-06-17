using System;
using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;

public class MomentumContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnBasedModeResumeHandler
{
	public static MomentumContextVM Instance;

	public readonly ReactiveDictionary<MomentumGroup, MomentumEntityVM> Momentums = new ReactiveDictionary<MomentumGroup, MomentumEntityVM>();

	public readonly ReactiveProperty<bool> IsTurnBasedActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsPlayerTurn = new ReactiveProperty<bool>();

	public readonly ReactiveCommand MomentumGroupUpdated = new ReactiveCommand();

	public MomentumContextVM()
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		AddDisposable(EventBus.Subscribe(this));
		Instance = this;
		HandleTurnBasedModeSwitched(TurnController.IsInTurnBasedCombat());
	}

	protected override void DisposeImplementation()
	{
		Instance = null;
		Clear();
	}

	private void OnUpdateHandler()
	{
		if (!IsTurnBasedActive.Value || Game.Instance?.TurnController?.MomentumController?.Groups == null)
		{
			Clear();
		}
		else
		{
			UpdateMomentums();
		}
	}

	private void UpdateMomentums()
	{
		bool flag = false;
		List<MomentumGroup> list = new List<MomentumGroup>();
		foreach (var (item, _) in Momentums)
		{
			if (!Game.Instance.TurnController.MomentumController.Groups.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (MomentumGroup item2 in list)
		{
			Momentums[item2].Dispose();
			Momentums.Remove(item2);
			flag = true;
		}
		foreach (MomentumGroup group in Game.Instance.TurnController.MomentumController.Groups)
		{
			if (!Momentums.ContainsKey(group))
			{
				Momentums.Add(group, new MomentumEntityVM(group));
				flag = true;
			}
		}
		if (flag)
		{
			MomentumGroupUpdated.Execute();
		}
	}

	private void Clear()
	{
		Momentums.ForEach(delegate(KeyValuePair<MomentumGroup, MomentumEntityVM> m)
		{
			m.Value?.Dispose();
		});
		Momentums.Clear();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		IsTurnBasedActive.Value = isTurnBased;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		IsPlayerTurn.Value = Game.Instance.TurnController.IsPlayerTurn;
	}

	public MomentumEntityVM TryGetMomentumEntity(BaseUnitEntity unit)
	{
		MomentumGroup group = Game.Instance.TurnController.MomentumController.GetGroup(unit);
		if (group == null)
		{
			return null;
		}
		return Momentums.Get(group);
	}

	public void ForceUpdateContext()
	{
		if (Game.Instance?.TurnController?.MomentumController?.Groups == null)
		{
			Clear();
		}
		else
		{
			UpdateMomentums();
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		IsTurnBasedActive.Value = true;
	}
}
