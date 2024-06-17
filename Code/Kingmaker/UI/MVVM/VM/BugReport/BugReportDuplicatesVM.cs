using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.BugReport;

public class BugReportDuplicatesVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Context;

	private readonly Action m_CloseCallback;

	public BugReportDuplicatesVM(Action closeCallback, string context)
	{
		m_CloseCallback = closeCallback;
		Context = context;
	}

	protected override void DisposeImplementation()
	{
	}

	public void Close()
	{
		m_CloseCallback();
	}
}
