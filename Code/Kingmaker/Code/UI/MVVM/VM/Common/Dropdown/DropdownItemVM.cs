using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;

public class DropdownItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Text;

	public readonly Sprite Icon;

	public DropdownItemVM(string text, Sprite icon = null)
	{
		Text = text;
		Icon = icon;
	}

	protected override void DisposeImplementation()
	{
	}
}
