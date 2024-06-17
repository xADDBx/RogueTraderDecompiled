using Kingmaker.Networking.Hash;

namespace Kingmaker.Networking.Desync;

public class DummyDesyncHandler : IDesyncHandler
{
	public void RaiseDesync(HashableState data, DesyncMeta meta)
	{
	}
}
