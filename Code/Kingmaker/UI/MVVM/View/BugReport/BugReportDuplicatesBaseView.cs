using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cheats;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.BugReport;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.BugReport;

public abstract class BugReportDuplicatesBaseView : ViewBase<BugReportDuplicatesVM>
{
	[Header("Localizations")]
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[SerializeField]
	private TextMeshProUGUI m_LoadingProcessText;

	[Header("Widget")]
	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private BugDuplicateItemView m_WidgetEntityView;

	protected InputLayer InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private Coroutine m_DuplicatesListCoroutine;

	public const string InputLayerContextName = "BugReportDuplicatesViewInput";

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_LoadingProcessText.gameObject.SetActive(value: true);
		m_TitleText.text = UIStrings.Instance.UIBugReport.DuplicateBugsTitleText.Text;
		m_LoadingProcessText.text = UIStrings.Instance.UIBugReport.LoadingProcessDuplicatesListText.Text;
		BuildNavigation();
		m_DuplicatesListCoroutine = StartCoroutine(LoadDuplicatesListCoroutine());
	}

	protected override void DestroyViewImplementation()
	{
		if (m_DuplicatesListCoroutine != null)
		{
			StopCoroutine(m_DuplicatesListCoroutine);
			m_DuplicatesListCoroutine = null;
		}
		base.gameObject.SetActive(value: false);
		InputLayer = null;
		m_WidgetList.Clear();
	}

	private IEnumerator LoadDuplicatesListCoroutine()
	{
		Task<FindTicketsResponse> task = GetDuplicatesListAsync();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		List<BugDuplicateItemVM> list = new List<BugDuplicateItemVM>();
		Ticket[] tickets = task.Result.Tickets;
		foreach (Ticket ticket in tickets)
		{
			list.Add(new BugDuplicateItemVM(ticket));
		}
		if (list.Count == 0)
		{
			m_LoadingProcessText.text = UIStrings.Instance.UIBugReport.DuplicatesListIsEmptyText.Text;
		}
		else
		{
			m_LoadingProcessText.gameObject.SetActive(value: false);
			DrawEntities(list);
		}
		m_DuplicatesListCoroutine = null;
	}

	private async Task<FindTicketsResponse> GetDuplicatesListAsync()
	{
		try
		{
			return await new SirenClient().Ticket.FindTickets(new FindTicketsRequest
			{
				Context = base.ViewModel.Context,
				Area = Utilities.GetBlueprintName(CheatsJira.GetCurrentAreaPart()),
				Project = "WH"
			});
		}
		catch (Exception arg)
		{
			UberDebug.LogError($"Failed FindTicketsRequest {arg}");
			throw;
		}
	}

	private void DrawEntities(IEnumerable<BugDuplicateItemVM> duplicatesList)
	{
		AddDisposable(m_WidgetList.DrawEntries(duplicatesList, m_WidgetEntityView));
		UpdateNavigation();
	}

	private void BuildNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateInput();
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	protected virtual void CreateInput()
	{
		InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "BugReportDuplicatesViewInput"
		});
	}

	protected virtual void UpdateNavigation()
	{
		InputLayer.Unbind();
		List<IConsoleNavigationEntity> navigationEntities = m_WidgetList.GetNavigationEntities();
		m_NavigationBehaviour.AddColumn(navigationEntities);
		InputLayer.Bind();
	}
}
