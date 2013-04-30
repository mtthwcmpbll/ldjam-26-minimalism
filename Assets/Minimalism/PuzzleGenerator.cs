using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PuzzleGenerator {

	public static Puzzle CreateNewPuzzle(
		int width,
		int height,
		int numberOfAnimals,
		int numberOfSlides,
		bool includeSwamps) {
		
		Puzzle p = new Puzzle();
		
		p.Width = width;
		p.Height = height;
		
		p.Tiles = new Matrix<Tile>(width, height);
		p.Creatures = new Matrix<Creature>(width, height);
		
		//collect all the possible ground types
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
				p.Tiles.SetValueAt(x, y, newTile);
			}
		}
		
		//collect all the possible creature types
		List<Creature.TileCreatureType> creatureTypes = new List<Creature.TileCreatureType>();
		foreach (object ct in Enum.GetValues(typeof(Creature.TileCreatureType))) {
			creatureTypes.Add((Creature.TileCreatureType)ct);
		}
		
		//place all the creatures for this puzzle
		List<Vector2> creaturePositions = new List<Vector2>();
		for (int i=0; i < numberOfAnimals; i++) {
			int randomX = rnd.Next(width);
			int randomY = rnd.Next (height);
			Vector2 randomPosition = new Vector2(randomX, randomY);
			
			//make sure we haven't used this position yet
			while (creaturePositions.Contains(randomPosition)) {
				randomX = rnd.Next(width);
				randomY = rnd.Next(height);
				randomPosition = new Vector2(randomX, randomY);
			}
			
			Debug.Log ("Making a creature at " + randomPosition);
			
			//get the ground type at this space
			Tile selectedTile = p.Tiles.GetValueAt(randomX, randomY);
			Creature newCreature = Creature.GetRandomCreatureForTile(selectedTile);
			p.Creatures.SetValueAt(randomX, randomY, newCreature);
			
			creaturePositions.Add (randomPosition);
		}
		
		//make some random slides to create the puzzle
		//first slide each of the animal rows/cols
		foreach (Vector2 cp in creaturePositions) {
			Tile.TileGroundType originalType = p.Tiles.GetValueAt((int)cp.x, (int)cp.y).GroundType;
			RandomlySlideTile(p, (int)cp.x, (int)cp.y, rnd);
		}
		//then randomly slide other columns to finish
		for (int i=0; i < numberOfSlides-numberOfAnimals; i++) {
			int randomX = rnd.Next(width);
			int randomY = rnd.Next (height);
			RandomlySlideTile(p, randomX, randomY, rnd);
		}
		
		return p;
	}
	
	private static void RandomlySlideTile(Puzzle p, int row, int col, System.Random rnd) {
		List<Vector2> creaturePositions = null;
		List<int> possibleDistances = null;
		
		int direction = rnd.Next(4);
		switch (direction) {
		case 0: //UP
			creaturePositions = p.GetCreaturesInColumn(col);
			possibleDistances = new List<int>();
			for (int i=1; i < p.Height-1; i++) {
				possibleDistances.Add(i);
			}
			SlideColumnHelper(p, col, creaturePositions, possibleDistances, rnd);
			break;
		case 1: //RIGHT
			creaturePositions = p.GetCreaturesInRow(row);
			possibleDistances = new List<int>();
			for (int i=1; i < p.Width-1; i++) {
				possibleDistances.Add(i);
			}
			SlideRowHelper(p, row, creaturePositions, possibleDistances, rnd);
			break;
		case 2: //DOWN
			creaturePositions = p.GetCreaturesInColumn(col);
			possibleDistances = new List<int>();
			for (int i=1; i < p.Height-1; i++) {
				possibleDistances.Add(i * -1);
			}
			SlideColumnHelper(p, col, creaturePositions, possibleDistances, rnd);
			break;
		case 3: //LEFT
			creaturePositions = p.GetCreaturesInRow(row);
			possibleDistances = new List<int>();
			for (int i=1; i < p.Width-1; i++) {
				possibleDistances.Add(i * -1);
			}
			SlideRowHelper(p, row, creaturePositions, possibleDistances, rnd);
			break;
		}
	}
	
	private static void SlideColumnHelper(Puzzle p, int col, List<Vector2> creaturePositions, List<int> possibleDistances, System.Random rnd) {
		//randomly select a possible distance and check that all creatures are still unsolved
		int selectedDistance = rnd.Next(possibleDistances.Count);
		int distance = possibleDistances[selectedDistance];
		p.Tiles.SlideColumnBy(col, distance); //make the slide
		//test that it's ok
		while (!p.AreCreaturesUnsolved(creaturePositions)) {
			Debug.Log ("Attempting distance " + distance);
			
			//if not, slide back
			p.Tiles.SlideColumnBy(col, distance * -1);
			//remove the bad position
			possibleDistances.RemoveAt(selectedDistance);
			//and try again
			selectedDistance = rnd.Next(possibleDistances.Count);
			distance = possibleDistances[selectedDistance];
			p.Tiles.SlideColumnBy(col, distance); //try another slide
		}
		Debug.Log("Sliding column by " + distance);
	}
	
	private static void SlideRowHelper(Puzzle p, int row, List<Vector2> creaturePositions, List<int> possibleDistances, System.Random rnd) {
		//randomly select a possible distance and check that all creatures are still unsolved
		int selectedDistance = rnd.Next(possibleDistances.Count);
		int distance = possibleDistances[selectedDistance];
		p.Tiles.SlideRowBy(row, distance); //make the slide
		//test that it's ok
		while (!p.AreCreaturesUnsolved(creaturePositions)) {
			//if not, slide back
			p.Tiles.SlideRowBy(row, distance * -1);
			//remove the bad position
			possibleDistances.RemoveAt(selectedDistance);
			//and try again
			selectedDistance = rnd.Next(possibleDistances.Count);
			distance = possibleDistances[selectedDistance];
			p.Tiles.SlideRowBy(row, distance); //try another slide
		}
		Debug.Log("Sliding row by " + distance);
	}
	
}
