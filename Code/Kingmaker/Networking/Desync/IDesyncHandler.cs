using Kingmaker.Networking.Hash;

namespace Kingmaker.Networking.Desync;

public interface IDesyncHandler
{
	void RaiseDesync(HashableState data, DesyncMeta meta);
}
