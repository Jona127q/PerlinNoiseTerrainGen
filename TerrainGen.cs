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
	public int xSize = 500;

	[Export]
	public int zSize = 500;

	[Export]
	public int GRID_SIZE = 400;

	[Export]
	public float MULTIPLIER = 400.0f;

	[Export]
	public float vertDistance = 1.0f;

	[Export]
	public float SURFACELEVEL = 100.0f;

	public float SANDLEVEL;

	public float GRASSLEVEL;

	public float ROCKLEVEL;

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
	public float træThreshold = 0.37f;

	// Laver noise variabler til x og y-offset for træ
	public float xOffset;
	public float zOffset;
	
	// Laver Max værdi for x og z-offset for træ
	[Export]
	public float maxTræOffset = 0.4f;

	public Node3D træNode;

	// Laver distribution faktor for træer
	[Export]
	public float distributionFaktor = 2.0f;

	// Laver variabel til at bestemme om træer skal genereres
	public bool genererTræ;

	// Laver variabel til at bestemme kantblur for træer
	[Export]
	public float træKantBlur = 0.05f;

	// Laver variabel til at bestemme blur mellem græs og træ
	[Export]
	public float BiomeKantBlur = 0.15f;
	public float distanceToMountain;
	public float distanceToSand;

	// Vægt til afstandsberegning
	public float vægt;

	// Eulers tal til brug i CumulativeDistribution
	public float e = 2.71828f;




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
		SANDLEVEL = (MULTIPLIER-SURFACELEVEL)*0.03f + SURFACELEVEL;
		GRASSLEVEL = (MULTIPLIER-SURFACELEVEL)*0.2f + SURFACELEVEL;
		ROCKLEVEL = (MULTIPLIER-SURFACELEVEL)*0.6f + SURFACELEVEL;

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
				
				// BESTEMMER BIOME
				if (y <= SURFACELEVEL)
				{	
					// FLOOR SURFACE VALUE
					y = SURFACELEVEL;

					// VERTEX IS WATER - SET COLOR TO BLUE
					image.SetPixel(x, z, new Color(0.0f, 0.0f, 1.0f, 1.0f));
				}

				if (y <= SANDLEVEL && y > SURFACELEVEL){image.SetPixel(x, z, new Color(1.0f, 1.0f, 0.0f, 1.0f));} // VERTEX IS SAND - SET COLOR TO YELLOW

				if (y <= ROCKLEVEL && y > GRASSLEVEL){image.SetPixel(x, z, new Color(0.5f, 0.5f, 0.5f, 1.0f));} // VERTEX IS ROCK - SET COLOR TO GREY

				if (y > ROCKLEVEL){image.SetPixel(x, z, new Color(1.0f, 1.0f, 1.0f, 1.0f));} // VERTEX IS SNOW - SET COLOR TO WHITE

				if (y <= GRASSLEVEL && y > SANDLEVEL) // VERTEX IS GRASS (Her skal der genereres træer)
				{
					// SET COLOR TO GREEN
					image.SetPixel(x, z, new Color(0.0f, 1.0f, 0.0f, 1.0f));


					
					// [------------------------------------------------------------------]
					// [------------------------- TRÆ GENERERING -------------------------]
					// [------------------------------------------------------------------]
					
					// Load node der skal holde træer
					træNode = GetParent().GetNode<Node3D>("TRÆER");
					
					// Bestemmer værdi for træ-noise (2 oktaver for at få mere simpel/udlignet støj)
					træNoise = NoiseMAGIC(x,z, TræNoiseSeed, 2);
					
					// Generer tilfældigt tal mellem 0 og 1 og anvend negativ aftagende eksponentialfunktion  - manipulerer densitet af "skove"
					float random = CumulativeDistribution((float)new Random().NextDouble(), distributionFaktor);


					// Vi behandler generering af træer forskelligt efter hvor høj Noise-værdien er
					// Hvis støjværdien er højere end threshold:
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
						// Placer ikke træ
						genererTræ = false;
					}

					
					// Hvis træ skal genereres, gør vi kanten blødere efter træKantBlur
					if(genererTræ)
					{
						// Finder afstand til threshold (mellem 0 og 1)
						kantAfstand = Math.Abs(træNoise - træThreshold);

						// Hvis afstand til threshold er mindre end kantblur, ændres chance efter afstand
						if(kantAfstand < træKantBlur)
						{
							// Laver vægt, baseret på afstand til threshold
							vægt = 1 - (kantAfstand/træKantBlur);

							// Hvis random vægtet værdi er lavere end Noise, vil træ ikke spawne alligevel
							if(random*vægt > træNoise){genererTræ = false;}

						}
					}
					

					// Hvis træ skal genereres, tjekker vi om træet er tæt på sten- eller sand-laget og gør kanten blødere efter BiomeKantBlur
					if(genererTræ)
					{
						// Finder afstand til stenkant (mellem 0 og 1)
						distanceToMountain = (GRASSLEVEL - y)/GRASSLEVEL;

						// Finder afstand til sandkant (mellem 0 og 1)
						distanceToSand = (y-SANDLEVEL)/SANDLEVEL;
						
						GD.Print("Y: ", y, "  -  Distance to Sand: ", distanceToSand, "  [Sand Level: ", SANDLEVEL, "]");

						// Sætter vægt til
						vægt = 0;

						// Hvis afstand til bjerg er mindre end kantblur, ændres chance efter afstand - Gør vægt eksponentielt aftagende
						if(distanceToMountain < BiomeKantBlur){CumulativeDistribution(vægt = 1 - (distanceToMountain/BiomeKantBlur), 3);} // Laver vægt, baseret på afstand til bjerg

						if(distanceToSand < BiomeKantBlur){CumulativeDistribution(vægt = 1 - (distanceToSand/BiomeKantBlur),3);} // Laver vægt, baseret på afstand til strand
						
						// Hvis random vægtet værdi er lavere end Noise, vil træ ikke spawne alligevel
						if(random*vægt > træNoise){genererTræ = false;}

						
					}

					

					// [----------- TRÆER GENERERES -----------]	
					if(genererTræ)
					{
						// Bestemmer random x og z-akse offsets for træ
						xOffset = (float)new Random().NextDouble()*maxTræOffset;
						zOffset = (float)new Random().NextDouble()*maxTræOffset;

						// Får ny y-værdi for træ fra samme Noise funktion, men med offset
						float y2 = NoiseMAGIC(x+xOffset,z+zOffset, MeshSeed, 16) * MULTIPLIER;


						// [----------- Placerer Træ -----------]	
						
						// Laver træ som instans af treeScene
						træscene NytTræ = treeScene.Instantiate() as træscene;

						// Sætter position for træ
						NytTræ.Position = new Vector3(x+xOffset, y2, z+zOffset);

						// Tilføjer træ til træNode
						træNode.AddChild(NytTræ);
					}
		
				}

				// Laver UV data for dette punkt
				uv = new Vector2(Mathf.InverseLerp(0, xSize, x),Mathf.InverseLerp(0, zSize, z));
				st.SetUV(uv);
				
				// Tilføjer denne vertex til surfacetool
				st.AddVertex(new Vector3(x*vertDistance, y, z*vertDistance));
			}


		}

		// GEM FARVE-BILLEDE SOM PNG (TIL RAPPORT)
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
		// Funktion / ligning: (1 - e^(-y*x))/(1 - e^(-y))
		return(float)(1 - Mathf.Pow(e, -y * x))/(1 - Mathf.Pow(e, -y));
	}


}
