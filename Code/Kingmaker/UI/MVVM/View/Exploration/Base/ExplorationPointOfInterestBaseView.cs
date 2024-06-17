using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationPointOfInterestBaseView : ViewBase<ExplorationPointOfInterestVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private Image m_UnavailableEffect;

	[SerializeField]
	private Image m_QuestMarkImage;

	[SerializeField]
	private Image m_QuestAreaImage;

	[SerializeField]
	private Image m_RumourMarkImage;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	protected Color m_HeaderColor = Color.white;

	[SerializeField]
	private Sprite[] m_PoiMarks;

	private Dictionary<Type, Sprite> m_PoiIcons;

	protected IDisposable Hint;

	private readonly List<Tweener> m_StartedTweeners = new List<Tweener>();

	private readonly Vector3 m_HoverScale = new Vector3(1.05f, 1.05f, 1.05f);

	public void Initialize()
	{
		m_CanvasGroup.alpha = 1f;
		m_PoiIcons = new Dictionary<Type, Sprite>
		{
			{
				typeof(BlueprintPointOfInterestBookEvent),
				m_PoiMarks[0]
			},
			{
				typeof(BlueprintPointOfInterestCargo),
				m_PoiMarks[1]
			},
			{
				typeof(BlueprintPointOfInterestColonyTrait),
				m_PoiMarks[2]
			},
			{
				typeof(BlueprintPointOfInterestExpedition),
				m_PoiMarks[3]
			},
			{
				typeof(BlueprintPointOfInterestGroundOperation),
				m_PoiMarks[4]
			},
			{
				typeof(BlueprintPointOfInterestLoot),
				m_PoiMarks[5]
			},
			{
				typeof(BlueprintPointOfInterestResources),
				m_PoiMarks[6]
			},
			{
				typeof(BlueprintPointOfInterestStatCheckLoot),
				m_PoiMarks[7]
			}
		};
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.PointOfInterestBlueprintType.Subscribe(delegate(BlueprintPointOfInterest value)
		{
			m_Icon.sprite = SetPointOfInterestIcon(value);
		}));
		AddDisposable(base.ViewModel.IsInteractable.Subscribe(SetInteractableState));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.IsExplored.Subscribe(SetExploredState));
		AddDisposable(base.ViewModel.IsRumour.Subscribe(delegate(bool val)
		{
			m_RumourMarkImage.enabled = val;
		}));
		AddDisposable(base.ViewModel.IsQuest.CombineLatest(base.ViewModel.QuestObjectiveName, (bool isQuest, string questObjectiveName) => new { isQuest, questObjectiveName }).Subscribe(value =>
		{
			SetQuestState(value.isQuest);
			if (value.isQuest && !string.IsNullOrWhiteSpace(value.questObjectiveName))
			{
				AddDisposable(m_QuestMarkImage.SetHint(base.ViewModel.QuestObjectiveName.Value));
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hint?.Dispose();
		Hint = null;
		m_StartedTweeners.ForEach(delegate(Tweener t)
		{
			t.Kill();
		});
		m_StartedTweeners.Clear();
	}

	public void SetFocus(bool value)
	{
		base.ViewModel?.SetFocus(value);
		SetFocusImpl(value);
	}

	protected virtual void SetFocusImpl(bool value)
	{
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

	private Sprite SetPointOfInterestIcon(BlueprintPointOfInterest poi)
	{
		Type type = poi.GetType();
		if (poi.Icon != null)
		{
			return poi.Icon;
		}
		if (!m_PoiIcons.ContainsKey(type))
		{
			return null;
		}
		return m_PoiIcons[type];
	}

	private void SetInteractableState(bool isInteractable)
	{
		m_UnavailableEffect.enabled = !isInteractable;
		if (!isInteractable)
		{
			SetHint(UIStrings.Instance.ExplorationTexts.ExploNotInteractable);
		}
	}

	private void SetExploredState(bool isExplored)
	{
		if (base.ViewModel.IsInteractable.Value)
		{
			if (!isExplored)
			{
				SetHint(UIStrings.Instance.ExplorationTexts.ExploNotExplored);
				m_CanvasGroup.alpha = 1f;
			}
			else
			{
				SetHint(UIStrings.Instance.ExplorationTexts.ExploAlreadyExplored);
				m_CanvasGroup.alpha = 0.5f;
			}
		}
	}

	private void SetHint(string stateText)
	{
		Hint?.Dispose();
		SetHintImpl(stateText);
	}

	protected virtual void SetHintImpl(string stateText)
	{
	}

	private void SetQuestState(bool isQuest)
	{
		m_QuestMarkImage.enabled = isQuest;
		m_QuestAreaImage.enabled = isQuest;
	}

	protected void AnimateHover(bool isHover)
	{
		Vector3 endValue = (isHover ? m_HoverScale : Vector3.one);
		m_StartedTweeners.Add(m_CanvasGroup.transform.DOScale(endValue, 0.1f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true));
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
