using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntityStatisticsOptOutPCView : SettingsEntityView<SettingsEntityStatisticsOptOutVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image m_HighlightedImage;

	[SerializeField]
	private Color NormalColor = Color.clear;

	[SerializeField]
	private Color HighlightedColor = new Color(0.52f, 0.52f, 0.52f, 0.29f);

	[SerializeField]
	private OwlcatMultiButton m_GoToStatisticsButton;

	[SerializeField]
	private TextMeshProUGUI m_GoToStatisticsButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_DeleteStatisticsDataButton;

	[SerializeField]
	private TextMeshProUGUI m_DeleteDataButtonLabel;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_GoToStatisticsButtonLabel.text = UIStrings.Instance.SettingsUI.ShowStatistics;
		AddDisposable(m_GoToStatisticsButton.OnLeftClick.AsObservable().Subscribe(base.ViewModel.OpenSettingsInBrowser));
		m_DeleteDataButtonLabel.text = UIStrings.Instance.SettingsUI.DeleteStatisticsData;
		AddDisposable(m_DeleteStatisticsDataButton.OnLeftClick.AsObservable().Subscribe(base.ViewModel.DeleteStatisticsData));
	}

	public void SetupColor(bool isHighlighted)
	{
		if (m_HighlightedImage != null)
		{
			m_HighlightedImage.color = (isHighlighted ? HighlightedColor : NormalColor);
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
		{
			h.HandleShowSettingsDescription(base.ViewModel.UISettingsEntity);
		});
		SetupColor(isHighlighted: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetupColor(isHighlighted: false);
	}

	protected override void UpdateLocalization()
	{
		base.UpdateLocalization();
		SetButtonsTexts();
	}

	private void SetButtonsTexts()
	{
		m_GoToStatisticsButtonLabel.text = UIStrings.Instance.SettingsUI.ShowStatistics;
		m_DeleteDataButtonLabel.text = UIStrings.Instance.SettingsUI.DeleteStatisticsData;
	}
}
