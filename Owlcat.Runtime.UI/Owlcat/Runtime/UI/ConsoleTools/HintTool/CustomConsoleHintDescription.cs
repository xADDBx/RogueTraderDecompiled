using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public class CustomConsoleHintDescription : ConsoleHintDescription
{
	protected readonly List<int> m_ActionIds;

	protected readonly InputLayer m_InputLayer;

	protected readonly IReadOnlyReactiveProperty<bool> m_HintIsActive;

	public CustomConsoleHintDescription(Transform holderTransform, ConsoleHint hintPrefab, HintsLabelPlacement labelPlacement, List<int> actionIds, InputLayer inputLayer, string label, IReadOnlyReactiveProperty<bool> hintIsActive)
		: base(holderTransform, hintPrefab, labelPlacement, inputLayer, label)
	{
		m_ActionIds = actionIds;
		m_InputLayer = inputLayer;
		m_HintIsActive = hintIsActive;
	}

	protected override void BindHint(ConsoleHint hint)
	{
		base.BindHint(hint);
		List<int> range = m_ActionIds.GetRange(0, m_ActionIds.Count);
		hint.BindCustomAction(range, m_InputLayer, m_HintIsActive);
	}
}
