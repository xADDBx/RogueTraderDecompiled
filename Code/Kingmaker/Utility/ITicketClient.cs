using System.Threading.Tasks;

namespace Kingmaker.Utility;

public interface ITicketClient
{
	Task<FindTicketsResponse> FindTickets(FindTicketsRequest request);
}
