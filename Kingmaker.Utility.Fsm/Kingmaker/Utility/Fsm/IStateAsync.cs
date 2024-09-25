using System.Threading.Tasks;

namespace Kingmaker.Utility.Fsm;

public interface IStateAsync
{
	Task OnEnter();

	Task OnExit();
}
