
using Godot;
using System;
using System.Globalization; // Import af System.Globalization-modulet til kulturspecifik information.
using System.Security.Cryptography; // Import af System.Security.Cryptography-modulet til kryptografiske operationer.
using System.Text; // Importerer System.Text-modulet til manipulation af strenge.
namespace PerlinNoise // Indkapsling af relaterede klasser og funktioner i PerlinNoise-navnerummet.
{
	public partial class perlinNoise : Node3D // En klasse, der repræsenterer generering af Perlin-støj, og som arver fra Node3D.
	{
		// Seed til tilfældighedsgeneratoren.
		public static uint[] seeds = new uint[] {
			3284157443,
			1911520717,
			2048419325
		};

		private const int SeedLength = 24; // Frøets længde i tegn.
		private const int ChunkSize = 8; // Størrelsen af hver del af frøet i tegn.

		// Kaldes, når noden kommer ind i scenetræet for første gang.
		public override void _Ready(){
			// Initialiserer Perlin-støjgeneratoren med det givne seed.
			// Kommenteres i øjeblikket ud, da seed-initialisering håndteres statisk.
			// newSeed(seed);
			GD.Print("New seed: ", seeds[0], " ", seeds[1], " ", seeds[2]); // Udskriv det nye seed til fejlfindingsformål.
		}		

		// Beregn Perlin-støjværdi for givne koordinater.
		public static float _perlinNoise(float x, float y){

			int x0 = (int)x; // Heltalsdelen af x-koordinaten.
			int y0 = (int)y; // Heltalsdelen af y-koordinaten.

			int x1 = x0 + 1; // Næste heltals x-koordinat.
			int y1 = y0 + 1; // Næste heltals y-koordinat.

			// Beregn prikproduktet af gradientvektorerne i hjørnerne af gittercellen.
			float dotProductTopLeft = getDotProduct(x0, y0, x, y);
			float dotProductTopRight = getDotProduct(x1, y0, x, y);

			// Beregn interpolationsvægte for x- og y-retningen.
			float interpolationWeightX = x - x0;
			float interpolationWeightY = y - y0;

			// Interpolér topværdier langs x-aksen.
			float topInterpolated = interpolate(dotProductTopLeft, dotProductTopRight, interpolationWeightX);

			// Beregn prikprodukt for bundværdier i hjørnerne af gittercellen.
			float dotProductBottomLeft = getDotProduct(x0, y1, x, y);
			float dotProductBottomRight = getDotProduct(x1, y1, x, y);

			// Interpolér bundværdier langs x-aksen.
			float bottomInterpolated = interpolate(dotProductBottomLeft, dotProductBottomRight, interpolationWeightX);

			// Interpolér mellem top- og bundværdier langs y-aksen.
			return (float)interpolate(topInterpolated, bottomInterpolated, interpolationWeightY);
		}

		// Interpolér mellem to punkter.
		public static float interpolate(float point0, float point1, float weight){
			// Anvend glat Hermite-interpolation mellem punkter ved hjælp af vægt.
			return (point1 - point0) * (3.0f - weight * 2.0f) * weight * weight + point0;
		}

		// Beregn prikproduktet af gradient- og afstandsvektorer.
		public static float getDotProduct(int ix, int iy, float x, float y){

			Vector2 gradientVector = randomGradient(ix, iy); // Få en tilfældig gradientvektor i den angivne gittercelle.
			Vector2 distanceVector = new Vector2(x - ix, y - iy); // Beregn afstandsvektor fra gittercellen til punktet.

			// Beregn prikproduktet af gradient- og afstandsvektorerne.
			float dotProduct = gradientVector.Dot(distanceVector);

			return dotProduct;
		}

		// Generer tilfældig gradientvektor baseret på gitterkoordinater.
		public static Vector2 randomGradient(int ix, int iy){

			// Genererer pseudo-tilfældigt en vinkel baseret på gitterkoordinaterne og seeds.
			uint a = (uint)ix * seeds[0]; // Brug det første seed til x-koordinaten.
			uint b = (uint)iy * seeds[1]; // Brug det andet seed til y-koordinaten.
			uint c = (uint)(ix ^ iy) * seeds[2]; // Brug en kombination af begge seeds til variation.
			
			// Konverter de genererede værdier til en vinkel i området [0, 2*Pi].
			float randomAngle = (a ^ b ^ c) * (3.14159265f / ~(~0u >> 1));

			// Opret en vektor med komponenter, der stammer fra den tilfældige vinkel.
			Vector2 gradientVector = new Vector2((float)Math.Sin(randomAngle), (float)Math.Cos(randomAngle));

			return gradientVector;
		}

		// Generer nyt frø baseret på en given streng.
		public static void newSeed(string seed){
			// Hvis det angivne seed er tomt eller null, skal du bruge et standard seed.
			if (string.IsNullOrEmpty(seed))
				seed = "DefaultSeed";
			
			GD.Print("Bruger seed: ", seed); // Udskriv seed til fejlfindingsformål.
			
			using (MD5 md5 = MD5.Create())
			{
				// Konverter seed-strengen til bytes.
				byte[] inputBytes = Encoding.UTF8.GetBytes(seed);
				
				// Beregn MD5-hash af frøet.
				byte[] hashBytes = md5.ComputeHash(inputBytes);
				
				// Konverter hash-bytes til en hexadecimal streng.
				string hashString = BitConverter.ToString(hashBytes).Replace("-", "");

				// Hvis hash-strengen er kortere end krævet, skal du gentage den for at opfylde længdekravet.
				while (hashString.Length < SeedLength)
					hashString += hashString;

				// Initialiser et array til at gemme de nye frø.
				uint[] newSeeds = new uint[3];

				// Uddrag bidder fra hash-strengen og konverter dem til uint-frø.
				for (int i = 0; i < 3; i++){
					int startIndex = i * ChunkSize;
					string chunk = hashString.Substring(startIndex, ChunkSize);
					newSeeds[i] = Convert.ToUInt32(chunk, 16);
				}

				// Opdater de globale seeds med de nyligt genererede.
				seeds = newSeeds;
			}
		}
	}
}
