using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public class InputBindStructConsoleHintDescription : ConsoleHintDescription
{
	public readonly InputBindStruct InputBindStruct;

	public InputBindStructConsoleHintDescription(Transform holderTransform, ConsoleHint hintPrefab, HintsLabelPlacement labelPlacement, InputBindStruct inputBindStruct, string label)
		: base(holderTransform, hintPrefab, labelPlacement, inputBindStruct.InputLayer, label)
	{
		InputBindStruct = inputBindStruct;
	}

	protected override void BindHint(ConsoleHint hint)
	{
		base.BindHint(hint);
		hint.Bind(InputBindStruct);
	}

	public override void Dispose()
	{
		base.Dispose();
		InputBindStruct?.Dispose();
	}
}
