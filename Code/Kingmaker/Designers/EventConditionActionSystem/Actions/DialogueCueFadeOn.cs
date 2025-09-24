using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0db0cab10eaf471a8dfe26e4f8e9edce")]
public class DialogueCueFadeOn : GameAction
{
	public override string GetDescription()
	{
		return "Вешает черный экран на диалог";
	}

	protected override void RunAction()
	{
		Game.Instance.RootUiContext.SurfaceVM.StaticPartVM.DialogContextVM.ToggleDialogFade(value: true);
	}

	public override string GetCaption()
	{
		return "Fade dialogue cue";
	}
}
