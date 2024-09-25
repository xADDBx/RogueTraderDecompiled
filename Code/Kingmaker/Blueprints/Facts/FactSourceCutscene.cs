using Kingmaker.AreaLogic.Cutscenes;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.Facts;

public class FactSourceCutscene
{
	[JsonProperty]
	public readonly string EntityId = "";

	[JsonProperty]
	public readonly string Name = "";

	[JsonProperty]
	public readonly string State = "";

	[JsonConstructor]
	public FactSourceCutscene()
	{
	}

	public FactSourceCutscene(CutscenePlayerData src)
	{
		EntityId = src.UniqueId;
		Name = src.Cutscene.name;
		State = src.HoldingState?.SceneName ?? "";
	}

	public static implicit operator FactSourceCutscene(CutscenePlayerData src)
	{
		if (src != null)
		{
			return new FactSourceCutscene(src);
		}
		return null;
	}

	public override string ToString()
	{
		return Name + " (" + State + ": " + EntityId + ")";
	}
}
