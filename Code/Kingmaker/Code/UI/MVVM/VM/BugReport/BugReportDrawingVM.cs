using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.BugReport;

public class BugReportDrawingVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_CloseCallback;

	public BugReportDrawingVM(Action closeCallback)
	{
		m_CloseCallback = closeCallback;
	}

	public void Close()
	{
		m_CloseCallback();
	}

	protected override void DisposeImplementation()
	{
	}
}
