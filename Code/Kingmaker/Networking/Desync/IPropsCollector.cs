using System.Collections.Generic;

namespace Kingmaker.Networking.Desync;

public interface IPropsCollector
{
	Dictionary<string, string> Collect();
}
