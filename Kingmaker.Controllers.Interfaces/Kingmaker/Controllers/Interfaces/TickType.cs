namespace Kingmaker.Controllers.Interfaces;

public enum TickType : byte
{
	None,
	Any,
	BeginOfFrame,
	EndOfFrame,
	Simulation
}
