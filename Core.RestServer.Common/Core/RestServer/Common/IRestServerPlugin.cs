using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Core.RestServer.Common;

public interface IRestServerPlugin
{
	HttpMethod HttpMethod { get; }

	string LocalPath { get; }

	Task Handle(HttpListenerContext context, CancellationToken cancellationToken);
}
