using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameModes;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class ShipPostsPanelConsoleView : ShipPostsPanelBaseView
{
	[SerializeField]
	private ShipPostConsoleView[] m_ShipPostViews;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private MonoBehaviour[] m_AnimatorObjects;

	[SerializeField]
	private Image[] m_PostGlitchImages;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private bool m_ShowTooltip;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private List<IUIAnimator> m_Animators;

	private Dictionary<int, List<IConsoleNavigationEntity>> m_Entities = new Dictionary<int, List<IConsoleNavigationEntity>>();

	private int m_CurrentPostIndex = -1;

	private readonly BoolReactiveProperty m_IsPlayerTurn = new BoolReactiveProperty();

	public void Initialize()
	{
		m_Animators = m_AnimatorObjects.SelectMany((MonoBehaviour o) => o.GetComponents<IUIAnimator>()).ToList();
	}

	protected override void BindViewImplementation()
	{
		CreateInput();
		if (base.ViewModel.Posts.Count != m_ShipPostViews.Length)
		{
			PFLog.UI.Error("Wrong posts count!");
			return;
		}
		for (int i = 0; i < m_ShipPostViews.Length; i++)
		{
			m_ShipPostViews[i].Bind(base.ViewModel.Posts[i]);
		}
		AddDisposable(base.ViewModel.IsActive.Subscribe(OnActive));
		AddDisposable(base.ViewModel.IsPlayerTurn.Subscribe(delegate(bool val)
		{
			m_IsPlayerTurn.Value = val;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_ShowTooltip = false;
		m_Entities.Clear();
	}

	private void SetConsoleEntities()
	{
		m_Entities.Clear();
		for (int i = 0; i < m_ShipPostViews.Length; i++)
		{
			m_Entities.Add(i, m_ShipPostViews[i].GetConsoleEntities());
		}
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		foreach (var (_, collection) in m_Entities)
		{
			list.AddRange(collection);
		}
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow(list);
	}

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(m_HintsWidget.BindHint(inputLayer.AddButton(OnFunc02, 11, m_IsPlayerTurn), UIStrings.Instance.HUDTexts.PostsBar, ConsoleHintsWidget.HintPosition.Left));
	}

	private void OnFunc02(InputActionEventData data)
	{
		if (!(Game.Instance.CurrentMode != GameModeType.SpaceCombat) && (!Game.Instance.TurnController.TurnBasedModeActive || Game.Instance.TurnController.IsPlayerTurn))
		{
			base.ViewModel.IsActive.Value = true;
		}
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(1, 0));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ShipPostsPanelConsoleView"
		});
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(OnDecline, 9, base.ViewModel.IsActive), UIStrings.Instance.CommonTexts.Cancel, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_InputLayer.AddButton(OnDecline, 11, base.ViewModel.IsActive));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
	}

	private void OnActive(bool active)
	{
		if (active)
		{
			SetConsoleEntities();
			m_Animators.ForEach(delegate(IUIAnimator a)
			{
				a.AppearAnimation();
			});
			GamePad.Instance.PushLayer(m_InputLayer);
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			return;
		}
		m_Animators.ForEach(delegate(IUIAnimator a)
		{
			a.DisappearAnimation();
		});
		GamePad.Instance.PopLayer(m_InputLayer);
		m_NavigationBehaviour.UnFocusCurrentEntity();
		if (m_CurrentPostIndex > -1)
		{
			HighlightPost(m_CurrentPostIndex, isHighlighted: false);
		}
	}

	private void OnDecline(InputActionEventData data)
	{
		base.ViewModel.Deactivate();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip.Value = tooltipBaseTemplate != null;
		if (m_ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowTooltip(tooltipBaseTemplate, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2>
				{
					new Vector2(0.5f, 0f)
				}
			});
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
		HighlightCurrentPost(entity);
	}

	private void HighlightCurrentPost(IConsoleEntity entity)
	{
		foreach (var (num2, source) in m_Entities)
		{
			if (source.FirstOrDefault((IConsoleNavigationEntity e) => e == entity) != null)
			{
				if (m_CurrentPostIndex > -1)
				{
					HighlightPost(m_CurrentPostIndex, isHighlighted: false);
				}
				m_CurrentPostIndex = num2;
				HighlightPost(num2, isHighlighted: true);
				break;
			}
		}
	}

	private void HighlightPost(int index, bool isHighlighted)
	{
		m_ShipPostViews[index].SetHighlighted(isHighlighted);
		m_PostGlitchImages[index].enabled = isHighlighted;
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}
}
