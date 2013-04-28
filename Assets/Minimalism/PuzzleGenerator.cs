using UnityEngine;
using System;
using System.Collections;

public class PuzzleGenerator {

	public static Puzzle CreateNewPuzzle(int width, int height) {
		Puzzle p = new Puzzle();
		
		p.Width = width;
		p.Height = height;
		
		p.Tiles = new Tile[width*height];
		
		System.Array groundTypes = Enum.GetValues(typeof(Tile.TileGroundType));
		System.Random rnd = new System.Random();
		for (int y=0; y < height; y++) {
			for (int x=0; x < width; x++) {
				Tile.TileGroundType randomGroundType = (Tile.TileGroundType)Enum.GetValues(typeof(Tile.TileGroundType)).GetValue(rnd.Next(groundTypes.Length));
				Tile newTile = new Tile(randomGroundType);
				p.Tiles[(y*width)+x] = newTile;
			}
		}
		
		return p;
	}
}
