using Godot;
using System;
using PerlinNoiseImage;
using System.Threading;

public partial class node_2d : Node2D
{

	public uint[] seeds;
	public int sizeX = 400;
	public int sizeY = 400;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Hello, World!");
		Thread.Sleep(1000);
		perlinNoiseImage.getPerlinNoiseImage(sizeX, sizeY, seeds);
		GD.Print("Done");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
