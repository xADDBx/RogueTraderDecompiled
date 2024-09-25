using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.Console;

public class ContextMenuEntityConsoleView : ContextMenuEntityView, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_HintIcon;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if ((bool)m_HintIcon)
		{
			m_HintIcon.gameObject.SetActive(value: true);
			m_HintIcon.sprite = GamePadIcons.Instance.GetIcon(8);
		}
		if (m_ButtonFx != null)
		{
			AddDisposable(m_Button.OnFocusAsObservable().Subscribe(delegate(bool value)
			{
				m_ButtonFx.DoHovered(value);
			}));
		}
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnConfirm()
	{
		base.ViewModel.Execute();
	}

	public Vector2 GetPosition()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		if (!(rectTransform != null))
		{
			return base.transform.position;
		}
		return rectTransform.anchoredPosition;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
