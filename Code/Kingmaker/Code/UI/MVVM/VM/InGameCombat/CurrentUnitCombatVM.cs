using System;
using Kingmaker.Settings;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.InGameCombat;

public class CurrentUnitCombatVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public CurrentUnitCombatVM()
	{
		AddDisposable(Game.Instance.Keyboard.Bind(SettingsRoot.Controls.Keybindings.General.HighlightObjects.Key + UIConsts.SuffixOn, HighlightOn));
		AddDisposable(Game.Instance.Keyboard.Bind(SettingsRoot.Controls.Keybindings.General.HighlightObjects.Key + UIConsts.SuffixOff, HighlightOff));
	}

	protected override void DisposeImplementation()
	{
	}

	private void HighlightOn()
	{
		CombatHUDRenderer.Instance.ForceDrawThreatArea = true;
	}

	private void HighlightOff()
	{
		CombatHUDRenderer.Instance.ForceDrawThreatArea = false;
	}
}
