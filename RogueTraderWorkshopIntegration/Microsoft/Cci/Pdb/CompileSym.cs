namespace Microsoft.Cci.Pdb;

internal struct CompileSym
{
	internal uint flags;

	internal ushort machine;

	internal ushort verFEMajor;

	internal ushort verFEMinor;

	internal ushort verFEBuild;

	internal ushort verMajor;

	internal ushort verMinor;

	internal ushort verBuild;

	internal string verSt;

	internal string[] verArgs;
}
