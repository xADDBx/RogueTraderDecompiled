using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.ConsoleTools.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class PrerequisiteEntryView : ViewBase<PrerequisiteEntryVM>, IWidgetView
{
	private class TMPPrerequisiteLinkNavigationEntity : TMPLinkNavigationEntity, IPrerequisiteLinkEntity
	{
		public string LinkId { get; }

		public TMPPrerequisiteLinkNavigationEntity(TextMeshProUGUI text, OwlcatMultiButton firstFocus, OwlcatMultiButton secondFocus, GlossaryPoint glossaryPoint, Action<string> onLinkClicked = null, Action<string> onLinkFocused = null)
			: base(text, firstFocus, secondFocus, glossaryPoint, onLinkClicked, onLinkFocused)
		{
			string[] keysFromLink = UIUtility.GetKeysFromLink(m_LinkId);
			if (keysFromLink.Length > 1)
			{
				LinkId = keysFromLink[1];
			}
		}
	}

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private Image m_Background;

	[Header("Colors")]
	[SerializeField]
	private Color32 m_DoneBGColor;

	[SerializeField]
	private Color32 m_RequiredBGColor;

	[SerializeField]
	private Color32 m_DoneTextColor;

	[SerializeField]
	private Color32 m_RequiredTextColor;

	[Header("Font Settings")]
	[SerializeField]
	private float m_DefaultFontSizeText = 18f;

	[SerializeField]
	private float m_DefaultFontSizeValue = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeText = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeValue = 22f;

	[Header("Focus")]
	[SerializeField]
	private SingleLinkMultiButton m_Focus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonFirstFocus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonSecondFocus;

	[Header("Console")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private int? m_LinkIndex;

	private string m_LinkKey;

	private List<TMPPrerequisiteLinkNavigationEntity> m_LinkEntities = new List<TMPPrerequisiteLinkNavigationEntity>();

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Text.text = base.ViewModel.Text;
		m_Value.text = base.ViewModel.Value;
		TextMeshProUGUI text = m_Text;
		Color color2 = (m_Value.color = (base.ViewModel.Done ? m_DoneTextColor : m_RequiredTextColor));
		text.color = color2;
		m_Background.color = (base.ViewModel.Done ? m_DoneBGColor : m_RequiredBGColor);
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Text.fontSize = (isControllerMouse ? m_DefaultFontSizeText : m_DefaultConsoleFontSizeText) * base.ViewModel.FontMultiplier;
		m_Value.fontSize = (isControllerMouse ? m_DefaultFontSizeValue : m_DefaultConsoleFontSizeValue) * base.ViewModel.FontMultiplier;
		AddDisposable(m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(CreateNavigation);
		if (Game.Instance.IsControllerMouse)
		{
			DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
			{
				AddDisposable(SetLinkHighlight());
			});
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_LinkIndex = null;
		m_LinkKey = null;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as PrerequisiteEntryVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is PrerequisiteEntryVM;
	}

	private void CreateNavigation()
	{
		m_LinkEntities = GenerateEntityList(m_Text, m_MultiButtonFirstFocus, m_MultiButtonSecondFocus, delegate
		{
			HighlightCurrentOnce();
		}, null);
		if (m_LinkEntities.Count > 1)
		{
			m_NavigationBehaviour.AddEntities(m_LinkEntities);
			return;
		}
		string linkId = string.Empty;
		if (m_LinkEntities.Count > 0)
		{
			linkId = m_LinkEntities.ElementAt(0).LinkId;
			AddDisposable(m_Focus.Button.OnConfirmClickAsObservable().Subscribe(HighlightCurrentOnce));
		}
		m_Focus.Initialize(linkId);
		m_NavigationBehaviour.AddEntity(m_Focus);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		ClearLinkIndexIfNeeded();
		if (entity == null)
		{
			return;
		}
		m_LinkIndex = m_NavigationBehaviour.Entities.IndexOf(entity);
		if (m_LinkIndex < 0 || m_LinkIndex > m_LinkEntities.Count - 1)
		{
			ClearLinkIndexIfNeeded();
			return;
		}
		m_LinkKey = m_LinkEntities[m_LinkIndex.Value].LinkId;
		if (m_LinkKey != null)
		{
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.StartHighlight(m_LinkKey);
			});
		}
	}

	public FloatConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}

	private IDisposable SetLinkHighlight()
	{
		if (m_Text == null)
		{
			return Disposable.Empty;
		}
		bool entered = TMP_TextUtilities.IsIntersectingRectTransform(m_Text.rectTransform, Input.mousePosition, UICamera.Claim());
		IDisposable enter = UniRxExtensionMethods.OnPointerEnterAsObservable(m_Text).Subscribe(delegate
		{
			entered = true;
		});
		IDisposable exit = UniRxExtensionMethods.OnPointerExitAsObservable(m_Text).Subscribe(delegate
		{
			entered = false;
		});
		IDisposable update = ObservableExtensions.Subscribe(m_Text.UpdateAsObservable(), delegate
		{
			if (!entered)
			{
				ClearFocusIfNeeded();
				ClearLinkIndexIfNeeded();
			}
			else
			{
				int num = TMP_TextUtilities.FindIntersectingLink(m_Text, Input.mousePosition, UICamera.Claim());
				if (num == -1)
				{
					ClearFocusIfNeeded();
					ClearLinkIndexIfNeeded();
				}
				else if (num != m_LinkIndex)
				{
					m_LinkIndex = num;
					m_LinkEntities.ElementAtOrDefault(m_LinkIndex.Value)?.SetFocus(value: true);
					m_LinkKey = UIUtility.GetKeysFromLink(m_Text.textInfo.linkInfo[m_LinkIndex.Value].GetLinkID())[1];
					EventBus.RaiseEvent(delegate(IUIHighlighter h)
					{
						h.StartHighlight(m_LinkKey);
					});
				}
			}
		});
		IDisposable click = m_Text.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (m_LinkIndex.HasValue && data.button == PointerEventData.InputButton.Right)
			{
				HighlightCurrentOnce();
			}
		});
		return Disposable.Create(delegate
		{
			enter?.Dispose();
			exit?.Dispose();
			update?.Dispose();
			click?.Dispose();
		});
	}

	private void HighlightCurrentOnce()
	{
		if (!string.IsNullOrEmpty(m_LinkKey))
		{
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.HighlightOnce(m_LinkKey);
			});
		}
	}

	private void ClearFocusIfNeeded()
	{
		if (m_LinkIndex.HasValue && !string.IsNullOrEmpty(m_LinkKey))
		{
			m_LinkEntities.ElementAtOrDefault(m_LinkIndex.Value)?.SetFocus(value: false);
		}
	}

	private void ClearLinkIndexIfNeeded()
	{
		int? linkIndex = m_LinkIndex;
		string linkKey = m_LinkKey;
		m_LinkIndex = null;
		m_LinkKey = null;
		if (linkIndex.HasValue && !string.IsNullOrEmpty(linkKey))
		{
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.StopHighlight(m_LinkKey);
			});
		}
	}

	private List<TMPPrerequisiteLinkNavigationEntity> GenerateEntityList(TextMeshProUGUI text, OwlcatMultiButton firstFocus, OwlcatMultiButton secondFocus, Action<string> onLinkClicked, Action<string> onLinkFocused)
	{
		return (from glossaryEntity in GlossaryPointsUtils.GetLinksCoordinatesDictionary(text)
			select new TMPPrerequisiteLinkNavigationEntity(text, firstFocus, secondFocus, glossaryEntity, onLinkClicked, onLinkFocused)).ToList();
	}
}
