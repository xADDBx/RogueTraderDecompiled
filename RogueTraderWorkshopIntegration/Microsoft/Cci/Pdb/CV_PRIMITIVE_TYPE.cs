using System.Runtime.InteropServices;

namespace Microsoft.Cci.Pdb;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct CV_PRIMITIVE_TYPE
{
	private const uint CV_MMASK = 1792u;

	private const uint CV_TMASK = 240u;

	private const uint CV_SMASK = 15u;

	private const int CV_MSHIFT = 8;

	private const int CV_TSHIFT = 4;

	private const int CV_SSHIFT = 0;

	private const uint CV_FIRST_NONPRIM = 4096u;
}
