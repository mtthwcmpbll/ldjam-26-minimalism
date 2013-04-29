using UnityEngine;
using System.Collections;

public class Puzzle {

	public int Height { get; set; }
	public int Width { get; set; }
	
	public int TilePadding = 4;
	
	public Matrix<Tile> Tiles;
	public Matrix<Creature> Creatures;
	
	public int WidthInPixels {
		get {
			int calcedWidth = (Width * Tile.TILE_SIZE_IN_PIXELS) + (TilePadding * (Width+1));
			Debug.Log ("Puzzle Width in pixels: " + calcedWidth);
			return calcedWidth;
		}
	}
	
	public int HeightInPixels {
		get {
			int calcedHeight = (Height * Tile.TILE_SIZE_IN_PIXELS) + (TilePadding * (Height+1));
			Debug.Log ("Puzzle Height in pixels: " + calcedHeight);
			return calcedHeight;
		}
	}
}
