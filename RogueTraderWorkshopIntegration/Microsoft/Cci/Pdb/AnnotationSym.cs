namespace Microsoft.Cci.Pdb;

internal struct AnnotationSym
{
	internal uint off;

	internal ushort seg;

	internal ushort csz;

	internal string[] rgsz;
}
