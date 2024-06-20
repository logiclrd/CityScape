using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SkiaSharp;

class Program
{
	static SKColor BuildingDark = new SKColor(6, 0, 54);
	static SKColor BuildingDarkWindow = new SKColor(77, 133, 173);

	static SKColor BuildingLight = new SKColor(33, 36, 91);
	static SKColor BuildingLightWindow = new SKColor(255, 251, 203);

	static SKColor TransparencyKey = new SKColor(255, 0, 255);

	static SKPaint BuildingDarkPaint =
		new SKPaint()
		{
			IsAntialias = false,
			Color = BuildingDark,
		};

	static SKPaint BuildingDarkWindowPaint =
		new SKPaint()
		{
			IsAntialias = false,
			Color = BuildingDarkWindow,
		};

	static SKPaint BuildingLightPaint =
		new SKPaint()
		{
			IsAntialias = false,
			Color = BuildingLight,
		};

	static SKPaint BuildingLightWindowPaint =
		new SKPaint()
		{
			IsAntialias = false,
			Color = BuildingLightWindow,
		};

	const int PixelSize = 4;
	const int MaximumBuildingFloors = 28;
	const int MinimumBuildingFloors = 4;
	const int MaximumBuildingWidth = 25;
	const int MinimumBuildingWidth = 3;
	const int PenthousePixels = 3;

	const int ScapeWidth = 3840 / PixelSize;

	static Random s_rnd = new Random();

	static void Main()
	{
		bool[] isCovered = new bool[ScapeWidth];
		List<Building> buildings = new List<Building>();

		while (isCovered.Any(x => !x))
		{
			Building building;

			while (true)
			{
				building = GenerateBuilding();

				bool covers = false;

				for (int i=0, l=building.Width; i < l; i++)
				{
					int xx = (i + building.X) % ScapeWidth;

					if (!isCovered[xx])
					{
						covers = true;
						isCovered[xx] = true;
					}
				}

				if (covers)
					break;
			}

			buildings.Add(building);
		}

		using (var render = RenderBuildings(buildings, ScapeWidth, MaximumBuildingFloors * 2 + PenthousePixels, PixelSize))
		{
			using (var data = render.Encode())
			using (var file = File.OpenWrite("output.png"))
			{
				file.Write(data.Span);
			}

			for (int i=0; i < ScapeWidth; i++)
			{
				Console.Write("{0} / {1}", i, ScapeWidth);
				Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b");

				using (var animationFrame = CreateAnimationFrame(render, -i * PixelSize))
				using (var data = animationFrame.Encode())
				using (var file = File.OpenWrite(Path.Combine("Frames", "Frame" + i.ToString("d4") + ".png")))
				{
					file.Write(data.Span);
				}
			}
		}
	}

	static Building GenerateBuilding()
	{
		var building = new Building();

		building.X = s_rnd.Next(ScapeWidth);
		building.Height = s_rnd.Next(MinimumBuildingFloors, MaximumBuildingFloors + 1);

		int totalBuildingWidth = s_rnd.Next(MinimumBuildingWidth, MaximumBuildingWidth);

		if (totalBuildingWidth < 2 * MinimumBuildingWidth)
		{
			// All dark or all light.
			if ((s_rnd.Next() & 1) == 0)
				building.DarkWidth = totalBuildingWidth;
			else
				building.LightWidth = totalBuildingWidth;
		}
		else
		{
			building.DarkWidth = s_rnd.Next(MinimumBuildingWidth, totalBuildingWidth - MinimumBuildingWidth + 1);
			building.LightWidth = totalBuildingWidth - building.DarkWidth;
		}

		building.PenthouseStartWidthDelta = s_rnd.Next(-1, 2);
		building.PenthouseEndWidthDelta = s_rnd.Next(-3, 4);

		return building;
	}

	static SKImage RenderBuildings(IEnumerable<Building> buildings, int width, int height, int pixelSize)
	{
		var imageInfo = new SKImageInfo(
			width: width * pixelSize,
			height: height * pixelSize,
			colorType: SKColorType.Rgba8888,
			alphaType: SKAlphaType.Unpremul);

		var surface = SKSurface.Create(imageInfo);

		var canvas = surface.Canvas;

		foreach (var building in buildings)
		{
			RenderBuilding(canvas, building, building.X, height, pixelSize);

			if (building.X + building.Width > width)
				RenderBuilding(canvas, building, building.X - width, height, pixelSize);
		}

		return surface.Snapshot();
	}

	static void RenderBuilding(SKCanvas canvas, Building building, int buildingX, int height, int pixelSize)
	{
		int buildingPixelHeight = building.Height * 2;

		canvas.DrawRect(
			pixelSize * buildingX,
			pixelSize * (height - buildingPixelHeight),
			pixelSize * building.DarkWidth,
			pixelSize * buildingPixelHeight,
			BuildingDarkPaint);

		canvas.DrawRect(
			pixelSize * (buildingX + building.DarkWidth),
			pixelSize * (height - buildingPixelHeight),
			pixelSize * building.LightWidth,
			pixelSize * buildingPixelHeight,
			BuildingLightPaint);

		for (int penthouseY = 0; penthouseY < 3; penthouseY++)
		{
			int penthouseWidthDelta = (int)Math.Round(
				(building.PenthouseStartWidthDelta * (2 - penthouseY) +
					building.PenthouseEndWidthDelta * penthouseY) / 3.0);

			int penthouseWidth = building.Width + penthouseWidthDelta * 2;

			int darkWidth = building.DarkWidth * penthouseWidth / building.Width;
			int lightWidth = penthouseWidth - darkWidth;

			canvas.DrawRect(
				pixelSize * (buildingX - penthouseWidthDelta),
				pixelSize * (height - buildingPixelHeight - penthouseY - 1),
				pixelSize * darkWidth,
				pixelSize,
				BuildingDarkPaint);

			canvas.DrawRect(
				pixelSize * (buildingX - penthouseWidthDelta + darkWidth),
				pixelSize * (height - buildingPixelHeight - penthouseY - 1),
				pixelSize * lightWidth,
				pixelSize,
				BuildingLightPaint);
		}

		// Windows
		for (int floorNumber = 1; floorNumber < building.Height; floorNumber++)
		{
			int floorY = height - floorNumber * 2;

			for (int dx = 2, l = building.Width - 2; dx < l; dx++)
			{
				if (s_rnd.Next() % 10 > 6)
				{
					var paint = (dx < building.DarkWidth) ? BuildingDarkWindowPaint : BuildingLightWindowPaint;

					canvas.DrawRect(
						pixelSize * (buildingX + dx),
						pixelSize * floorY,
						pixelSize,
						pixelSize,
						paint);
				}
			}
		}
	}

	static SKImage CreateAnimationFrame(SKImage staticImage, int xOffset)
	{
		var imageInfo = new SKImageInfo(
			width: 1920,
			height: staticImage.Height,
			colorType: SKColorType.Rgba8888,
			alphaType: SKAlphaType.Unpremul);

		var surface = SKSurface.Create(imageInfo);

		var canvas = surface.Canvas;

		canvas.DrawImage(staticImage, xOffset, 0);

		if (xOffset > 0)
			canvas.DrawImage(staticImage, xOffset - staticImage.Width, 0);
		else
			canvas.DrawImage(staticImage, xOffset + staticImage.Width, 0);

		return surface.Snapshot();
	}
}
