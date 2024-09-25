using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public abstract class SettingsEntityWithValueConsoleView<TSettingsEntityVM> : SettingsEntityConsoleView<TSettingsEntityVM>, IConsoleNavigationEntity, IConsoleEntity, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler where TSettingsEntityVM : SettingsEntityWithValueVM
{
	[SerializeField]
	private Image m_HighlightedImage;

	[SerializeField]
	private Color NormalColor = Color.clear;

	[SerializeField]
	private Color OddColor = new Color(0.77f, 0.75f, 0.69f, 0.29f);

	[SerializeField]
	private Color HighlightedColor = new Color(0.52f, 0.52f, 0.52f, 0.29f);

	[SerializeField]
	private Image m_PointImage;

	[SerializeField]
	private Image m_MarkImage;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	private string m_ModificationNotAllowedReason;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.ModificationAllowed.Subscribe(delegate(bool value)
		{
			OnModificationChanged(base.ViewModel.ModificationAllowedReason.Value, value);
		}));
		if (m_PointImage != null && m_MarkImage != null)
		{
			AddDisposable(base.ViewModel.IsChanged.Subscribe(UpdatePoints));
		}
		SetupColor(isHighlighted: false);
	}

	private void UpdatePoints(bool isChanged)
	{
		if (m_PointImage != null)
		{
			m_PointImage.gameObject.SetActive((!isChanged && !base.ViewModel.IsSet) || base.ViewModel.HideMarkImage);
		}
		if (m_MarkImage != null)
		{
			m_MarkImage.gameObject.SetActive(isChanged && !base.ViewModel.HideMarkImage);
		}
	}

	public void SetupColor(bool isHighlighted)
	{
		Color color = (base.ViewModel.IsOdd ? OddColor : NormalColor);
		if (m_HighlightedImage != null)
		{
			m_HighlightedImage.color = (isHighlighted ? HighlightedColor : color);
		}
	}

	protected void CallNotAllowedNotification()
	{
		if (!base.ViewModel.ModificationAllowed.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(m_ModificationNotAllowedReason, addToLog: true, WarningNotificationFormat.Attention);
			});
		}
	}

	public virtual void OnModificationChanged(string reason, bool allowed = true)
	{
		m_ModificationNotAllowedReason = reason;
	}

	public virtual void SetFocus(bool value)
	{
		SetupColor(value);
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(base.ViewModel.UISettingsEntity);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleHideSettingsDescription();
			});
		}
	}

	public bool IsValid()
	{
		return true;
	}

	public abstract bool HandleLeft();

	public abstract bool HandleRight();
}
