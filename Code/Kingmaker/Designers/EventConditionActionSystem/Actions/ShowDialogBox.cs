using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("09a180a958e44f641b9990d0f96aeed4")]
public class ShowDialogBox : GameAction
{
	public LocalizedString Text;

	public ParametrizedContextSetter Parameters;

	public ActionList OnAccept;

	public ActionList OnCancel;

	public override string GetCaption()
	{
		return "Show dialog box " + Text;
	}

	protected override void RunAction()
	{
		NamedParametersContext parameters = null;
		ParametrizedContextSetter.ParameterEntry[] array = (Parameters?.Parameters).EmptyIfNull();
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
		{
			parameters = parameters ?? new NamedParametersContext();
			parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
		}
		UIUtility.ShowMessageBox(Text, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			using (parameters?.RequestContextData())
			{
				switch (button)
				{
				case DialogMessageBoxBase.BoxButton.Yes:
					OnAccept.Run();
					break;
				case DialogMessageBoxBase.BoxButton.No:
					OnCancel.Run();
					break;
				case DialogMessageBoxBase.BoxButton.Close:
					OnCancel.Run();
					break;
				}
			}
		});
	}
}
