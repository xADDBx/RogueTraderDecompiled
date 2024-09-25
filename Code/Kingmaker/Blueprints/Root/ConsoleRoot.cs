using Kingmaker.Blueprints.Console;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

namespace Kingmaker.Blueprints.Root;

[TypeId("6d29fa7e398a4862b889eaca8eb0c605")]
public class ConsoleRoot : BlueprintScriptableObject
{
	public GamePadIcons Icons;

	public GamePadTexts.Reference Texts;

	public static ConsoleRoot Instance => BlueprintRoot.Instance.ConsoleRoot;
}
