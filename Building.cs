record Building
{
	public int X;
	public int Height;
	public int DarkWidth;
	public int LightWidth;
	public int PenthouseStartWidthDelta;
	public int PenthouseEndWidthDelta;

	public int Width => DarkWidth + LightWidth;
}
