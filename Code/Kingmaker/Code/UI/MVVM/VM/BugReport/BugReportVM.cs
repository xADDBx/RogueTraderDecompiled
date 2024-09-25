using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.UI.MVVM.VM.BugReport;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.BugReport;

public class BugReportVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly ReactiveProperty<BugReportDrawingVM> m_BugReportDrawingVM = new ReactiveProperty<BugReportDrawingVM>();

	private readonly ReactiveProperty<BugReportDuplicatesVM> m_BugReportDuplicatesVM = new ReactiveProperty<BugReportDuplicatesVM>();

	private readonly (string Description, List<(BugContext.AspectType? Aspect, string Assignee)> Assignees)[] m_AllContextData;

	public readonly OwlcatDropdownVM ContextDropdownVM;

	public readonly OwlcatDropdownVM AspectDropdownVM;

	private OwlcatDropdownVM m_AssigneeDropdownVM;

	private OwlcatDropdownVM m_FixVersionDropdownVM;

	private Dictionary<(int, int), int> m_ContextAspectToAssigneeMap = new Dictionary<(int, int), int>();

	public IReadOnlyReactiveProperty<BugReportDrawingVM> BugReportDrawingVM => m_BugReportDrawingVM;

	public IReadOnlyReactiveProperty<BugReportDuplicatesVM> BugReportDuplicatesVM => m_BugReportDuplicatesVM;

	public BugReportVM()
	{
		m_AllContextData = ReportingUtils.Instance.GetContextDescriptions();
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		(string, List<(BugContext.AspectType?, string)>)[] allContextData = m_AllContextData;
		for (int i = 0; i < allContextData.Length; i++)
		{
			(string, List<(BugContext.AspectType?, string)>) tuple = allContextData[i];
			list.Add(new DropdownItemVM(tuple.Item1));
		}
		AddDisposable(ContextDropdownVM = new OwlcatDropdownVM(list));
		List<DropdownItemVM> list2 = new List<DropdownItemVM>();
		string[] names = Enum.GetNames(typeof(BugContext.AspectType));
		foreach (string text in names)
		{
			list2.Add(new DropdownItemVM(text));
		}
		AddDisposable(AspectDropdownVM = new OwlcatDropdownVM(list2));
	}

	public OwlcatDropdownVM GetAssigneeDropDownVM(int contextIndex)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (m_AllContextData[contextIndex].Assignees != null)
		{
			foreach (var item2 in m_AllContextData[contextIndex].Assignees)
			{
				int item = (int)(item2.Aspect.HasValue ? item2.Aspect.Value : ((BugContext.AspectType)(-1)));
				dictionary.TryAdd(item2.Assignee, dictionary.Count);
				m_ContextAspectToAssigneeMap[(contextIndex, item)] = dictionary[item2.Assignee];
			}
		}
		return m_AssigneeDropdownVM = new OwlcatDropdownVM(dictionary.Select((KeyValuePair<string, int> x) => new DropdownItemVM(x.Key)).ToList());
	}

	public int GetAssigneeIndex(int contextIndex, int aspectIndex)
	{
		(int, int) key = (contextIndex, aspectIndex);
		if (!m_ContextAspectToAssigneeMap.TryGetValue(key, out var value))
		{
			return 0;
		}
		return value;
	}

	public OwlcatDropdownVM GetFixVersionDropDownVM()
	{
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		Dictionary<string, string> fixVersions = ReportingUtils.Instance.Assignees.Result.FixVersions;
		string[] fixVersions2 = ReportingUtils.FixVersions;
		foreach (string text in fixVersions2)
		{
			string value;
			string text2 = (fixVersions.TryGetValue(text, out value) ? ("[" + text + "] " + value) : text);
			list.Add(new DropdownItemVM(text2));
		}
		return m_FixVersionDropdownVM = new OwlcatDropdownVM(list);
	}

	public void ShowDrawing()
	{
		m_BugReportDrawingVM.Value = new BugReportDrawingVM(HideDrawing);
	}

	private void HideDrawing()
	{
		m_BugReportDrawingVM.Value?.Dispose();
	}

	public void ShowDuplicates()
	{
		m_BugReportDuplicatesVM.Value = new BugReportDuplicatesVM(HideDuplicates, ReportingUtils.Instance.CurrentContextName);
	}

	private void HideDuplicates()
	{
		m_BugReportDuplicatesVM.Value?.Dispose();
	}

	protected override void DisposeImplementation()
	{
		HideDrawing();
		HideDuplicates();
		m_AssigneeDropdownVM?.Dispose();
		m_FixVersionDropdownVM?.Dispose();
	}
}
