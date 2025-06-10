using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.Service;

public class ExportMapsArbiterTaskFactory : IArbiterTaskFactory
{
	public ArbiterTask Create(InstructionInfo instruction, ArbiterStartupParameters parameters)
	{
		if (instruction.Name.Contains("ExportMaps"))
		{
			return new MapsExportTask();
		}
		return null;
	}
}
