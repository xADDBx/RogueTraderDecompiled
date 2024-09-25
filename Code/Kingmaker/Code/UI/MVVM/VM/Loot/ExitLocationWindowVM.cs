using System;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class ExitLocationWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_OnConfirm;

	private readonly Action m_OnDecline;

	public readonly string Header;

	public readonly string Description;

	public readonly string AdditionalInformation;

	public ExitLocationWindowVM(Action onConfirm, Action onDecline)
	{
		m_OnConfirm = onConfirm;
		m_OnDecline = onDecline;
		Header = UIStrings.Instance.ActionTexts.ExitArea;
		Description = UIStrings.Instance.LootWindow.ExitDescription;
		AdditionalInformation = UIStrings.Instance.LootWindow.CollectAllBeforeLeave;
	}

	public void Confirm()
	{
		m_OnConfirm?.Invoke();
		Decline();
	}

	public void Decline()
	{
		m_OnDecline?.Invoke();
	}

	protected override void DisposeImplementation()
	{
		m_OnDecline?.Invoke();
	}
}
