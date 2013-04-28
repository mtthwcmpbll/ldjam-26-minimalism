using UnityEngine;
using System.Collections;

public class Puzzle {

	public int Height { get; set; }
	public int Width { get; set; }
	
	public int TilePadding = 4;
	
	public Tile[] Tiles;
	
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
	
	public Tile GetTileAt(int col, int row) {
		return Tiles[(row*Height)+col];
	}
	
	private void SetTileAt(int col, int row, Tile tile) {
		Tiles[(row*Height)+col] = tile;
	}
	
	public Tile[] GetRow(int rowIndex) {
		Tile[] row = new Tile[Width];
		for (int col=0; col < Width; col++) {
			row[col] = GetTileAt(col, rowIndex);
		}
		return row;
	}
	
	public void SlideRowBy(int rowIndex, int tiles) {
		Tile[] oldRow = GetRow(rowIndex);
		for (int col=0; col < Width; col++) {
			int oldPosition = col - tiles;
			while (oldPosition < 0) {
				oldPosition += Width;
			}
			while (oldPosition >= Width) {
				oldPosition -= Width;
			}
			
			Debug.Log ("Old position: " + oldPosition);
			Debug.Log ("New position: " + col);
			SetTileAt(col, rowIndex, oldRow[oldPosition]);
		}
	}
}
