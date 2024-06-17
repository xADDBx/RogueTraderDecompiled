using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationResourcePointBaseView : ViewBase<ExplorationResourceVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_ActiveImage;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	private readonly List<Tweener> m_StartedTweeners = new List<Tweener>();

	private readonly Vector3 m_HoverScale = new Vector3(1.05f, 1.05f, 1.05f);

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(base.ViewModel.IsBeingMined.Subscribe(m_ActiveImage.gameObject.SetActive));
	}

	protected override void DestroyViewImplementation()
	{
		m_StartedTweeners.ForEach(delegate(Tweener t)
		{
			t.Kill();
		});
		m_StartedTweeners.Clear();
	}

	protected void AnimateHover(bool isHover)
	{
		Vector3 endValue = (isHover ? m_HoverScale : Vector3.one);
		m_StartedTweeners.Add(m_CanvasGroup.transform.DOScale(endValue, 0.1f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
	}

	public void SetFocus(bool value)
	{
		base.ViewModel.SetFocus(value);
		SetFocusImpl(value);
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

	protected virtual void SetFocusImpl(bool value)
	{
	}

	protected void HandleClick()
	{
		base.ViewModel.Interact();
	}

	public bool CanInteract()
	{
		return true;
	}

	public bool CanShowTooltip()
	{
		return false;
	}

	public void Interact()
	{
		base.ViewModel.Interact();
	}

	public void ShowTooltip()
	{
	}
}
