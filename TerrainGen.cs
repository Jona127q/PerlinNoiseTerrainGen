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
	public int StandardxSize = 500;
	public int xSize;

	[Export]
	public int StandardzSize = 500;
	public int zSize;

	[Export]
	public int GRID_SIZE = 400;

	[Export]
	public float MULTIPLIER = 400.0f;

	[Export]
	public float standardvertDistance = 1.0f;
	public float vertDistance;

	[Export]
	public float waterLevel = 150.0f;

	[Export]
	public string seed = "";
	public bool emptySeed = true;
	public float SANDLEVEL;

	public float GRASSLEVEL;

	public float ROCKLEVEL;

	public float y;

	public int vert;
	public Vector2 uv;
	public float kantAfstand;
	public string TræNoiseSeed = "TræNoiseSeed";
	public string OffsetSeed = "OffsetSeed";

	// Træ-scene
	PackedScene treeScene;

	// laver værdi til træ-noise punkt
	public float træNoise;

	// laver mindsteværdi for træ-noise før træ bliver spawnet (Mellem 0 og 1)
	[Export]
	public float træThreshold = 0.35f;

	// Laver noise variabler til x og y-offset for træ
	public float xOffset;
	public float zOffset;
	
	// Laver Max værdi for x og z-offset for træ
	[Export]
	public float maxTræOffset = 0.45f;

	public Node træNode;

	// Laver distribution faktor for træer
	[Export]
	public float distributionFaktor = 3.0f;

	// Laver variabel til at bestemme om træer skal genereres
	public bool genererTræ;

	// Laver variabel til at bestemme kantblur for træer
	[Export]
	public float træKantBlur = 0.2f;

	// Laver variabel til at bestemme blur mellem græs og træ
	[Export]
	public float BiomeKantBlur = 0.2f;
	public float distanceToMountain;
	public float distanceToSand;

	// Vægt til afstandsberegning
	public float vægt;

	// Eulers tal til brug i CumulativeDistribution
	public float e = 2.71828f;

	// Lav resolution mindre
	[Export]
	public int resolution = 1;


	// Testværdi til debugging
	public int Testværdi = 10000;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		update = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(update)
		{
			update = false;

			// Loader træ-scene
			treeScene = GD.Load<PackedScene>("res://Træ/træscene.tscn");
			GD.Print("Tree Scene Loaded");

			// Load node der skal holde træer
			try
			{
				træNode = GetParent().GetNode<Node3D>("TRÆER");
				GD.Print("TræNode loaded");
			}
			catch
			{
				GD.Print("TræNode not found");
			}

			// Fjern eksisterende træer
			FjernTræer();
			GD.Print("Træer fjernet");

			// Sætter seed for Perlin Noise (Til Mesh)
			if(string.IsNullOrEmpty(seed)){
				seed = Convert.ToString(new Random().Next());
				emptySeed = true;
			}

			perlinNoise.newSeed(seed);

			GD.Print("Seed set");

			// Manipuler detaljeringsgraden af terrain
			xSize = StandardxSize/resolution;
			zSize = StandardzSize/resolution;
			vertDistance = standardvertDistance*resolution;
			GD.Print("Resolution set");

			// Generer terrain
			Generate_Terrain();
			GD.Print("Terrain Generated");
			if(emptySeed) seed = "";
			emptySeed = false;
		}
	}



	public void Generate_Terrain()
	{	
		// Laver Image til farver (Bruges til texture)
		Image image = Image.Create(xSize, zSize, false, Image.Format.Rgb8);

		// Udregner højde for skift mellem biomer
		SANDLEVEL = (MULTIPLIER-waterLevel)*0.03f + waterLevel;
		GRASSLEVEL = (MULTIPLIER-waterLevel)*0.2f + waterLevel;
		ROCKLEVEL = (MULTIPLIER-waterLevel)*0.6f + waterLevel;

		// Laver Arraymesh og Surfacetool til generering af mesh
		ArrayMesh a_mesh = new ArrayMesh();
		SurfaceTool st = new SurfaceTool();

		// Starter Surfacetool
		st.Begin(Mesh.PrimitiveType.Triangles);
		
		// Sætter seed
		perlinNoise.newSeed(seed);
		// For hver punkt i zSize+1
		for(int z = 0; z < zSize+1; z++)
		{
			// For hver punkt xSize+1
			for(int x = 0; x < xSize+1; x++)
			{


				// Bestemmer y-værdi til vertex (højde) ud fra Noise (16 oktaver for masser af detaljer)
				y = NoiseMAGIC(x*vertDistance,z*vertDistance, 16) * MULTIPLIER;
				
				// BESTEMMER BIOME
				if (y <= waterLevel )
				{	
					// FLOOR SURFACE VALUE
					y = waterLevel;


					// VERTEX IS WATER - SET COLOR TO BLUE
					image.SetPixel(x, z, new Color(0.0f, 0.0f, 1.0f, 1.0f));

				}

				if (y <= SANDLEVEL && y > waterLevel){image.SetPixel(x, z, new Color(1.0f, 1.0f, 0.0f, 1.0f));} // VERTEX IS SAND - SET COLOR TO YELLOW

				if (y <= ROCKLEVEL && y > GRASSLEVEL){image.SetPixel(x, z, new Color(0.5f, 0.5f, 0.5f, 1.0f));} // VERTEX IS ROCK - SET COLOR TO GREY

				if (y > ROCKLEVEL){image.SetPixel(x, z, new Color(1.0f, 1.0f, 1.0f, 1.0f));} // VERTEX IS SNOW - SET COLOR TO WHITE

				if (y <= GRASSLEVEL && y > SANDLEVEL) // VERTEX IS GRASS (Her skal der genereres træer)
				{
					// SET COLOR TO GREEN
					image.SetPixel(x, z, new Color(0.0f, 1.0f, 0.0f, 1.0f));



					
					// [------------------------------------------------------------------]
					// [------------------------- TRÆ GENERERING -------------------------]
					// [------------------------------------------------------------------]
					

					// Sætter seed for træ-noise
					//perlinNoise.newSeed(seed);
					
					// Bestemmer værdi for træ-noise (3 oktaver for at få mere simpel/udlignet støj)
					træNoise = NoiseMAGIC(x*vertDistance,z*vertDistance, 16);
					
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
						// Sætter seed for x-offset & Bestemmer random x offset for træ
						//perlinNoise.newSeed("feaugajdsnba");
						xOffset = (NoiseMAGIC(x*10000/(z*vertDistance*distanceToMountain+10),  z*10000/(x*vertDistance*distanceToMountain+10), 16)-0.5f)*2.5f;


						// Sætter seed for z-offset & Bestemmer random z offset for træ
						//perlinNoise.newSeed("fehvfe7s83");
						zOffset = (NoiseMAGIC(x*10000/(z*vertDistance*distanceToMountain+10),  z*10000/(x*vertDistance*distanceToMountain+10), 16)-0.5f)*2.5f;
						

						// Clamp værdier for x og z-offset
						xOffset = Mathf.Clamp(xOffset, -1, 1)*maxTræOffset;
						zOffset = Mathf.Clamp(zOffset, -1, 1)*maxTræOffset;

						
						Testværdi +=1;

						if(Testværdi > 5000)
						{
							GD.Print("xOffset: " + xOffset);
							GD.Print("zOffset: " + zOffset);
							GD.Print("");
							Testværdi = 0;
						}

						// Får ny y-værdi for træ fra samme Noise funktion, men med offset
						float y2 = NoiseMAGIC(x*vertDistance+xOffset,z*vertDistance+zOffset, 16) * MULTIPLIER;

						// [----------- Placerer Træ -----------]	
						
						// Laver træ som instans af treeScene
						træscene NytTræ = treeScene.Instantiate() as træscene;

						// Sætter position for træ
						NytTræ.Position = new Vector3(x*vertDistance+xOffset, y2, z*vertDistance+zOffset);

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
		GD.Print("Image saved");



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
	public float NoiseMAGIC(float x, float y, int okataver)
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
			val += perlinNoise._perlinNoise(x * frequency / GRID_SIZE, y * frequency / GRID_SIZE) * amplitude;

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
		foreach (Node child in træNode.GetChildren())
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
