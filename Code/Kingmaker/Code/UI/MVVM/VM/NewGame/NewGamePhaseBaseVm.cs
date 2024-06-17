using System;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame;

public abstract class NewGamePhaseBaseVm : VMBase
{
	private Action m_NextStep;

	private Action m_BackStep;

	public BoolReactiveProperty IsEnabled { get; } = new BoolReactiveProperty();


	public virtual BoolReactiveProperty IsNextButtonAvailable { get; } = new BoolReactiveProperty();


	protected NewGamePhaseBaseVm(Action backStep, Action nextStep)
	{
		m_NextStep = nextStep;
		m_BackStep = backStep;
		IsEnabled.Value = false;
	}

	public virtual void OnNext()
	{
		ApplySettings();
		m_NextStep?.Invoke();
	}

	public virtual void OnBack()
	{
		m_BackStep?.Invoke();
	}

	protected override void DisposeImplementation()
	{
		m_NextStep = null;
		m_BackStep = null;
	}

	public virtual bool SetEnabled(bool value, bool? direction = null)
	{
		IsEnabled.Value = value;
		return true;
	}

	private void ApplySettings()
	{
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
	}
}
