using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsBuiltListElemBaseView : ViewBase<ColonyProjectVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[Serializable]
	private struct ColonyProjectsBuiltListElemVisual
	{
		public CanvasGroupWithAlpha[] Elements;

		public void Apply()
		{
			CanvasGroupWithAlpha[] elements = Elements;
			foreach (CanvasGroupWithAlpha canvasGroupWithAlpha in elements)
			{
				canvasGroupWithAlpha.Apply();
			}
		}
	}

	[Serializable]
	private struct CanvasGroupWithAlpha
	{
		public CanvasGroup CanvasGroup;

		public float Alpha;

		public void Apply()
		{
			CanvasGroup.alpha = Alpha;
		}
	}

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Slider m_ProgressBar;

	[SerializeField]
	private OwlcatButton m_Button;

	[SerializeField]
	private Color m_HeaderColor = Color.white;

	[SerializeField]
	private ColonyProjectsBuiltListElemVisual m_BindedVisual;

	[SerializeField]
	private ColonyProjectsBuiltListElemVisual m_UnbindedVisual;

	private IDisposable m_Hint;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(base.ViewModel.SegmentsToBuild.Subscribe(delegate(int val)
		{
			m_ProgressBar.maxValue = val;
		}));
		AddDisposable(base.ViewModel.Progress.Subscribe(delegate(int val)
		{
			m_ProgressBar.value = val;
		}));
		AddDisposable(base.ViewModel.IsBuilding.Subscribe(SetBuildingState));
	}

	protected override void DestroyViewImplementation()
	{
		m_Hint?.Dispose();
		m_Hint = null;
	}

	public void SetBindedVisual()
	{
		m_BindedVisual.Apply();
	}

	public void SetUnbindedVisual()
	{
		m_UnbindedVisual.Apply();
	}

	private void SetBuildingState(bool isBuilding)
	{
		m_ProgressBar.gameObject.SetActive(isBuilding);
		string hint = (isBuilding ? UIStrings.Instance.ColonyProjectsTexts.ProjectIsBuilding : UIStrings.Instance.ColonyProjectsTexts.ProjectIsFinished);
		SetHint(hint);
	}

	private void SetHint(string stateText)
	{
		m_Hint?.Dispose();
		string text = $"{stateText}\n<b><color=#{ColorUtility.ToHtmlStringRGB(m_HeaderColor)}>{base.ViewModel.Title}";
		m_Hint = m_Button.SetHint(text);
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public bool CanInteract()
	{
		return false;
	}

	public bool CanShowTooltip()
	{
		return false;
	}

	public void Interact()
	{
	}

	public void ShowTooltip()
	{
	}
}
