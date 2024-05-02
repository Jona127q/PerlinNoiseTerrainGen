using Godot;
using System;
using System.Globalization;
using System.Numerics;
namespace PerlinNoise
{
	public partial class perlinNoise : Node3D
	{
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}



		public static float _perlinNoise(float x, float y){

			int x0 = (int)x;
			int y0 = (int)y;

			int x1 = x0 + 1;
			int y1 = y0 + 1;

			float interpolationWheightX = x - x0;
			float interpolationWheightY = y - y0;

			float dotProductTopLeft = getDotProduct(x0, y0, x, y);
			float dotProductTopRight = getDotProduct(x1, y0, x, y);

			float topInterpolated = interpolate(dotProductTopLeft, dotProductTopRight, interpolationWheightX);


			float dotProductBottomLeft = getDotProduct(x0, y1, x, y);
			float dotProductBottomRight = getDotProduct(x1, y1, x, y);

			float bottomInterpolated = interpolate(dotProductBottomLeft, dotProductBottomRight, interpolationWheightX);

			return (float)interpolate(topInterpolated, bottomInterpolated, interpolationWheightY);

		}

		public static float interpolate(float point1, float point2, float wheight){
			return (point1 - point2) * (3.0f - wheight * 2.0f) * wheight * wheight + point1;
		}

		public static float getDotProduct(int ix, int iy, float x, float y){

			Godot.Vector2 gradientVector = randomGradient(ix, iy);

			Godot.Vector2 distanceVector = new Godot.Vector2(x - (float)ix, y - (float)iy);

			System.Numerics.Vector2 gradientVector2 = new System.Numerics.Vector2(gradientVector.X, gradientVector.Y);
			System.Numerics.Vector2 distanceVector2 = new System.Numerics.Vector2(distanceVector.X, distanceVector.Y);

			float dotProduct = System.Numerics.Vector2.Dot(gradientVector2, distanceVector2);

			return dotProduct;
		}

		public static Godot.Vector2 randomGradient(int ix, int iy){
			// No precomputed gradients mean this works for any number of grid coordinates
			const int w = 8 * sizeof(uint);
			const int s = w / 2;
			uint a = (uint)ix, b = (uint)iy;
			a *= 3284157443;

			b ^= a << s | a >> w - s;
			b *= 1911520717;

			a ^= b << s | b >> w - s;
			a *= 2048419325;
			float random = a * (3.14159265f / ~(~0u >> 1)); // in [0, 2*Pi]

			// Create the vector from the angle
			Godot.Vector2 gradientVector = new Godot.Vector2((float)Math.Sin(random), (float)Math.Cos(random));

			return gradientVector;
		}
	}
}
