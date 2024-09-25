using Kingmaker.Blueprints;

namespace Kingmaker.UI.Common.DebugInformation;

public interface IHasBlueprintInfo
{
	BlueprintScriptableObject Blueprint { get; }
}
