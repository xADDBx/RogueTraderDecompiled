using System;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorItemVM : SelectionGroupEntityVM
{
	public readonly ReactiveProperty<Texture2D> Texture = new ReactiveProperty<Texture2D>();

	public readonly int Number;

	private Action m_OnSelect;

	private Action m_OnUnselect;

	public TextureSelectorItemVM(Texture2D value, Action onSelect, int number, bool allowSwitchOff = false, Action onUnselect = null)
		: base(allowSwitchOff)
	{
		Texture.Value = value;
		m_OnSelect = onSelect;
		Number = number;
		if (onUnselect == null)
		{
			return;
		}
		AddDisposable(IsSelected.Subscribe(delegate(bool selected)
		{
			if (!selected)
			{
				onUnselect();
			}
		}));
	}

	public void UpdateTextureAndSetter(Texture2D value, Action setter)
	{
		Texture.Value = value;
		m_OnSelect = setter;
	}

	protected override void DoSelectMe()
	{
		m_OnSelect();
	}

	public void DoFocusMe()
	{
		DoSelectMe();
	}
}
