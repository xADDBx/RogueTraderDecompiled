using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class MinimalAdmissibleDamageReasonItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite IconSprite;

	public readonly string Text;

	public MinimalAdmissibleDamageReasonItemVM(Sprite iconSprite, string text)
	{
		IconSprite = iconSprite;
		Text = text;
	}

	protected override void DisposeImplementation()
	{
	}
}
