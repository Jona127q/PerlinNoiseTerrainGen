using Godot;
using System;
using PerlinNoiseImage;
using System.Threading;

public partial class node_2d : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Hello, World!");
		Thread.Sleep(1000);
		perlinNoiseImage.getPerlinNoiseImage();
		GD.Print("Done");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}