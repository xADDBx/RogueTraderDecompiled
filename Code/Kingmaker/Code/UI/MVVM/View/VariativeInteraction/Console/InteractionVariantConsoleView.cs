using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Pointer;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction.Console;

public class InteractionVariantConsoleView : InteractionVariantView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private ConsoleHint m_Hint;

	[SerializeField]
	private TextMeshProUGUI m_ResourcesHint;

	private InputLayer m_InputLayer;

	public void SetInputLayer(InputLayer inputLayer)
	{
		m_InputLayer = inputLayer;
		m_Hint.gameObject.SetActive(value: false);
		AddDisposable(m_Hint.BindCustomAction(8, m_InputLayer, m_Button.OnFocusAsObservable().And(ConsoleCursor.Instance.IsNotActiveProperty).ToReactiveProperty()));
		AddDisposable(m_Button.OnFocusAsObservable().Subscribe(delegate
		{
			base.transform.SetAsLastSibling();
		}));
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		TextMeshProUGUI resourcesHint = m_ResourcesHint;
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		resourcesHint.text = ((requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0) ? GetConsoleResourceHint() : string.Empty);
	}

	protected string GetConsoleResourceHint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{UIStrings.Instance.Overtips.HasResourceCount.Text}: {base.ViewModel.ResourceCount}\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.RequiredResourceCount.Text}: {base.ViewModel.RequiredResourceCount}\n");
		return stringBuilder.ToString();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel != null;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.Interact();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public Vector2 GetPosition()
	{
		return m_Button.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
