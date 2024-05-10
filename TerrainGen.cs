using Godot;
using System;
using System.Collections.Generic;
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
	public float kantAfstand;

	public uint[] MeshSeed = new uint[]
	{
		328415744,
		191152071,
		204841932
	};

	public uint[] TræNoiseSeed = new uint[]
	{
		819465230,
		357921046,
		625049183
	};

	public uint[] xOffsetSeed = new uint[]
	{
		542038179,
		375612890,
		908123746
	};

	public uint[] zOffsetSeed = new uint[]
	{
		148932057,
		295684173,
		632019784
	};



	// Træ-scene
	PackedScene treeScene;

	// laver værdi til træ-noise punkt
	public float træNoise;

	// laver mindsteværdi for træ-noise før træ bliver spawnet (Mellem 0 og 1)
	[Export]
	public float træThreshold = 0.2f;

	// Laver noise variabler til x og y-offset for træ
	public float xOffset;
	public float zOffset;
	// Laver Max værdi for x og z-offset for træ
	[Export]
	public float maxTræOffset = 0.1f;

	public Node3D træNode;

	// Laver distribution faktor for træer
	[Export]
	public float distributionFaktor = 2.5f;

	// Laver variabel til at bestemme generation case af træer
	public int GenerationCase;

	// Laver variabel til at bestemme om træer skal genereres
	public bool genererTræ;

	// Laver variabel til at bestemme kantblur for træer
	[Export]
	public float træKantBlur = 0.07f;

	// Laver variabel til at bestemme blur mellem græs og træ
	[Export]
	public float StenKantBlur = 0.00f;


	// Vægt til afstandsberegning
	public float vægt;

	
	
	// Eulers tal
	public float e = 2.71828f;


	
	
	
	
	
	// Test variabler
	public int TestCounter1 = 0;
	public int TestCounter2 = 100;
	public int TestCounter3 = 100;
	public int TestCounter4 = 100000;

	public float TestValue1 = 0.2f;

	public bool TestBool = true;







	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(update)
		{
			FjernTræer();
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

		// Loader træ-scene
		treeScene = GD.Load<PackedScene>("res://Træ/træscene.tscn");
		
		// For hver punkt i zSize+1
		for(int z = 0; z < zSize+1; z++)
		{

			// For hver punkt xSize+1
			for(int x = 0; x < xSize+1; x++)
			{

				// Bestemmer y-værdi til vertex (højde) ud fra Noise (16 oktaver for masser af detaljer)
				y = NoiseMAGIC(x,z, MeshSeed, 16) * MULTIPLIER;
				
				
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


					
					// [------------------------------------------------------------------]
					// [------------------------- TRÆ GENERERING -------------------------]
					// [------------------------------------------------------------------]
					
					// Load node der skal holde træer
					træNode = GetParent().GetNode<Node3D>("TRÆER");
					
					// Bestemmer værdi for træ-noise (2 oktaver for at få mere udlignet støj)
					træNoise = NoiseMAGIC(x,z, TræNoiseSeed, 2);
					
					// Contrast
					//træNoise *= 1.2f;
					// Clipping using clamp function
					//træNoise = Mathf.Clamp(træNoise, 0.0f, 1.0f);



					// Placering af træer er baseret på sandsynligheder.
					// Hvis vi modtager en støjværdi på 1 (Maksimal styrke), vil der f.eks. være 1/1 chance for at spawne et træ (100% chance)
					// Hvis vi derimod modtager en støjværdi på 0.5, vil der være 1/2 [0,5/1] chance for at spawne et træ (50% chance)


					// Generer tilfældigt tal mellem 0 og 1
					// Derefter anvendes en negativ aftagende eksponentialfunktion for at manipulere sandsynlighed for at spawne et træ (Densitet af træer)
					float random = CumulativeDistribution((float)new Random().NextDouble(), distributionFaktor);


					// Vi behandler generering af træer forskelligt efter hvor høj Noise-værdien er
					// Hvis støjværdien er højere end threshold (Høj sandsynlighed for at spawne træ)
					if(træNoise > træThreshold)
					{
						// Hvis random værdi er højere end støjværdi, vil træ spawne
						if(random > træNoise){genererTræ = true;}
						
						// Hvis random værdi er lavere end støjværdi, spawner træet ikke
						else{genererTræ = false;}
					}
					
					// Hvis støjværdien er lavere end threshold
					else
					{
						// Udregn afstand til threshold
						float distanceToThreshold = Mathf.Abs(træNoise - træThreshold);

						// Hvis afstanden til threshold er større end kantblur, spawner træet ikke
						genererTræ = false;

						// Hvis afstanden til threshold er mindre end kantblur, spawner træet med en chance baseret på afstanden
						if(distanceToThreshold < træKantBlur)
						{
							// Laver vægt, baseret på afstand til threshold
							float vægt = 1 - (distanceToThreshold/træKantBlur);

							// Hvis random værdi med vægt er lavere end vægten, vil træ spawne
							if(random < vægt){genererTræ = true;}
						}

					}
					

					// Vi tjekker om vores random værdi er højere end vores støj-værdi og om støjværdien er højere end vores threshold - Hvis det er sandt, spawnes et træ
					if(genererTræ)
					{
						// Hvis træ er tæt på sten, spawner træet efter afstand til sten
						
						// Udregn afstand til bjerg
						float distanceToMountain = Mathf.Abs(ROCKLEVEL - y);
						
						if(distanceToMountain < StenKantBlur)
						{
							// Laver vægt, baseret på afstand til bjerg
							vægt = 1 - (distanceToMountain/StenKantBlur);
							GD.Print("Distance to mountain: ", distanceToMountain);
							GD.Print("Vægt: ", vægt);
							GD.Print("0-1: ", distanceToMountain/MAXLEVEL);
							GD.Print("");
							GD.Print("");

						}
						

						

						// Sæt vægt
						//vægt = 1 - (distanceToMountain/StenKantBlur);
						
						
						// [----------- Laver offset for træ -----------]	

						// Bestemmer random x og z-akse offsets for træ
						xOffset = (float)new Random().NextDouble()*maxTræOffset;
						zOffset = (float)new Random().NextDouble()*maxTræOffset;


						// [----------- Placerer Træ -----------]	
						
						// Laver træ som instans af treeScene
						træscene NytTræ = treeScene.Instantiate() as træscene;

						// Sætter position for træ
						NytTræ.Position = new Vector3(x+xOffset, y, z+zOffset);

						// Tilføjer træ til træNode
						træNode.AddChild(NytTræ);
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
	public float NoiseMAGIC(float x, float y, uint[] seed, int okataver)
	{
		
		// Nulstiller højde
		float val = 0;

		// Sætter frekvens og amplitude 
		float frequency = 1f;
		float amplitude = 1f;

		// Laver 16 oktaver/lag af perlin noise og lægger dem sammen
		for (int i = 0; i < okataver; i++)
		{

			// Kalder perlin noise funktion fra PerlinNoise.cs
			val += perlinNoise._perlinNoise(x * frequency / GRID_SIZE, y * frequency / GRID_SIZE, seed) * amplitude;

			// Fordobler frekvens og halverer amplitude for hver ny oktav
			frequency *= 2;
			amplitude /= 2;
		}


		// Contrast
		val *= 1.2f;
		// Clipping using clamp function
		val = Mathf.Clamp(val, -1.0f, 1.0f);

		// map value to 0.0-1.0 manually
		val = (val + 1.0f) * 0.5f;

		// Returnerer noise værdi (Højde) for den givne x og y værdi
		return val;
	}


	// Fjerner træer når der genereres nyt terrain
	public void FjernTræer()
	{
		// Går igennem alle 'børn' under TRÆER og fjerner dem
		foreach (Node child in GetParent().GetNode<Node3D>("TRÆER").GetChildren())
		{
			child.QueueFree();
		}

	}

	// Negativ aftagende eksponentialfunktion for at manipulere sandsynlighed for at spawne et træ
	public float CumulativeDistribution(float x, float y)
	{	
		// Funktion
		float x2 = (float)(1 - Mathf.Pow(e, -y * x))/(1 - Mathf.Pow(e, -y));

		// Printer værdier (Max 10 gange)
		if(TestCounter1 < 10)
		{
			GD.Print(TestCounter1,": CumulativeDistribution før: ",x, "   -   efter: ", x2);
			TestCounter1 += 1;
		}
		
		return x2;
		
	}






}
