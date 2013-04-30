using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

public class PuzzleController : MonoBehaviour {
	
	public ToneMaker toneMaker;
	
	private enum SlideDirection { NONE, HORIZONTAL, VERTICAL }
	
	private Puzzle currentPuzzle;
	private FContainer puzzleContainer;
	private FContainer creaturesContainer;
	
	private FSprite fadeToWhite;
	
	// This holds the slidable columns or rows (depending on which direction the user is sliding things).
	private FContainer[] slidingContainers;
	
	// This holds whether we're currently in the middle of a slide.  If the value is NONE, the
	// there hasn't been a slide initiated yet and either direction can be started.  If it's
	// set to a direction, that's the only direction that can be moved in until the slide is released.
	private SlideDirection slideDirection = SlideDirection.NONE;
	
	// Holds a reference to the container we're currently sliding.  Only supports sliding one row/col at a time.
	private FContainer currentlySlidingContainer = null;
	private float currentlySlidingContainerStartPosition = -1.0f;
	private float currentlySlidingContainerLastPosition = -1.0f;
	private int currentlySlidingContainerIndex = -1;
	
	private int currentDifficulty = 3;
	private int maxDifficulty = 9;
	
	// Holds a complete copy of the currently-sliding row/col so we can "fake" the wrap-around as it slides
	private FContainer backCloneOfSlidingContainer = null;
	private FContainer frontCloneOfSlidingContainer = null;
	
	private Vector2 lastMousePosition = Vector2.zero;
	
	// Use this for initialization
	void Start () {
		FutileParams fparams = new FutileParams(false, false, true, true);

		fparams.AddResolutionLevel(480.0f, 1.0f, 1.0f, "");
		fparams.origin = new Vector2(0.5f, 0.5f);

		Futile.instance.Init(fparams);
		
		//set up Futile's camera so we can use our own camera, too
		Futile.instance.camera.clearFlags = CameraClearFlags.Nothing;
		Futile.instance.camera.cullingMask = 1 << 11;  //11 - the layer you want to put futile objects on

		Futile.atlasManager.LoadAtlas("Atlases/Sprites");
		//Futile.atlasManager.LoadFont("Domine", "Domine", "Atlases/Sprites", 0.0f, 0.0f);
		//Futile.atlasManager.LoadFont("DomineBold", "DomineBold", "Atlases/Sprites", 0.0f, 0.0f);
		
		InitPuzzle();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			//figure out which rows were touches
			float mouseXDelta = Input.mousePosition.x - lastMousePosition.x;
			float mouseYDelta = Input.mousePosition.y - lastMousePosition.y;
			
			if (slideDirection.Equals(SlideDirection.NONE)) {				
				if (Mathf.Abs(mouseXDelta) > Mathf.Abs(mouseYDelta)) {
					Debug.Log("Starting a horizontal slide");
					BeginRowSlide(mouseXDelta);
					
					bool horizontalCrossedRestPosition;
					bool verticalCrossedRestPosition;
					CheckForCrossedRestPosition(out horizontalCrossedRestPosition, out verticalCrossedRestPosition);
					if (horizontalCrossedRestPosition) {
						CheckRowForSolved();
					}
				} else if (Mathf.Abs(mouseYDelta) > Mathf.Abs(mouseXDelta)) {
					Debug.Log("Starting a vertical slide");
					BeginColumnSlide(mouseYDelta);
					
					bool horizontalCrossedRestPosition;
					bool verticalCrossedRestPosition;
					CheckForCrossedRestPosition(out horizontalCrossedRestPosition, out verticalCrossedRestPosition);
					if (verticalCrossedRestPosition) {
						CheckColumnForSolved();
					}
				}
			} else if (slideDirection.Equals(SlideDirection.HORIZONTAL)) {
				//continuing a horizontal slide
				currentlySlidingContainerLastPosition = currentlySlidingContainer.x;
				
				currentlySlidingContainer.x += mouseXDelta;
				backCloneOfSlidingContainer.x += mouseXDelta;
				frontCloneOfSlidingContainer.x += mouseXDelta;
				
				bool horizontalCrossedRestPosition;
				bool verticalCrossedRestPosition;
				CheckForCrossedRestPosition(out horizontalCrossedRestPosition, out verticalCrossedRestPosition);
				if (horizontalCrossedRestPosition) {
					CheckRowForSolved();
				}
			} else if (slideDirection.Equals(SlideDirection.VERTICAL)) {
				//continuing a vertical slide
				currentlySlidingContainerLastPosition = currentlySlidingContainer.y;
				
				currentlySlidingContainer.y += mouseYDelta;
				backCloneOfSlidingContainer.y += mouseYDelta;
				frontCloneOfSlidingContainer.y += mouseYDelta;
				
				bool horizontalCrossedRestPosition;
				bool verticalCrossedRestPosition;
				CheckForCrossedRestPosition(out horizontalCrossedRestPosition, out verticalCrossedRestPosition);
				if (verticalCrossedRestPosition) {
					CheckColumnForSolved();
				}
			}
		} else if (Input.GetMouseButtonUp(0) && !slideDirection.Equals(SlideDirection.NONE)) {
			Debug.Log ("Releasing a slide");
			
			bool horizontalCrossedRestPosition;
			bool verticalCrossedRestPosition;
			CheckForCrossedRestPosition(out horizontalCrossedRestPosition, out verticalCrossedRestPosition);
			
			if (slideDirection.Equals(SlideDirection.HORIZONTAL)) {
				Debug.Log("Completing a horizontal slide");
				EndRowSlide();
				
				CheckRowForSolved();
			} else if (slideDirection.Equals(SlideDirection.VERTICAL)) {
				Debug.Log("Completing a vertical slide");
				EndColumnSlide();
				
				CheckColumnForSolved();
			}
			
			//remove the old clone
			if (null != backCloneOfSlidingContainer) {
				puzzleContainer.RemoveChild(backCloneOfSlidingContainer);
				backCloneOfSlidingContainer = null;
			}
			if (null != frontCloneOfSlidingContainer) {
				puzzleContainer.RemoveChild(frontCloneOfSlidingContainer);
				frontCloneOfSlidingContainer = null;
			}
			
			currentlySlidingContainerIndex = -1;
			
			slideDirection = SlideDirection.NONE;
		}
		
