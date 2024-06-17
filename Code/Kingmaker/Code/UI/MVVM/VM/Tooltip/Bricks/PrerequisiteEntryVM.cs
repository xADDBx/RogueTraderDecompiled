using System;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class PrerequisiteEntryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Text;

	public readonly string Value;

	public readonly bool Done;

	public readonly bool Inverted;

	public readonly float FontMultiplier = FontSizeMultiplier;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public PrerequisiteEntryVM(string text, bool done, bool inverted, string value = null)
	{
		Text = text;
		Value = value ?? string.Empty;
		Done = done;
		Inverted = inverted;
	}

	protected override void DisposeImplementation()
	{
	}
}
