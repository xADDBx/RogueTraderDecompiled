using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.EtudeCounter;

public class EtudeCounterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEtudeCounterHandler, ISubscriber, IEtudeCounterSystemHandler
{
	public readonly StringReactiveProperty Label = new StringReactiveProperty();

	public readonly BoolReactiveProperty ShowSubLabel = new BoolReactiveProperty();

	public readonly StringReactiveProperty SubLabel = new StringReactiveProperty();

	public readonly StringReactiveProperty Counter = new StringReactiveProperty();

	public readonly FloatReactiveProperty Progress = new FloatReactiveProperty(0f);

	public readonly BoolReactiveProperty ShowProgress = new BoolReactiveProperty();

	public readonly BoolReactiveProperty ShowCounter = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsShowing = new BoolReactiveProperty();

	public readonly ReactiveCommand CounterChanged = new ReactiveCommand();

	public readonly ReactiveCommand ProgressChanged = new ReactiveCommand();

	public readonly BoolReactiveProperty IsSystemFailEnabled = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsSystemSuccessEnabled = new BoolReactiveProperty();

	public BoolReactiveProperty IsExtraTextShowed = new BoolReactiveProperty();

	public ReactiveCommand<bool> ShowExtraText = new ReactiveCommand<bool>();

	private readonly Dictionary<string, EtudeShowCounterUIStruct> m_Configs = new Dictionary<string, EtudeShowCounterUIStruct>();

	public EtudeCounterVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		IsExtraTextShowed.Value = true;
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(UpdateValues));
	}

	protected override void DisposeImplementation()
	{
	}

	void IEtudeCounterHandler.ShowEtudeCounter(EtudeShowCounterUIStruct counterUIStruct)
	{
		m_Configs.Add(counterUIStruct.Id, counterUIStruct);
	}

	void IEtudeCounterHandler.HideEtudeCounter(string id)
	{
		m_Configs.Remove(id);
	}

	public void ToggleExtraText()
	{
		if (IsExtraTextShowed.Value)
		{
			IsExtraTextShowed.Value = false;
			ShowExtraText.Execute(parameter: false);
		}
		else
		{
			IsExtraTextShowed.Value = true;
			ShowExtraText.Execute(parameter: true);
		}
	}

	private void UpdateValues()
	{
		if (m_Configs.Empty() || Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.TurnController.IsPreparationTurn)
		{
			IsShowing.Value = false;
			return;
		}
		EtudeShowCounterUIStruct etudeShowCounterUIStruct = m_Configs.Values.First();
		Label.Value = etudeShowCounterUIStruct.Label;
		ShowSubLabel.Value = etudeShowCounterUIStruct.ShowSubLabel;
		SubLabel.Value = etudeShowCounterUIStruct.SubLabel;
		if (etudeShowCounterUIStruct.Type.HasFlag(EtudeUICounterTypes.Slider) || etudeShowCounterUIStruct.Type.HasFlag(EtudeUICounterTypes.Label))
		{
			int num = etudeShowCounterUIStruct.ValueGetter?.Invoke() ?? 0;
			int num2 = etudeShowCounterUIStruct.TargetValueGetter?.Invoke() ?? 0;
			ShowCounter.Value = etudeShowCounterUIStruct.Type.HasFlag(EtudeUICounterTypes.Label);
			ShowProgress.Value = etudeShowCounterUIStruct.Type.HasFlag(EtudeUICounterTypes.Slider) && (float)num2 > 0f;
			if (ShowCounter.Value)
			{
				Counter.Value = ((num2 > 0) ? $"{num}/{num2}" : $"{num}");
				CounterChanged.Execute();
			}
			else if (ShowProgress.Value)
			{
				Progress.Value = (float)num / (float)num2;
				ProgressChanged.Execute();
			}
		}
		else
		{
			ShowProgress.Value = false;
			ShowCounter.Value = false;
		}
		IsShowing.Value = true;
	}

	public void ShowEtudeCounterSystem(EtudeUICounterSystemTypes type)
	{
		switch (type)
		{
		case EtudeUICounterSystemTypes.Fail:
			IsSystemFailEnabled.Value = true;
			break;
		case EtudeUICounterSystemTypes.Success:
			IsSystemSuccessEnabled.Value = true;
			break;
		}
	}

	public void HideEtudeCounterSystem(EtudeUICounterSystemTypes type)
	{
		switch (type)
		{
		case EtudeUICounterSystemTypes.Fail:
			IsSystemFailEnabled.Value = false;
			break;
		case EtudeUICounterSystemTypes.Success:
			IsSystemSuccessEnabled.Value = false;
			break;
		}
	}
}
