using System;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;

public class CharacterVisualSettingsEntityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly BoolReactiveProperty IsOn = new BoolReactiveProperty();

	private readonly Action m_ChangeValue;

	public readonly BoolReactiveProperty Locked = new BoolReactiveProperty();

	public CharacterVisualSettingsEntityVM(bool value, Action changeValue)
	{
		IsOn.Value = value;
		m_ChangeValue = changeValue;
	}

	public void Switch()
	{
		UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		m_ChangeValue?.Invoke();
		IsOn.Value = !IsOn.Value;
	}

	public void SetValue(bool value)
	{
		if (value != IsOn.Value)
		{
			Switch();
		}
	}

	public void SetLock(bool @lock = true)
	{
		Locked.Value = @lock;
	}

	protected override void DisposeImplementation()
	{
	}
}
