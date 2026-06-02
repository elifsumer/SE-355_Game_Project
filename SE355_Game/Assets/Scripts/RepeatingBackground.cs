using System.Collections.Generic;
using UnityEngine;

public class RepeatingBackground : MonoBehaviour
{
	[SerializeField] private Camera targetCamera;
	[SerializeField] private float verticalOffset = 0f;
	[SerializeField] private float zPosition = 1f;
	[SerializeField] private int extraTiles = 2;

	private readonly List<Transform> tiles = new List<Transform>();
	private SpriteRenderer sourceRenderer;
	private float tileWidth;

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

	private void LateUpdate()
	{
		PositionTiles();
	}

	private void FitToCameraHeight()
	{
		float spriteHeight = sourceRenderer.sprite.bounds.size.y;
		float visibleHeight = targetCamera.orthographicSize * 2f;
		float scale = visibleHeight / spriteHeight;

		transform.localScale = new Vector3(scale, scale, 1f);
		tileWidth = sourceRenderer.sprite.bounds.size.x * scale;
	}

	private void CreateTiles()
	{
		float visibleWidth = targetCamera.orthographicSize * 2f * targetCamera.aspect;
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
		float startX = cameraX - (tiles.Count * tileWidth * 0.5f);
		startX = Mathf.Floor(startX / tileWidth) * tileWidth;

		for (int i = 0; i < tiles.Count; i++)
		{
			float yPosition = targetCamera.transform.position.y + verticalOffset -
				(sourceRenderer.sprite.bounds.center.y * transform.localScale.y);

			tiles[i].position = new Vector3(startX + (i * tileWidth), yPosition, zPosition);
		}
	}
}
