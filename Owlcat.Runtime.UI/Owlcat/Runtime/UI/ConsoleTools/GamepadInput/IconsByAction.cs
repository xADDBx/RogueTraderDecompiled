using System;
using System.Collections.Generic;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

[Serializable]
public class IconsByAction
{
	public RewiredActionType Type;

	public List<SpriteByConsole> Icons;
}
