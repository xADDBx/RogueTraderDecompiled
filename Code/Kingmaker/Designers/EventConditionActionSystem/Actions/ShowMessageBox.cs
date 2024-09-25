using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Common;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("603b5218c76141dd8dcae6e3f4a52162")]
public class ShowMessageBox : GameAction
{
	public LocalizedString Text;

	public ActionList OnClose;

	public int WaitTime;

	public override string GetCaption()
	{
		return "Show message box";
	}

	protected override void RunAction()
	{
		UIUtility.ShowMessageBox(Text, DialogMessageBoxBase.BoxType.Message, delegate
		{
			OnClose.Run();
		}, null, null, null, WaitTime);
	}
}
