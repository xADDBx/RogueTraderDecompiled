using System.Runtime.InteropServices;

namespace Microsoft.Cci.Pdb;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct LeafPointer
{
	internal struct LeafPointerBody
	{
		internal uint utype;

		internal LeafPointerAttr attr;
	}
}
