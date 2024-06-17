using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public class ConsoleHintsWidget : MonoBehaviour, IDisposable
{
	public enum HintPosition
	{
		Left,
		Center,
		Right
	}

	[FormerlySerializedAs("m_ConsoleHintLeftPrefab")]
	[SerializeField]
	private ConsoleHint m_ConsoleHintPrefab;

	[Header("Hint Positions")]
	[SerializeField]
	private Transform m_LeftHintsContainer;

	[SerializeField]
	[FormerlySerializedAs("m_HintsContainer")]
	private Transform m_CenterHintsContainer;

	[SerializeField]
	private Transform m_RightHintsContainer;

	private readonly List<IDisposable> m_Hints = new List<IDisposable>();

	public IConsoleHint BindHint(InputBindStruct bind, string label = "", HintPosition hintPosition = HintPosition.Center)
	{
		InputBindStructConsoleHintDescription inputBindStructConsoleHintDescription = new InputBindStructConsoleHintDescription(GetHintPositionTransform(hintPosition), labelPlacement: GetLabelPlacement(hintPosition), hintPrefab: m_ConsoleHintPrefab, inputBindStruct: bind, label: label);
		inputBindStructConsoleHintDescription.Initizalize();
		m_Hints.Add(inputBindStructConsoleHintDescription);
		return inputBindStructConsoleHintDescription;
	}

	public IConsoleHint CreateCustomHint(List<int> actionIds, InputLayer inputLayer, string label = "", IReadOnlyReactiveProperty<bool> hintIsActive = null, HintPosition hintPosition = HintPosition.Center)
	{
		CustomConsoleHintDescription customConsoleHintDescription = new CustomConsoleHintDescription(GetHintPositionTransform(hintPosition), labelPlacement: GetLabelPlacement(hintPosition), hintPrefab: m_ConsoleHintPrefab, actionIds: actionIds, inputLayer: inputLayer, label: label, hintIsActive: hintIsActive);
		customConsoleHintDescription.Initizalize();
		m_Hints.Add(customConsoleHintDescription);
		return customConsoleHintDescription;
	}

	public IConsoleHint CreateCustomHint(int actionId, InputLayer inputLayer, string label = "", IReadOnlyReactiveProperty<bool> hintIsActive = null, HintPosition hintPosition = HintPosition.Center)
	{
		return CreateCustomHint(new List<int> { actionId }, inputLayer, label, hintIsActive, hintPosition);
	}

	private Transform GetHintPositionTransform(HintPosition hintPosition)
	{
		return hintPosition switch
		{
			HintPosition.Left => m_LeftHintsContainer, 
			HintPosition.Center => m_CenterHintsContainer, 
			HintPosition.Right => m_RightHintsContainer, 
			_ => m_CenterHintsContainer, 
		};
	}

	private HintsLabelPlacement GetLabelPlacement(HintPosition hintPosition)
	{
		return hintPosition switch
		{
			HintPosition.Left => HintsLabelPlacement.Right, 
			HintPosition.Center => HintsLabelPlacement.Right, 
			HintPosition.Right => HintsLabelPlacement.Left, 
			_ => HintsLabelPlacement.Right, 
		};
	}

	public void Dispose()
	{
		ClearHints();
	}

	private void ClearHints()
	{
		foreach (IDisposable hint in m_Hints)
		{
			hint.Dispose();
		}
		m_Hints.Clear();
	}
}
