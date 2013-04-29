using UnityEngine;
using System.Collections;

public class Puzzle {

	public int Height { get; set; }
	public int Width { get; set; }
	
	public int TilePadding = 4;
	
	public Tile[] Tiles;
	public Tile[] Animals;
	
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
	
	public Tile[] GetColumn(int colIndex) {
		Tile[] col = new Tile[Height];
		for (int row=0; row < Height; row++) {
			col[row] = GetTileAt(colIndex, row);
		}
		return col;
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
	
	// positive values slide up, negative slide down
	public void SlideColumnBy(int colIndex, int tiles) {
		Tile[] oldColumn = GetColumn(colIndex);
		for (int row=0; row < Height; row++) {
			int oldPosition = row - tiles;
			while (oldPosition < 0) {
				oldPosition += Height;
			}
			while (oldPosition >= Height) {
				oldPosition -= Height;
			}
			
			Debug.Log ("Old position: " + oldPosition);
			Debug.Log ("New position: " + row);
			SetTileAt(colIndex, row, oldColumn[oldPosition]);
		}
	}
}
