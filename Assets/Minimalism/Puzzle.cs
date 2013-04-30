using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Puzzle {

	public int Height { get; set; }
	public int Width { get; set; }
	
	public int TilePadding = 4;
	
	public Matrix<Tile> Tiles;
	public Matrix<Creature> Creatures;
	
	public int WidthInPixels {
		get {
			int calcedWidth = (Width * Tile.TILE_SIZE_IN_PIXELS) + (TilePadding * (Width+1));
			return calcedWidth;
		}
	}
	
	public int HeightInPixels {
		get {
			int calcedHeight = (Height * Tile.TILE_SIZE_IN_PIXELS) + (TilePadding * (Height+1));
			return calcedHeight;
		}
	}
	
	public bool AreCreaturesSolved() {
		List<Creature> creatures = Creatures.getValues();
		foreach (Creature c in creatures) {
			if (!isCreatureSolved(c)) {
				return false;
			}
		}
		
		return true;
	}
	
	public bool isCreatureSolved(Creature c) {
		Vector2 creaturePosition;
		if (Creatures.getPosition(c, out creaturePosition)) {
			if (Tiles.GetValueAt(creaturePosition).GroundType.Equals(c.HomeGround)) {
				return true;
			}
		}
		return false;
	}
	
	//TODO: this method is inconsistent, but I don't have time to fix it right now
	public bool AreCreaturesUnsolved(List<Vector2> creaturePositions) {
		foreach (Vector2 cp in creaturePositions) {
			if (Tiles.GetValueAt(cp).GroundType.Equals(Creatures.GetValueAt(cp).HomeGround)) {
				return false;
			}
		}
		return true;
	}
	
	public List<Vector2> GetCreaturesInRow(int row) {
		List<Vector2> creaturePositions = new List<Vector2>();
		for (int col=0; col < Width; col++) {
			if (null != Creatures.GetValueAt(col, row)) {
				creaturePositions.Add(new Vector2(col, row));
			}
		}
		return creaturePositions;
	}
	
	public List<Vector2> GetCreaturesInColumn(int col) {
		List<Vector2> creaturePositions = new List<Vector2>();
		for (int row=0; row < Height; row++) {
			if (null != Creatures.GetValueAt(col, row)) {
				creaturePositions.Add(new Vector2(col, row));
			}
		}
		return creaturePositions;
	}
	
}
