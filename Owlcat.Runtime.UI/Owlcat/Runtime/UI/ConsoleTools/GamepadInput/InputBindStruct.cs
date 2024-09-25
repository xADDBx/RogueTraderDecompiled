using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.Dependencies;
using UniRx;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class InputBindStruct : IDisposable
{
	public readonly InputLayer InputLayer;

	public readonly List<BindDescription> Binds = new List<BindDescription>();

	public readonly ReactiveProperty<float> Percentage = new ReactiveProperty<float>();

	public bool IsLongPress;

	private IDisposable m_Debug;

	public InputBindStruct()
	{
	}

	public InputBindStruct(InputLayer inputLayer)
	{
		InputLayer = inputLayer;
	}

	public InputBindStruct(InputLayer inputLayer, BindDescription bindDescription)
		: this(inputLayer)
	{
		Binds.Add(bindDescription);
	}

	public InputBindStruct(InputLayer inputLayer, List<BindDescription> bindDescriptions)
		: this(inputLayer)
	{
		Binds.AddRange(bindDescriptions);
	}

	public InputBindStruct Debug()
	{
		Binds[0].Debug = true;
		if (Binds[0].Enabled != null)
		{
			m_Debug = Binds[0].Enabled.Subscribe(delegate(bool v)
			{
				UIKitLogger.Log($"Flag for group '{Binds[0].Group}' = {v}");
			});
		}
		return this;
	}

	public void Dispose()
	{
		InputLayer?.RemoveBinds(Binds);
		Binds.Clear();
	}
}
