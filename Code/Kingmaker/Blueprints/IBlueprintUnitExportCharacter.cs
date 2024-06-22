using Kingmaker.Blueprints.Facts;
using Kingmaker.Items;

namespace Kingmaker.Blueprints;

public interface IBlueprintUnitExportCharacter
{
	void SyncFacts(BlueprintUnitFact[] facts);

	void SyncBody(PartUnitBody partUnitBody);
}
