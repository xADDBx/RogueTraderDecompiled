namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

public static class FlagExtensions
{
	public static bool HasFlag(this uint flags, ParticleFlags flag)
	{
		return (flags & (uint)flag) != 0;
	}
}
