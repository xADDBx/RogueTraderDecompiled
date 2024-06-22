using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.NavigatorResource;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.NavigatorResource.Base;

public class SectorMapBottomHudBaseView : ViewBase<SectorMapBottomHudVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ResourceCurrentValueText;

	[SerializeField]
	private Color m_NormalTextColor;

	[SerializeField]
	private Color m_PositiveChangeTextColor;

	[SerializeField]
	private Color m_NegativeChangeTextColor;

	[Header("NeedScanEffect")]
	[SerializeField]
	private RectTransform m_NeedScanCircle;

	[SerializeField]
	private Image m_NeedScanHoverLights;

	[SerializeField]
	private TextMeshProUGUI m_NeedScanLabel;

	[Header("WillChangeNavigatorResource")]
	[SerializeField]
	private TextMeshProUGUI m_WillChangeNavigatorResourceCount;

	[SerializeField]
	protected OwlcatMultiButton m_ScanButton;

	[Header("NavigatorResourceChangedAnimator")]
	[SerializeField]
	private TextMeshProUGUI m_PlusNavigatorResourceObject;

	[SerializeField]
	private FadeAnimator m_PlusNavigatorResourceAnimator;

	private Tweener m_NeedScanTween;

	private Tween m_NoMoneyTween;

	private int m_OldCurrentValue;

	protected string CenterOnShipHintText;

	protected override void BindViewImplementation()
	{
		m_OldCurrentValue = base.ViewModel.CurrentValue.Value;
		m_ResourceCurrentValueText.text = base.ViewModel.CurrentValue.Value.ToString();
		m_NeedScanLabel.text = UIStrings.Instance.GlobalMap.ScanForRoutes;
		string text = Game.Instance?.Player?.PlayerShip?.CharacterName;
		CenterOnShipHintText = ((!string.IsNullOrWhiteSpace(text)) ? string.Format(UIStrings.Instance.GlobalMap.SetCameraOnVoidshipWithShipName, text) : ((string)UIStrings.Instance.GlobalMap.SetCameraOnVoidship));
		AddDisposable(base.ViewModel.IsScanAvailable.Subscribe(NeedScanEffect));
		AddDisposable(base.ViewModel.IsWillChangeNavigatorResource.CombineLatest(base.ViewModel.WillChangeNavigatorResourceCount, (bool isWillChange, int navigatorResourceCost) => new { isWillChange, navigatorResourceCost }).Skip(1).Subscribe(value =>
		{
			SetScanButtonActiveLayer(value.isWillChange ? "ChangeResourceEffect" : "Default");
			if (value.isWillChange)
			{
				m_WillChangeNavigatorResourceCount.text = $"-{value.navigatorResourceCost}";
			}
		}));
		AddDisposable(base.ViewModel.CurrentValue.Subscribe(delegate
		{
			CurrentValueReaction();
		}));
		AddDisposable(base.ViewModel.NoMoney.Skip(1).Subscribe(delegate
		{
			NoMoneyReaction(base.ViewModel.NeedMoneyCount);
		}));
		AddDisposable(m_ResourceCurrentValueText.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.SpaceCombatTexts.NavigatorResource, UIStrings.Instance.SpaceCombatTexts.NavigatorResourceDescription)));
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.SpaceExploration.KoronusRouteButtonUnHover.Play();
	}

	protected virtual void OnUpdateHandler()
	{
	}

	protected void NeedScanHoverEffect(bool state)
	{
		if (!(!base.ViewModel.IsScanAvailable.Value && state))
		{
			float num = (state ? 1.44f : 1f);
			m_NeedScanCircle.DOScale(new Vector2(num, num), 0.3f);
			m_NeedScanHoverLights.DOFade(state ? 1f : 0f, 0.3f);
			if (state)
			{
				UISounds.Instance.Sounds.SpaceExploration.KoronusRouteButtonHover.Play();
			}
			else
			{
				UISounds.Instance.Sounds.SpaceExploration.KoronusRouteButtonUnHover.Play();
			}
		}
	}

	protected bool IsPointOnScreen(Vector3 point)
	{
		return UIUtilityGetRect.CheckObjectInRect(point, 35f, 35f);
	}

	private void CurrentValueReaction()
	{
		if (m_OldCurrentValue != base.ViewModel.CurrentValue.Value)
		{
			m_ResourceCurrentValueText.text = base.ViewModel.CurrentValue.Value.ToString();
			Color endValue = ((m_OldCurrentValue > base.ViewModel.CurrentValue.Value) ? m_NegativeChangeTextColor : m_PositiveChangeTextColor);
			m_OldCurrentValue = base.ViewModel.CurrentValue.Value;
			Sequence s = DOTween.Sequence();
			s.Append(m_ResourceCurrentValueText.DOColor(endValue, 1f));
			s.Append(m_ResourceCurrentValueText.DOColor(m_NormalTextColor, 1f));
			HandleCostChanged();
		}
	}

	private void NoMoneyReaction(int needMoneyCount)
	{
		m_NoMoneyTween?.Kill();
		SetScanButtonActiveLayer("NoResourceEffect");
		m_WillChangeNavigatorResourceCount.text = $"-{needMoneyCount}";
		m_NoMoneyTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 1f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			SetScanButtonActiveLayer("Default");
			m_NoMoneyTween = null;
		});
	}

	protected virtual void NeedScanEffect(bool state)
	{
		SetScanButtonActiveLayer(state ? "NeedScanWheel" : "Default");
	}

	public void SetScanButtonActiveLayer(string layer)
	{
		m_ScanButton.SetActiveLayer(layer);
	}

	private void HandleCostChanged()
	{
		int currentValueOfResourcesChangeCount = base.ViewModel.CurrentValueOfResourcesChangeCount;
		if (currentValueOfResourcesChangeCount != 0)
		{
			string text = ((currentValueOfResourcesChangeCount > 0) ? "+" : string.Empty);
			m_PlusNavigatorResourceObject.text = text + currentValueOfResourcesChangeCount;
			m_PlusNavigatorResourceAnimator.AppearAnimation(delegate
			{
				m_PlusNavigatorResourceAnimator.DisappearAnimation();
			});
			Sequence s = DOTween.Sequence();
			s.Append(m_PlusNavigatorResourceObject.gameObject.transform.DOLocalMoveY(136f, 0.5f));
			s.AppendInterval(0.5f);
			s.Append(m_PlusNavigatorResourceObject.gameObject.transform.DOLocalMoveY(186f, 1f));
			s.Append(m_PlusNavigatorResourceObject.gameObject.transform.DOLocalMoveY(Vector3.zero.y, 0f));
			base.ViewModel.CurrentValueOfResourcesChangeCount = 0;
		}
	}
}
