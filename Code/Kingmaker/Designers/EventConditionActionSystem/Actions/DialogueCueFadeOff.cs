using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("16396f9ab2dd4f7bb90f314049d0f795")]
public class DialogueCueFadeOff : GameAction
{
	public override string GetDescription()
	{
		return "Снимает черный экран с диалога";
	}

	protected override void RunAction()
	{
		Game.Instance.RootUiContext.SurfaceVM.StaticPartVM.DialogContextVM.ToggleDialogFade(value: false);
	}

	public override string GetCaption()
	{
		return "Fade dialogue cue off";
	}
}
