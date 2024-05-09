using Godot;
using System;
using PerlinNoise;




public partial class TerrainGen : MeshInstance3D
{
	// Sætter variabler
	[Export]
	public bool update = false;

	[Export]
	public int xSize = 20;

	[Export]
	public int zSize = 20;

	[Export]
	public int GRID_SIZE = 400;

	[Export]
	public float MULTIPLIER = 1.0f;

	[Export]
	public float vertDistance = 1.0f;

	[Export]
	public float SURFACELEVEL = 0.0f;

	public float SANDLEVEL;

	public float GRASSLEVEL;

	public float ROCKLEVEL;

	public float SNOWLEVEL;

	public float MAXLEVEL;

	public float y;

	public int vert;
	public Vector2 uv;

	public uint[] seeds;

	// Loader træ som scene
	PackedScene treeScene = GD.Load<PackedScene>("res://Træ/træscene.tscn");

	// laver værdi til træ-noise punkt
	public float træNoise;

	// laver mindsteværdi for træ-noise før træ bliver spawnet
	[Export]
	public float træThreshold = 0.5f;

	// Laver noise variabler til x og y-offset for træ
	public float xOffset;
	public float zOffset;
	// Laver Max værdi for x og z-offset for træ
	[Export]
	public float maxTræOffset = 0.1f;




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(update)
		{
			Generate_Terrain();
			update = false;
		}
	}



	public void Generate_Terrain()
	{	
		// Laver Image til farver (Bruges til texture)
		Image image = Image.Create(xSize, zSize, false, Image.Format.Rgb8);

		// Udregner højde for skift mellem biomer
		MAXLEVEL = MULTIPLIER;
		SANDLEVEL = (MAXLEVEL-SURFACELEVEL)*0.03f + SURFACELEVEL;
		GRASSLEVEL = (MAXLEVEL-SURFACELEVEL)*0.2f + SURFACELEVEL;
		ROCKLEVEL = (MAXLEVEL-SURFACELEVEL)*0.6f + SURFACELEVEL;
		SNOWLEVEL = (MAXLEVEL-SURFACELEVEL)*0.8f + SURFACELEVEL;


		// PRINTING LEVELS
		GD.Print("SURFACELEVEL: ", Mathf.Round(SURFACELEVEL));
		GD.Print("SANDLEVEL: ", Mathf.Round(SANDLEVEL));
		GD.Print("GRASSLEVEL: ", Mathf.Round(GRASSLEVEL));
		GD.Print("ROCKLEVEL: ", Mathf.Round(ROCKLEVEL));
		GD.Print("SNOWLEVEL: ", Mathf.Round(SNOWLEVEL));
		GD.Print("MAXLEVEL: ", Mathf.Round(MAXLEVEL));

		
		// Laver Arraymesh og Surfacetool til generering af mesh
		ArrayMesh a_mesh = new ArrayMesh();
		SurfaceTool st = new SurfaceTool();

		// Starter Surfacetool
		st.Begin(Mesh.PrimitiveType.Triangles);
		
		// For hver punkt i zSize+1
		for(int z = 0; z < zSize+1; z++)
		{

			// For hver punkt xSize+1
			for(int x = 0; x < xSize+1; x++)
			{

				
				// Sætter mesh seed
				seeds = new uint[]
				{
				3284157443,
				1911520717,
				2048419325
				};

				// Bestemmer y-værdi ud fra Noise funktion (PT BARE BØLGER)
				y = NoiseMAGIC(x,z, seeds) * MULTIPLIER;
				
				
				// get distance from center
				//float distanceToMiddle = Mathf.Sqrt(Mathf.Pow(x - xSize / 2, 2) + Mathf.Pow(z - zSize / 2, 2));
				// set y value to be higher in the middle
				//y += centerMultiplier * Mathf.Pow(1 - Mathf.Clamp(distanceToMiddle / (xSize / 2), 0, 1), 2);
				

				// BESTEMMER BIOME
				if (y <= SURFACELEVEL)
				{	
					// VERTEX IS WATER
					// FLOOR SURFACE VALUE
					y = SURFACELEVEL;

					// SET COLOR TO BLUE
					image.SetPixel(x, z, new Color(0.0f, 0.0f, 1.0f, 1.0f));
				}

				if (y <= SANDLEVEL && y > SURFACELEVEL)
				{
					// VERTEX IS SAND
					// SET COLOR TO YELLOW
					image.SetPixel(x, z, new Color(1.0f, 1.0f, 0.0f, 1.0f));
				}

				if (y <= GRASSLEVEL && y > SANDLEVEL)
				{
					// VERTEX IS GRASS
					// SET COLOR TO GREEN
					image.SetPixel(x, z, new Color(0.0f, 1.0f, 0.0f, 1.0f));


					// [------------------------- TRÆ GENERERING -------------------------]
					
					// Load node som træ skal placeres under (gå til parent, og find node med navn "TRÆER")
					Node træNode = GetParent().GetNode("TRÆER");


					// Sætter træ-noise seed
					seeds = new uint[]
					{
					8194652307,
					3579210468,
					6250491837
					};

					// Bestemmer værdi for træ-noise
					træNoise = NoiseMAGIC(x,z, seeds);

					// Hvis træ-noise er over threshold, spawnes træ
					if(træNoise > træThreshold)
					{

						// [----------- Laver offset for træ -----------]	

						// Sætter x offset seed
						seeds = new uint[]
						{
						1489320576,
						2956841730,
						6320197845
						};

						// Bestemmer x offset for træ ud fra noise
						xOffset = (NoiseMAGIC(x,z, seeds) * maxTræOffset)-maxTræOffset/2;


						// Sætter z offset seed
						seeds = new uint[]
						{
						7630189425,
						3102968475,
						5021893746
						};

						// Bestemmer z offset for træ ud fra noise
						zOffset = (NoiseMAGIC(x,z, seeds) * maxTræOffset)-maxTræOffset/2;


						// [----------- Placerer Træ -----------]	
						
						// Laver træ som instans af treeScene
						Træ træ = (Træ)treeScene.Instance() as Træ;

						// Sætter position for træ
						træ.Position = new Vector3(x*vertDistance+xOffset, y, z*vertDistance+zOffset);

						// Tilføjer træ til træNode
						træNode.AddChild(træ);


						// INSPIRATION KODE
						//spawn spillere
						/*spiller Nyspiller = Spiller.Instantiate() as spiller;
						GD.Print("Spiller"+i+" spawned");

						//set position
						Nyspiller.Position = new Vector2(i*100,100);

						AddChild(Nyspiller);
						GD.Print("Spiller"+i+" added to scene");*/

					}

					
				


				}

				if (y <= ROCKLEVEL && y > GRASSLEVEL)
				{
					// VERTEX IS ROCK
					// SET COLOR TO GREY
					image.SetPixel(x, z, new Color(0.5f, 0.5f, 0.5f, 1.0f));
				}

				if (y <= SNOWLEVEL && y > ROCKLEVEL)
				{
					// VERTEX IS SNOW
					// SET COLOR TO WHITE
					image.SetPixel(x, z, new Color(1.0f, 1.0f, 1.0f, 1.0f));
				}

				if (y > SNOWLEVEL)
				{
					// VERTEX IS MOUNTAIN
					// SET COLOR TO White
					image.SetPixel(x, z, new Color(1.0f, 1.0f, 1.0f, 1.0f));
				}

				

				// Laver UV data for dette punkt
				uv = new Vector2(Mathf.InverseLerp(0, xSize, x),Mathf.InverseLerp(0, zSize, z));
				st.SetUV(uv);
				
				// Tilføjer denne vertex til surfacetool
				st.AddVertex(new Vector3(x*vertDistance, y, z*vertDistance));
			}


		}

		// GEM BILLEDE SOM PNG (KUN FOR TEST)
		image.SavePng("res://terrainImage.png");



		// -------------- LAV MESH AF TREKANTER -------------- //

		// Sætter vertex til 0
		vert = 0;

		// For hver punkt i zSize
		for(int z = 0; z < zSize; z++)
		{
			// For hver punkt i xSize
			for(int x = 0; x < xSize; x++)
			{
				// Laver trekant 1 i urets rækkefølge
				st.AddIndex(vert+0);
				st.AddIndex(vert+1);
				st.AddIndex(vert + xSize + 1);

				// Laver trekant 2 i urets rækkefølge
				st.AddIndex(vert + xSize + 1);
				st.AddIndex(vert + 1);
				st.AddIndex(vert + xSize + 2);

				// Gå til næste vertex
				vert++;
			}
			vert++;
			// Gå til næste vertex

		}

		// Generer Normals ved brug af SurfaceTool
		st.GenerateNormals();

		// Commit mesh og sæt den på Meshinstance
		a_mesh = st.Commit(a_mesh);
		Mesh = a_mesh;
		
		// Laver texture og sætter den på material
		ImageTexture colorTexture = new ImageTexture();
		colorTexture.SetImage(image);

		// Laver material
		StandardMaterial3D material = new StandardMaterial3D();
		material.AlbedoTexture = colorTexture;

		// Sætter material på mesh
		MaterialOverride = material;
	}



	// Noise funktion (Perlin Noise), der kalder funktion fra PerlinNoise.cs og returnerer værdi mellem 0 og 1
	public float NoiseMAGIC(float x, float y, uint[] seed)
	{

		// Sætter seed
		seeds = seed;

		// Nulstiller højde
		float val = 0;

		// Sætter frekvens og amplitude
		float frequency = 1f;
		float amplitude = 1f;



		// Laver 16 oktaver/lag af perlin noise og lægger dem sammen
		for (int i = 0; i < 16; i++)
		{

			// Kalder perlin noise funktion fra PerlinNoise.cs
			val += perlinNoise._perlinNoise(x * frequency / GRID_SIZE, y * frequency / GRID_SIZE, seeds) * amplitude;

			// Fordobler frekvens og amplitude halveres for hver oktav
			frequency *= 2;
			amplitude /= 2;
		}



		// Contrast
		val *= 1.2f;
		// Clipping using th function
		val = Mathf.Clamp(val, -1.0f, 1.0f);

		// map value to 0.0-1.0 manually
		val = (val + 1.0f) * 0.5f;

		// Returnerer noise værdi (Højde) for den givne x og y værdi
		return val;
	}


}
