using System;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class SystemScannerView : ViewBase<SystemScannerVM>
{
	[SerializeField]
	private RectTransform m_Radar;

	[SerializeField]
	private WidgetListMVVM m_WidgetListObjects;

	[SerializeField]
	private SystemScannerObjectView m_SystemScannerObjectViewPrefab;

	[SerializeField]
	private CanvasGroup m_RadarCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_AnomaliesTypesText;

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.ShouldShow.CombineLatest(base.ViewModel.IsOnSystemMap, base.ViewModel.IsInSpaceCombat, (bool shouldShow, bool isOnSystemMap, bool isInSpaceCombat) => new { shouldShow, isOnSystemMap, isInSpaceCombat }).Subscribe(value =>
		{
			base.gameObject.SetActive(value.shouldShow && value.isOnSystemMap && !value.isInSpaceCombat);
		}));
		AddDisposable(base.ViewModel.IsAnomalyInteractedChanged.Skip(1).Subscribe(delegate
		{
			ResetRadar();
			DrawObjects();
		}));
		AddDisposable(base.ViewModel.IsSystemChanged.Skip(1).Subscribe(delegate
		{
			RotateRadar();
		}));
		m_AnomaliesTypesText.text = string.Empty;
		m_AnomaliesTypesText.color = new Color(1f, 1f, 1f, 0f);
	}

	private void RotateRadar()
	{
		ResetRadar();
		m_RadarCanvasGroup.alpha = 1f;
		m_Radar.DOLocalRotate(new Vector3(0f, 0f, -450f), 2f).OnStart(delegate
		{
			m_RadarCanvasGroup.DOFade(0f, 0.5f).SetDelay(1.5f);
		}).OnComplete(delegate
		{
			DrawObjects();
		})
			.SetRelative(isRelative: true)
			.SetEase(Ease.Linear)
			.SetUpdate(isIndependentUpdate: true);
	}

	private void ResetRadar()
	{
		m_WidgetListObjects.Clear();
		m_AnomaliesTypesText.text = string.Empty;
	}

	private void DrawObjects()
	{
		SystemScannerObjectVM[] vmCollection = base.ViewModel.ObjectsList.ToArray();
		m_WidgetListObjects.DrawEntries(vmCollection, m_SystemScannerObjectViewPrefab);
		SetAnomaliesTypesText();
	}

	private void SetAnomaliesTypesText()
	{
		m_AnomaliesTypesText.text = string.Empty;
		m_AnomaliesTypesText.color = new Color(1f, 1f, 1f, 0f);
		string text = string.Concat("<color=#73BE53>", UIStrings.Instance.ExplorationTexts.NoAnomalyInSystem, "</color>");
		m_AnomaliesTypesText.text = ((base.ViewModel.AnomaliesTypesText.Count == 0) ? text : string.Join(Environment.NewLine, base.ViewModel.AnomaliesTypesText));
		m_AnomaliesTypesText.DOFade(1f, 0.5f);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
