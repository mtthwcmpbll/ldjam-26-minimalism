using UnityEngine;
using System.Collections;

public class PuzzleController : MonoBehaviour {
	
	private enum SlideDirection { NONE, HORIZTONAL, VERTICAL }
	
	private Puzzle currentPuzzle;
	private FContainer puzzleContainer;
	
	// This holds the slidable columns or rows (depending on which direction the user is sliding things).
	private FContainer[] slidingContainers;
	
	// This holds whether we're currently in the middle of a slide.  If the value is NONE, the
	// there hasn't been a slide initiated yet and either direction can be started.  If it's
	// set to a direction, that's the only direction that can be moved in until the slide is released.
	private SlideDirection slideDirection = SlideDirection.NONE;
	
	// Holds a reference to the container we're currently sliding.  Only supports sliding one row/col at a time.
	private FContainer currentlySlidingContainer = null;
	private int currentlySlidingContainerIndex = -1;
	
	// Holds a complete copy of the currently-sliding row/col so we can "fake" the wrap-around as it slides
	private FContainer cloneOfSlidingContainer = null;
	
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
		
		currentPuzzle = PuzzleGenerator.CreateNewPuzzle(8, 8);
		puzzleContainer = new FContainer();
		
		for (int y=0; y < currentPuzzle.Height; y++) {
			for (int x=0; x < currentPuzzle.Width; x++) {
				Tile currTile = currentPuzzle.GetTileAt(x, y);
				puzzleContainer.AddChild(currTile.TileSprite);
				currTile.TileSprite.x = (currentPuzzle.TilePadding * (x+1)) + (Tile.TILE_SIZE_IN_PIXELS * x) + (Tile.TILE_SIZE_IN_PIXELS/2);
				currTile.TileSprite.y = (currentPuzzle.TilePadding * (y+1)) + (Tile.TILE_SIZE_IN_PIXELS * y) + (Tile.TILE_SIZE_IN_PIXELS/2);
			}
		}
		
		puzzleContainer.x = -1 * ((float)currentPuzzle.WidthInPixels / 2.0f);
		puzzleContainer.y = -1 * ((float)currentPuzzle.HeightInPixels / 2.0f);
		Futile.stage.AddChild(puzzleContainer);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			//figure out which rows were touches
			
			if (slideDirection.Equals(SlideDirection.NONE)) {
				float mouseXDelta = Input.mousePosition.x - lastMousePosition.x;
				float mouseYDelta = Input.mousePosition.y - lastMousePosition.y;
				
				Debug.Log("Mouse deltas: " + new Vector2(mouseXDelta, mouseYDelta));
				
				if (Mathf.Abs(mouseXDelta) > Mathf.Abs(mouseYDelta)) {
					BeginRowSlide(mouseXDelta, mouseYDelta);
				} else if (Mathf.Abs(mouseYDelta) > Mathf.Abs(mouseXDelta)) {
					Debug.Log("Starting a vertical slide");
					slideDirection = SlideDirection.VERTICAL;
					//TODO
				}
			} else if (slideDirection.Equals(SlideDirection.HORIZTONAL)) {
				float mouseXDelta = Input.mousePosition.x - lastMousePosition.x;
				currentlySlidingContainer.x += mouseXDelta;
				cloneOfSlidingContainer.x += mouseXDelta;
			}
		} else if (Input.GetMouseButtonUp(0) && !slideDirection.Equals(SlideDirection.NONE)) {
			Debug.Log ("Releasing a slide");
			
			if (slideDirection.Equals(SlideDirection.HORIZTONAL)) {
				//perform the actual slide on the Puzzle's Tiles model
				float colWidth = currentPuzzle.WidthInPixels / currentPuzzle.Width;
				int numberOfTilesSlid = Mathf.FloorToInt(currentlySlidingContainer.x / colWidth);
				Debug.Log ("Number of tiles slid: " + numberOfTilesSlid);
				
				currentPuzzle.SlideRowBy(currentlySlidingContainerIndex, numberOfTilesSlid);
				
				//shuffle the sprites around so the row container
				for (int col=0; col < currentPuzzle.Width; col++) {
					Tile currTile = currentPuzzle.GetTileAt(col, currentlySlidingContainerIndex);
					currTile.TileSprite.x = (currentPuzzle.TilePadding * (col+1)) + (Tile.TILE_SIZE_IN_PIXELS * col) + (Tile.TILE_SIZE_IN_PIXELS/2);
				}
				
				currentlySlidingContainer.x = 0;
			}
			
			//remove the old clone
			if (null != cloneOfSlidingContainer) {
				puzzleContainer.RemoveChild(cloneOfSlidingContainer);
				cloneOfSlidingContainer = null;
			}
			
			currentlySlidingContainerIndex = -1;
			
			slideDirection = SlideDirection.NONE;
		}
		
		lastMousePosition = Input.mousePosition;
	}
	
	private void BeginRowSlide(float mouseXDelta, float mouseYDelta) {
		Debug.Log("Starting a horizontal slide");
		slideDirection = SlideDirection.HORIZTONAL;
		
		//restructure the puzzle into rows so we can slide some of theme around
		ReorganizeIntoRows();
		
		//figure out which row we're sliding
		float rowHeight = currentPuzzle.HeightInPixels / currentPuzzle.Height;
		Vector2 puzzleLocalPosition = puzzleContainer.ScreenToLocal(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		currentlySlidingContainerIndex = Mathf.FloorToInt(puzzleLocalPosition.y / rowHeight);
		Debug.Log("Starting to slide row " + currentlySlidingContainerIndex);
		currentlySlidingContainer = slidingContainers[currentlySlidingContainerIndex];
		
		//make a copy of the row and stick it on the front or the end of the row to "fake" wrap-around
		cloneOfSlidingContainer = new FContainer();
		for (int col=0; col < currentPuzzle.Width; col++) {
			Tile tileToCopy = currentPuzzle.GetTileAt(col, currentlySlidingContainerIndex);
			FSprite copiedTileSprite = tileToCopy.GetSpriteRepresentation();
			copiedTileSprite.x = tileToCopy.TileSprite.x;
			copiedTileSprite.y = tileToCopy.TileSprite.y;
			cloneOfSlidingContainer.AddChild(copiedTileSprite);
		}
		if (mouseXDelta > 0) {
			//sliding right, put the clone on the left side of the row
			cloneOfSlidingContainer.x = currentlySlidingContainer.x - (currentPuzzle.WidthInPixels - currentPuzzle.TilePadding);
			cloneOfSlidingContainer.y = currentlySlidingContainer.y;
		} else if (mouseXDelta < 0) {
			//sliding left, put the clone on the right side of the row
			cloneOfSlidingContainer.x = currentlySlidingContainer.x + (currentPuzzle.WidthInPixels - currentPuzzle.TilePadding);
			cloneOfSlidingContainer.y = currentlySlidingContainer.y;
		}
		puzzleContainer.AddChild(cloneOfSlidingContainer);
		
		//actually move the row
		currentlySlidingContainer.x += mouseXDelta;
	}
	
	private void ReorganizeIntoRows() {
		slidingContainers = new FContainer[currentPuzzle.Height];
		
		for (int row=0; row < currentPuzzle.Height; row++) {
			FContainer currRowContainer = new FContainer();
			
			//copy the tile FSprites into this new row container
			for (int col=0; col < currentPuzzle.Width; col++) {
				Tile currTile = currentPuzzle.GetTileAt(col, row);
				
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
}
