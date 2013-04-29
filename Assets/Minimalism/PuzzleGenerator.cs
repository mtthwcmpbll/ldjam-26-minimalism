using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PuzzleGenerator {

	public static Puzzle CreateNewPuzzle(
		int width,
		int height,
		int numberOfAnimals,
		bool includeSwamps) {
		
		Puzzle p = new Puzzle();
		
		p.Width = width;
		p.Height = height;
		
		p.Tiles = new Tile[width*height];
		p.Animals = new Tile[width*height];
		
		List<Tile.TileGroundType> groundTypes = new List<Tile.TileGroundType>();
		foreach (object gt in Enum.GetValues(typeof(Tile.TileGroundType))) {
			groundTypes.Add((Tile.TileGroundType)gt);
		}
		if (!includeSwamps) {
			groundTypes.Remove(Tile.TileGroundType.SWAMP);
		}
		System.Random rnd = new System.Random();
		for (int y=0; y < height; y++) {
			for (int x=0; x < width; x++) {
				Tile.TileGroundType randomGroundType = groundTypes[rnd.Next(groundTypes.Count)];
				Tile newTile = new Tile(randomGroundType);
				p.Tiles[(y*width)+x] = newTile;
			}
		}
		
		return p;
	}
}
