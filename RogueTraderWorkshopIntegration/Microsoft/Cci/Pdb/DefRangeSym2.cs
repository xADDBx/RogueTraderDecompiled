namespace Microsoft.Cci.Pdb;

internal struct DefRangeSym2
{
	internal uint id;

	internal uint program;

	internal ushort count;

	internal CV_lvar_addr_range[] range;
}
