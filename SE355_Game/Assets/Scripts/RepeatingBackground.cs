using System.Collections.Generic;
using UnityEngine;

public class RepeatingBackground : MonoBehaviour
{
	[SerializeField] private Camera targetCamera;
	[SerializeField] private float verticalOffset = 0f;
	[SerializeField] private float zPosition = 1f;
	[SerializeField] private int extraTiles = 6;
	[SerializeField, Range(0f, 1f)] private float parallaxEffect = 0.3f;

	[Header("Menu Auto-Scroll")]
	[SerializeField] private float menuScrollSpeed = 3f;

	private readonly List<Transform> tiles = new List<Transform>();
	private SpriteRenderer sourceRenderer;
	private float tileWidth;
	private float visibleHalfWidth;

	/// <summary>
	/// An offset subtracted from the effective camera X position when the game
	/// is in a menu state. Incremented each frame using unscaled time
	/// so the background scrolls left-to-right even while Time.timeScale is 0.
	/// </summary>
	private float menuScrollOffset;
	private GameModeManager gameManager;

	private void Awake()
	{
		sourceRenderer = GetComponent<SpriteRenderer>();
		if (targetCamera == null)
		{
			targetCamera = Camera.main;
		}

		if (sourceRenderer == null || sourceRenderer.sprite == null || targetCamera == null)
		{
			enabled = false;
			return;
		}

		transform.SetParent(null, false);
		FitToCameraHeight();
		CreateTiles();
	}

	private void Start()
	{
		gameManager = FindFirstObjectByType<GameModeManager>();
	}

	private void LateUpdate()
	{
		// While in a menu, accumulate a scroll offset using unscaled delta time
		// so the background keeps sliding even when Time.timeScale is 0.
		if (gameManager != null && gameManager.IsInMenu)
		{
			menuScrollOffset += menuScrollSpeed * Time.unscaledDeltaTime;
		}

		PositionTiles();
	}

	private void FitToCameraHeight()
	{
		float spriteHeight = sourceRenderer.sprite.bounds.size.y;
		float visibleHeight = targetCamera.orthographicSize * 2f;
		float scale = visibleHeight / spriteHeight;

		transform.localScale = new Vector3(scale, scale, 1f);
		tileWidth = sourceRenderer.sprite.bounds.size.x * scale;
		visibleHalfWidth = targetCamera.orthographicSize * targetCamera.aspect;
	}

	private void CreateTiles()
	{
		float visibleWidth = visibleHalfWidth * 2f;
		int tileCount = Mathf.CeilToInt(visibleWidth / tileWidth) + extraTiles;

		tiles.Add(transform);

		for (int i = 1; i < tileCount; i++)
		{
			GameObject tile = new GameObject($"{name} Tile {i}");
			SpriteRenderer tileRenderer = tile.AddComponent<SpriteRenderer>();
			tileRenderer.sprite = sourceRenderer.sprite;
			tileRenderer.color = sourceRenderer.color;
			tileRenderer.material = sourceRenderer.sharedMaterial;
			tileRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
			tileRenderer.sortingOrder = sourceRenderer.sortingOrder;

			tile.transform.rotation = transform.rotation;
			tile.transform.localScale = transform.localScale;
			tile.name = $"{name} Tile {i}";
			tiles.Add(tile.transform);
		}

		PositionTiles();
	}

	private void PositionTiles()
	{
		float cameraX = targetCamera.transform.position.x;
		float parallaxOffset = cameraX * parallaxEffect;
		float effectiveCameraX = cameraX - parallaxOffset;

		// Start tiles 3 tile-widths before the left edge of the visible area
		// so they are fully placed before the camera can ever see them
		float leftEdge = effectiveCameraX - visibleHalfWidth;
		float startX = Mathf.Floor((leftEdge - tileWidth * 3f) / tileWidth) * tileWidth;

		// Apply the menu scroll as a visual offset that wraps around tileWidth.
		// The modulo keeps it within one tile so tiles always cover the viewport,
		// creating a seamless infinite scroll illusion.
		float visualShift = menuScrollOffset % tileWidth;

		for (int i = 0; i < tiles.Count; i++)
		{
			float yPosition = targetCamera.transform.position.y + verticalOffset -
				(sourceRenderer.sprite.bounds.center.y * transform.localScale.y);

			tiles[i].position = new Vector3(startX + (i * tileWidth) + parallaxOffset + visualShift, yPosition, zPosition);
		}
	}
}
