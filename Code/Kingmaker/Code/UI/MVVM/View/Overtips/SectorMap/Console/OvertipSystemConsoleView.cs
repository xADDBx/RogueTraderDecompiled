using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup.Console;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.UI.Common;
using Kingmaker.UI.Pointer;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.Console;

public class OvertipSystemConsoleView : OvertipSystemView, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[Header("SpaceSystemPopup")]
	[SerializeField]
	private SpaceSystemNavigationButtonsConsoleView m_SpaceSystemNavigationButtonsConsoleView;

	[SerializeField]
	private OwlcatButton m_HoverButton;

	public readonly BoolReactiveProperty IsNavigationValid = new BoolReactiveProperty(initialValue: true);

	[HideInInspector]
	public bool IsFocused;

	private RectTransform RectTransform => base.transform as RectTransform;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_HoverButton.OnHoverAsObservable().Subscribe(delegate(bool state)
		{
			if (state)
			{
				ConsoleCursor.Instance.SetCursorPositionToTarget(RectTransform.anchoredPosition, smooth: true);
			}
			SetFocus(state);
		}));
		AddDisposable(m_HoverButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CheckPing();
		}));
		AddDisposable(base.ViewModel.SpaceSystemNavigationButtonsVM.Subscribe(m_SpaceSystemNavigationButtonsConsoleView.Bind));
	}

	public SectorMapObjectEntity GetSectorMapObject()
	{
		return base.ViewModel?.SectorMapObject;
	}

	public void SetFocus(bool value)
	{
		if (IsFocused == value)
		{
			return;
		}
		IsFocused = value;
		GetSectorMapObject().SetConsoleFocusState(value);
		if (UINetUtility.IsControlMainCharacter())
		{
			if (value)
			{
				base.ViewModel.ShowSpaceSystemPopup();
			}
			else
			{
				base.ViewModel.CloseSpaceSystemPopup();
			}
		}
	}

	public bool IsValid()
	{
		if (!IsNavigationValid.Value)
		{
			return false;
		}
		if (base.ViewModel?.SectorMapObject == null)
		{
			return false;
		}
		if (Game.Instance.SectorMapController.IsInformationWindowInspectMode)
		{
			return false;
		}
		if (base.ViewModel.IsExploredAndVisible && base.ViewModel.SectorMapObject.View.isActiveAndEnabled)
		{
			return UIUtilityGetRect.CheckObjectInRect(base.ViewModel.SectorMapObject.View.Data.Position, 100f, 100f);
		}
		return false;
	}

	private void CheckPing()
	{
		if (base.ViewModel.IsNotMainCharacter.Value && IsFocused)
		{
			base.ViewModel.CheckPingCoop();
		}
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public bool CanConfirmClick()
	{
		return IsFocused;
	}

	public void OnConfirmClick()
	{
		CheckPing();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
