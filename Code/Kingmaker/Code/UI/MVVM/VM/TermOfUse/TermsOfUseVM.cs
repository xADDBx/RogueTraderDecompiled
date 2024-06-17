using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.TermOfUse;

public class TermsOfUseVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_CloseAction;

	public readonly UITermsOfUseTexts TermsOfUseTexts;

	public TermsOfUseVM(Action closeAction)
	{
		m_CloseAction = closeAction;
		TermsOfUseTexts = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TermsOfUseTexts;
	}

	protected override void DisposeImplementation()
	{
	}

	public void TermsOfUseAccept()
	{
		TermsOfUseClose();
	}

	public void TermsOfUseDecline()
	{
		string text = TermsOfUseTexts.AreYouReallyWantToDeclineAgreement;
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxBase.BoxType.Dialog, OnDeclineDialogAnswer);
		});
	}

	private void OnDeclineDialogAnswer(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			SystemUtil.ApplicationQuit();
		}
	}

	public void TermsOfUseClose()
	{
		m_CloseAction?.Invoke();
	}

	public LocalizedString GetLicenceText()
	{
		if (!Game.Instance.IsControllerMouse)
		{
			return TermsOfUseTexts.LicenceConsole;
		}
		return TermsOfUseTexts.Licence;
	}
}
