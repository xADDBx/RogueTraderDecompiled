using System.Collections.Generic;
using Kingmaker.Enums;

namespace Kingmaker.UnitLogic.Parts;

public interface IDestructionStagesManager
{
	string name { get; }

	IEnumerable<DestructionStage> Stages { get; }

	void ChangeStage(DestructionStage stage, bool onLoad);
}