		lastMousePosition = Input.mousePosition;
	}
	
	private void InitPuzzle() {
		if (null == fadeToWhite) {
			fadeToWhite = new FSprite("white");
			fadeToWhite.scale = 1000.0f;
			Futile.stage.AddChildAtIndex(fadeToWhite, 100);
		}
		
		if (null != currentPuzzle) {
			Futile.stage.RemoveChild(puzzleContainer);
			Futile.stage.RemoveChild(creaturesContainer);
			puzzleContainer = null;
			creaturesContainer = null;
			currentPuzzle = null;
		}
		
		currentPuzzle = PuzzleGenerator.CreateNewPuzzle(8, 8, currentDifficulty, 4 * currentDifficulty, false);
		
		//build the tile sprites
		puzzleContainer = new FContainer();
		for (int y=0; y < currentPuzzle.Height; y++) {
			for (int x=0; x < currentPuzzle.Width; x++) {
				Tile currTile = currentPuzzle.Tiles.GetValueAt(x, y);
				puzzleContainer.AddChild(currTile.TileSprite);
				currTile.TileSprite.x = (currentPuzzle.TilePadding * (x+1)) + (Tile.TILE_SIZE_IN_PIXELS * x) + (Tile.TILE_SIZE_IN_PIXELS/2);
				currTile.TileSprite.y = (currentPuzzle.TilePadding * (y+1)) + (Tile.TILE_SIZE_IN_PIXELS * y) + (Tile.TILE_SIZE_IN_PIXELS/2);
			}
		}
		
		//build the creature sprites
		creaturesContainer = new FContainer();
		for (int y=0; y < currentPuzzle.Height; y++) {
			for (int x=0; x < currentPuzzle.Width; x++) {
				Creature currCreature = currentPuzzle.Creatures.GetValueAt(x, y);
				if (null != currCreature) {
					creaturesContainer.AddChild(currCreature.TileSprite);
					currCreature.TileSprite.x = (currentPuzzle.TilePadding * (x+1)) + (Tile.TILE_SIZE_IN_PIXELS * x) + (Tile.TILE_SIZE_IN_PIXELS/2);
					currCreature.TileSprite.y = (currentPuzzle.TilePadding * (y+1)) + (Tile.TILE_SIZE_IN_PIXELS * y) + (Tile.TILE_SIZE_IN_PIXELS/2);
				}
			}
		}
		
		puzzleContainer.x = -1 * ((float)currentPuzzle.WidthInPixels / 2.0f);
		puzzleContainer.y = -1 * ((float)currentPuzzle.HeightInPixels / 2.0f);
		creaturesContainer.x = puzzleContainer.x;
		creaturesContainer.y = puzzleContainer.y;
		Futile.stage.AddChildAtIndex(puzzleContainer, 0);
		Futile.stage.AddChildAtIndex(creaturesContainer, 1);
		
		fadeToWhite.alpha = 1.0f;
		Go.to(fadeToWhite, 0.5f,
			new TweenConfig()
				.floatProp("alpha", 0.0f)
				.setEaseType(EaseType.SineIn));
	}
	
	private void CheckForCrossedRestPosition(out bool horizontalCrossedRestPosition, out bool verticalCrossedRestPosition) {
		//figure out which rows were touches
		float mouseXDelta = Input.mousePosition.x - lastMousePosition.x;
		float mouseYDelta = Input.mousePosition.y - lastMousePosition.y;
		
		float rowHeight = currentPuzzle.HeightInPixels / currentPuzzle.Height;
		float colWidth = currentPuzzle.WidthInPixels / currentPuzzle.Width;
		
		horizontalCrossedRestPosition = false;
		if (mouseXDelta > 0) {
			//sliding right
			horizontalCrossedRestPosition = (currentlySlidingContainer.x % colWidth) < (currentlySlidingContainerLastPosition % colWidth);
		} else if (mouseXDelta < 0) {
			//sliding left
			horizontalCrossedRestPosition = (currentlySlidingContainer.x % colWidth) > (currentlySlidingContainerLastPosition % colWidth);
		}
		verticalCrossedRestPosition = false;
		if (mouseYDelta > 0) {
			//sliding up
			verticalCrossedRestPosition = (currentlySlidingContainer.y % rowHeight) < (currentlySlidingContainerLastPosition % rowHeight);
		} else if (mouseYDelta < 0) {
			//sliding down
			verticalCrossedRestPosition = (currentlySlidingContainer.y % rowHeight) > (currentlySlidingContainerLastPosition % rowHeight);
		}
	}
	
	private void CheckRowForSolved() {
		//HOLY SHIT THIS IS SUPER HACKY: Temporarily make the slide to affect the datamodel, check for solved-ness, then slide back
		float colWidth = currentPuzzle.WidthInPixels / currentPuzzle.Width;
		int numberOfTilesSlid = Mathf.FloorToInt((currentlySlidingContainer.x + (colWidth/2)) / colWidth);
		currentPuzzle.Tiles.SlideRowBy(currentlySlidingContainerIndex, numberOfTilesSlid);
		
		if (currentPuzzle.AreCreaturesSolved()) {
			//the puzzle is solved!
			HandlePuzzleSolved();
		} else {
			//check to see if we solved anything in this row
			List<Vector2> creaturesInRow = currentPuzzle.GetCreaturesInRow(currentlySlidingContainerIndex);
			foreach (Vector2 creaturePosition in creaturesInRow) {
				Creature currCreature = currentPuzzle.Creatures.GetValueAt(creaturePosition);
				bool solved = currentPuzzle.isCreatureSolved(currCreature);
				
				if (solved) {
					//TODO: flash this creature and make its tone
					Debug.Log("This " + currCreature.CreatureType + " is solved!");
					toneMaker.PlayToneAt((int)creaturePosition.y);
				}
			}
		}
		
		//correct out hack
		currentPuzzle.Tiles.SlideRowBy(currentlySlidingContainerIndex, numberOfTilesSlid * -1);
	}
	
	private void CheckColumnForSolved() {
		//HOLY SHIT THIS IS SUPER HACKY: Temporarily make the slide to affect the datamodel, check for solved-ness, then slide back
		float rowHeight = currentPuzzle.HeightInPixels / currentPuzzle.Height;
		int numberOfTilesSlid = Mathf.FloorToInt((currentlySlidingContainer.y + (rowHeight/2)) / rowHeight);
		currentPuzzle.Tiles.SlideColumnBy(currentlySlidingContainerIndex, numberOfTilesSlid);
		
		if (currentPuzzle.AreCreaturesSolved()) {
			//the puzzle is solved!
			HandlePuzzleSolved();
		} else {
			//check to see if we solved anything in this row
			List<Vector2> creaturesInColumn = currentPuzzle.GetCreaturesInColumn(currentlySlidingContainerIndex);
			foreach (Vector2 creaturePosition in creaturesInColumn) {
				Creature currCreature = currentPuzzle.Creatures.GetValueAt(creaturePosition);
				bool solved = currentPuzzle.isCreatureSolved(currCreature);
				
				if (solved) {
					//TODO: flash this creature and make its tone
					Debug.Log("This " + currCreature.CreatureType + " is solved!");
					toneMaker.PlayToneAt((int)creaturePosition.y);
				}
			}
		}
		
		//correct our hack
		currentPuzzle.Tiles.SlideColumnBy(currentlySlidingContainerIndex, numberOfTilesSlid * -1);
	}
	
	private void HandlePuzzleSolved() {
		Debug.Log ("CONGRATS!  THIS PUZZLE IS SOLVED!");
		
		toneMaker.PlayFinale();
		
		currentDifficulty = Mathf.Min(currentDifficulty+2, maxDifficulty);
		
		fadeToWhite.alpha = 0.0f;
		Go.to(fadeToWhite, 0.5f,
			new TweenConfig()
				.floatProp("alpha", 1.0f)
				.setEaseType(EaseType.SineIn)
				.onComplete(thisTween => InitPuzzle() ));
	}
	
	private void BeginRowSlide(float mouseXDelta) {
		slideDirection = SlideDirection.HORIZONTAL;
		
		//restructure the puzzle into rows so we can slide some of theme around
		ReorganizeIntoRows();
		
		//figure out which row we're sliding
		float rowHeight = currentPuzzle.HeightInPixels / currentPuzzle.Height;
		Vector2 puzzleLocalPosition = puzzleContainer.ScreenToLocal(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		currentlySlidingContainerIndex = Mathf.FloorToInt(puzzleLocalPosition.y / rowHeight);
		Debug.Log("Starting to slide row " + currentlySlidingContainerIndex);
		currentlySlidingContainer = slidingContainers[currentlySlidingContainerIndex];
		currentlySlidingContainerStartPosition = currentlySlidingContainer.x;
		currentlySlidingContainerLastPosition = currentlySlidingContainer.x;
		
		//make a copy of the row and stick it on the front and the end of the row to "fake" wrap-around
		backCloneOfSlidingContainer = CopyRowOfSprites(currentlySlidingContainerIndex);
		frontCloneOfSlidingContainer = CopyRowOfSprites(currentlySlidingContainerIndex);
		
		//sliding right, put the clone on the left side of the row
		backCloneOfSlidingContainer.x = currentlySlidingContainer.x - (currentPuzzle.WidthInPixels - currentPuzzle.TilePadding);
		backCloneOfSlidingContainer.y = currentlySlidingContainer.y;

		//sliding left, put the clone on the right side of the row
		frontCloneOfSlidingContainer.x = currentlySlidingContainer.x + (currentPuzzle.WidthInPixels - currentPuzzle.TilePadding);
		frontCloneOfSlidingContainer.y = currentlySlidingContainer.y;
		
		puzzleContainer.AddChild(backCloneOfSlidingContainer);
		puzzleContainer.AddChild(frontCloneOfSlidingContainer);
		
		//actually move the row
		currentlySlidingContainer.x += mouseXDelta;
		backCloneOfSlidingContainer.x += mouseXDelta;
		frontCloneOfSlidingContainer.x += mouseXDelta;
	}
	
	private void EndRowSlide() {
		//perform the actual slide on the Puzzle's Tiles model
		float colWidth = currentPuzzle.WidthInPixels / currentPuzzle.Width;
		int numberOfTilesSlid = Mathf.FloorToInt((currentlySlidingContainer.x + (colWidth/2)) / colWidth);
		Debug.Log ("Number of tiles slid: " + numberOfTilesSlid);
		
		currentPuzzle.Tiles.SlideRowBy(currentlySlidingContainerIndex, numberOfTilesSlid);
		
		//shuffle the sprites around so the row container
		for (int col=0; col < currentPuzzle.Width; col++) {
			Tile currTile = currentPuzzle.Tiles.GetValueAt(col, currentlySlidingContainerIndex);
			currTile.TileSprite.x = (currentPuzzle.TilePadding * (col+1)) + (Tile.TILE_SIZE_IN_PIXELS * col) + (Tile.TILE_SIZE_IN_PIXELS/2);
		}
		
		currentlySlidingContainer.x = 0;
	}
	
	private void BeginColumnSlide(float mouseYDelta) {
		slideDirection = SlideDirection.VERTICAL;
		
		//restructure the puzzle into rows so we can slide some of theme around
		ReorganizeIntoColumns();
		
		//figure out which column we're sliding
		float colWidth = currentPuzzle.WidthInPixels / currentPuzzle.Width;
		Vector2 puzzleLocalPosition = puzzleContainer.ScreenToLocal(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		currentlySlidingContainerIndex = Mathf.FloorToInt(puzzleLocalPosition.x / colWidth);
		Debug.Log("Starting to slide column " + currentlySlidingContainerIndex);
		currentlySlidingContainer = slidingContainers[currentlySlidingContainerIndex];
		currentlySlidingContainerStartPosition = currentlySlidingContainer.y;
		currentlySlidingContainerLastPosition = currentlySlidingContainer.y;
		
		//make a copy of the column and stick it on the top or bottom of this column to "fake" wrap-around
		backCloneOfSlidingContainer = CopyColumnOfSprites(currentlySlidingContainerIndex);
		frontCloneOfSlidingContainer = CopyColumnOfSprites(currentlySlidingContainerIndex);
		
		//sliding up, put the clone on the bottom
		backCloneOfSlidingContainer.x = currentlySlidingContainer.x;
		backCloneOfSlidingContainer.y = currentlySlidingContainer.y - (currentPuzzle.HeightInPixels - currentPuzzle.TilePadding);
		
		//sliding down, put the clone on the top
		frontCloneOfSlidingContainer.x = currentlySlidingContainer.x;
		frontCloneOfSlidingContainer.y = currentlySlidingContainer.y + (currentPuzzle.HeightInPixels - currentPuzzle.TilePadding);
		
		puzzleContainer.AddChild(backCloneOfSlidingContainer);
		puzzleContainer.AddChild(frontCloneOfSlidingContainer);
		
		//actually move the row
		currentlySlidingContainer.y += mouseYDelta;
	}
	
	private void EndColumnSlide() {
		//perform the actual slide on the Puzzle's Tiles model
		float rowHeight = currentPuzzle.HeightInPixels / currentPuzzle.Height;
		int numberOfTilesSlid = Mathf.FloorToInt((currentlySlidingContainer.y + (rowHeight/2)) / rowHeight);
		Debug.Log ("Number of tiles slid: " + numberOfTilesSlid);
		
		currentPuzzle.Tiles.SlideColumnBy(currentlySlidingContainerIndex, numberOfTilesSlid);
		
		//shuffle the sprites around so the row container
		for (int row=0; row < currentPuzzle.Height; row++) {
			Tile currTile = currentPuzzle.Tiles.GetValueAt(currentlySlidingContainerIndex, row);
			currTile.TileSprite.y = (currentPuzzle.TilePadding * (row+1)) + (Tile.TILE_SIZE_IN_PIXELS * row) + (Tile.TILE_SIZE_IN_PIXELS/2);
		}
		
		currentlySlidingContainer.y = 0;
	}
	
	private FContainer CopyRowOfSprites(int row) {
		FContainer copy = new FContainer();
		
		for (int col=0; col < currentPuzzle.Width; col++) {
			Tile tileToCopy = currentPuzzle.Tiles.GetValueAt(col, row);
			FSprite copiedTileSprite = tileToCopy.GetSpriteRepresentation();
			copiedTileSprite.x = tileToCopy.TileSprite.x;
			copiedTileSprite.y = tileToCopy.TileSprite.y;
			copy.AddChild(copiedTileSprite);
		}
		
		return copy;
	}
	
	private FContainer CopyColumnOfSprites(int col) {
		FContainer copy = new FContainer();
		
		for (int row=0; row < currentPuzzle.Height; row++) {
			Tile tileToCopy = currentPuzzle.Tiles.GetValueAt(col, row);
			FSprite copiedTileSprite = tileToCopy.GetSpriteRepresentation();
			copiedTileSprite.x = tileToCopy.TileSprite.x;
			copiedTileSprite.y = tileToCopy.TileSprite.y;
			copy.AddChild(copiedTileSprite);
		}
		
		return copy;
	}
	
	private void ReorganizeIntoFlat() {
		//detach any tile sprites from the sliding containers
		foreach (FContainer container in slidingContainers) {
			container.RemoveAllChildren();
		}
		slidingContainers = null;
		
		for (int row=0; row < currentPuzzle.Height; row++) {
			//copy the tile FSprites into this new row container
			for (int col=0; col < currentPuzzle.Width; col++) {
				Tile currTile = currentPuzzle.Tiles.GetValueAt(col, row);
				
				//add it to the flat puzzle container
				puzzleContainer.AddChild(currTile.TileSprite);
			}
		}
	}
	
	private void ReorganizeIntoRows() {
		slidingContainers = new FContainer[currentPuzzle.Height];
		
		for (int row=0; row < currentPuzzle.Height; row++) {
			FContainer currRowContainer = new FContainer();
			
			//copy the tile FSprites into this new row container
			for (int col=0; col < currentPuzzle.Width; col++) {
				Tile currTile = currentPuzzle.Tiles.GetValueAt(col, row);
				
				//remove this tile's sprite from the general puzzle container
				puzzleContainer.RemoveChild(currTile.TileSprite);
				
				//add it to this row container
				currRowContainer.AddChild(currTile.TileSprite);
			}
			
			//add the row container to the puzzle container
			slidingContainers[row] = currRowContainer;
			puzzleContainer.AddChild(currRowContainer);
		}
	}
	
	private void ReorganizeIntoColumns() {
		slidingContainers = new FContainer[currentPuzzle.Width];
		
		for (int col=0; col < currentPuzzle.Width; col++) {
			FContainer currColContainer = new FContainer();
			
			//copy the tile FSprites into this new row container
			for (int row=0; row < currentPuzzle.Height; row++) {
				Tile currTile = currentPuzzle.Tiles.GetValueAt(col, row);
				
				//remove this tile's sprite from the general puzzle container
				puzzleContainer.RemoveChild(currTile.TileSprite);
				
				//add it to this row container
				currColContainer.AddChild(currTile.TileSprite);
			}
			
			//add the row container to the puzzle container
			slidingContainers[col] = currColContainer;
			puzzleContainer.AddChild(currColContainer);
		}
	}
	
}
