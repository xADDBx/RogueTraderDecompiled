using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class HintVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Text;

	public readonly string BindingText;

	public readonly Color Color;

	public HintVM(HintData data)
	{
		Text = data.Text;
		if (!data.BindingName.IsNullOrEmpty())
		{
			BindingText = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(data.BindingName));
		}
		Color = data.Color;
	}

	protected override void DisposeImplementation()
	{
	}
}
