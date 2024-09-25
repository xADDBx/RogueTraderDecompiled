using System;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;

public class EncyclopediaPageBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	protected readonly IBlock m_Block;

	public readonly float FontMultiplier = FontSizeMultiplier;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public EncyclopediaPageBlockVM(IBlock block)
	{
		m_Block = block;
	}

	protected override void DisposeImplementation()
	{
	}
}
