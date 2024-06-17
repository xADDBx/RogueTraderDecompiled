using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Colonization;

public class BuildingCirclePCView : ViewBase<BuildingCircleVM>
{
	[SerializeField]
	private OwlcatMultiButton m_CircleButton;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	private OwlcatButton m_NewButton;

	[SerializeField]
	private GameObject m_Timer;

	[SerializeField]
	private GameObject m_Locked;

	[SerializeField]
	private GameObject m_Blocked;

	public void SetData(BlueprintColonyProject project, Color32 color, Colony colony)
	{
		m_Background.color = color;
		m_NewButton.gameObject.SetActive(colony.ProjectCanStart(project));
		IEnumerable<ColonyProject> source = colony.Projects.Where((ColonyProject i) => i.Blueprint == project);
		m_Counter.transform.parent.gameObject.SetActive(source.Any());
		m_Counter.text = source.Count().ToString();
		if (source.Any())
		{
			m_Timer.SetActive(source.FirstOrDefault((ColonyProject proj) => !proj.IsFinished) != null);
		}
		if (project.Icon != null)
		{
			m_Icon.sprite = project.Icon;
		}
		else
		{
			PFLog.System.Log("Please assign building icon for building " + project.Name);
		}
		m_CircleButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			UpdateBuildingInfo(project, colony);
		});
		AddDisposable(m_CircleButton.SetHint(project.Name));
	}

	private void UpdateBuildingInfo(BlueprintColonyProject project, Colony colony)
	{
		EventBus.RaiseEvent(delegate(IColonizationUIHandler h)
		{
			h.UpdateBuildingScreen(project, colony);
		});
	}

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}
}
