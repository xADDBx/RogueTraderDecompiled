using System;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public abstract class ConsoleHintDescription : IConsoleHint, IDisposable
{
	private Transform m_HolderTransform;

	private ConsoleHint m_HintPrefab;

	private ConsoleHint m_Hint;

	private string m_Label;

	private HintsLabelPlacement m_LabelPlacement;

	private IDisposable m_InputLayerSubscription;

	public readonly InputLayer InputLayer;

	protected ConsoleHintDescription(Transform holderTransform, ConsoleHint hintPrefab, HintsLabelPlacement labelPlacement, InputLayer inputLayer, string label)
	{
		m_HolderTransform = holderTransform;
		m_HintPrefab = hintPrefab;
		m_LabelPlacement = labelPlacement;
		m_Label = label;
		InputLayer = inputLayer;
	}

	public void Initizalize()
	{
		m_InputLayerSubscription = InputLayer.LayerBinded.Subscribe(LayerBinded);
	}

	public virtual void Dispose()
	{
		m_InputLayerSubscription?.Dispose();
		m_InputLayerSubscription = null;
		LayerBinded(binded: false);
	}

	private void LayerBinded(bool binded)
	{
		if (binded && m_Hint == null)
		{
			m_Hint = WidgetFactory.GetWidget(m_HintPrefab);
			BindHint(m_Hint);
		}
		else if (!binded && m_Hint != null)
		{
			UnbindHint(m_Hint);
			WidgetFactory.DisposeWidget(m_Hint);
			m_Hint = null;
		}
	}

	protected virtual void BindHint(ConsoleHint hint)
	{
		hint.SetLabel(m_Label);
		hint.transform.SetParent(m_HolderTransform, worldPositionStays: false);
		hint.transform.SetAsFirstSibling();
		if (hint is ConsoleHintWithAutoLayout consoleHintWithAutoLayout)
		{
			consoleHintWithAutoLayout.LabelPlacement = m_LabelPlacement;
		}
	}

	protected virtual void UnbindHint(ConsoleHint hint)
	{
		hint.PartialDispose();
	}

	public void SetLabel(string value)
	{
		m_Label = value;
		if (m_Hint != null)
		{
			m_Hint.SetLabel(m_Label);
		}
	}

	public ConsoleHint GetRealHint()
	{
		return m_Hint;
	}
}
