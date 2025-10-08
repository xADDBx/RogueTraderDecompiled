using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipAbilitiesConsoleView : ShipAbilitiesBaseView, IShipCustomizationPage
{
	[Header("Console Part")]
	[SerializeField]
	private CharInfoFeatureConsoleView m_CharInfoFeatureConsoleView;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private bool m_InputAdded;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour?.DeepestFocusAsObservable.Subscribe(OnFocus));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InputAdded = false;
	}

	protected override void DrawEntitiesImpl()
	{
		base.DrawEntitiesImpl();
		m_ActiveAbilitiesWidgetList.DrawEntries(base.ViewModel.ActiveAbilities, m_CharInfoFeatureConsoleView);
		m_PassiveAbilitiesWidgetList.DrawEntries(base.ViewModel.PassiveAbilities, m_CharInfoFeatureConsoleView);
	}

	public ConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.SetEntitiesGrid(m_ActiveAbilitiesWidgetList.Entries.Cast<IConsoleEntity>().ToList(), 2);
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour2 = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour2.SetEntitiesGrid(m_PassiveAbilitiesWidgetList.Entries.Cast<IConsoleEntity>().ToList(), 2);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour2);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		return m_NavigationBehaviour;
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
		if (!m_InputAdded)
		{
			m_InputAdded = true;
		}
	}

	private void OnFocus(IConsoleEntity entity)
	{
		CharInfoFeatureConsoleView charInfoFeatureConsoleView = entity as CharInfoFeatureConsoleView;
		if ((bool)charInfoFeatureConsoleView)
		{
			RectTransform targetRect = charInfoFeatureConsoleView.transform as RectTransform;
			m_ActiveAbilitiesRectExtended.EnsureVisibleVertical(targetRect);
			m_PassiveAbilitiesRectExtended.EnsureVisibleVertical(targetRect);
			HandleTooltip(entity);
		}
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (!(monoBehaviour == null) && entity is CharInfoFeatureConsoleView charInfoFeatureConsoleView)
		{
			TooltipConfig tooltipConfig = default(TooltipConfig);
			tooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0f)
			};
			TooltipConfig config = tooltipConfig;
			monoBehaviour.ShowConsoleTooltip(charInfoFeatureConsoleView.TooltipTemplate(), m_NavigationBehaviour, config);
		}
	}

	public bool CanOverrideClose()
	{
		return !base.IsBinded;
	}
}
