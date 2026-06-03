using System.Collections.Generic;
using UnityEngine;

public class BarrierSpawner : MonoBehaviour
{
	[SerializeField] private BarrierPair barrierPairPrefab;
	[SerializeField] private Transform player;
	[SerializeField] private Camera targetCamera;

	[Header("Spawn Distances")]
	[SerializeField] private float spawnAheadDistance = 45f;
	[SerializeField] private float firstSpawnDistance = 28f;
	[SerializeField] private float cleanupBehindDistance = 35f;

	[Header("Barrier Defaults")]
	[SerializeField] private float spacing = 15f;
	[SerializeField] private float minY = -14f;
	[SerializeField] private float maxY = 14f;
	[SerializeField] private float gapSize = 12f;
	[SerializeField] private float barrierWidth = 4.5f;
	[SerializeField] private float gapVerticalMargin = 2f;

	[Header("Pattern Settings")]
	[SerializeField] private float patternBreatherGap = 8f;
	[SerializeField, Range(0f, 1f)] private float corridorChance = 0.3f;
	[SerializeField, Range(0f, 1f)] private float staircaseChance = 0.25f;

	private readonly List<BarrierPair> spawnedPairs = new List<BarrierPair>();
	private float nextSpawnX;
	private bool hasInitializedSpawn = false;

	private float MinGapCenter => minY + gapSize * 0.5f + gapVerticalMargin;
	private float MaxGapCenter => maxY - gapSize * 0.5f - gapVerticalMargin;

	private GameModeManager gameManager;

	private void Start()
	{
		if (targetCamera == null)
		{
			targetCamera = Camera.main;
		}

		if (player == null || barrierPairPrefab == null)
		{
			enabled = false;
			return;
		}

		gameManager = FindFirstObjectByType<GameModeManager>();

		// Don't spawn barriers during the menu — wait until the game is playing
	}

	private void Update()
	{
		// Only spawn barriers while the game is actually being played
		if (gameManager != null && gameManager.CurrentState != GameModeManager.GameState.Playing)
			return;

		// First time we enter Playing state, set up the initial spawn position
		if (!hasInitializedSpawn)
		{
			nextSpawnX = player.position.x + firstSpawnDistance;
			hasInitializedSpawn = true;
		}

		SpawnUntilAhead();
		CleanupOldPairs();
	}

	private void SpawnUntilAhead()
	{
		float targetX = player.position.x + spawnAheadDistance;

		while (nextSpawnX <= targetX)
		{
			SpawnNextPattern();
		}
	}

	/// <summary>
	/// Randomly selects and spawns one of the available barrier patterns.
	/// </summary>
	private void SpawnNextPattern()
	{
		float roll = Random.value;

		if (roll < corridorChance)
		{
			SpawnCorridorPattern();
		}
		else if (roll < corridorChance + staircaseChance)
		{
			SpawnStaircasePattern();
		}
		else
		{
			// Standard single barrier with random gap position
			SpawnBarrier(nextSpawnX, RandomGapCenter());
			nextSpawnX += spacing;
		}
	}

	/// <summary>
	/// Spawns a corridor of barriers with alternating high/low gaps.
	/// Forces the player to flip gravity repeatedly to weave through.
	/// </summary>
	private void SpawnCorridorPattern()
	{
		int count = Random.Range(3, 6);

		// Pick a center Y for the corridor, then oscillate gently around it.
		// This creates a navigable wave pattern instead of extreme zigzags.
		float corridorCenter = Random.Range(MinGapCenter + 3f, MaxGapCenter - 3f);
		float oscillation = Random.Range(2f, 3f);

		float lowY = Mathf.Clamp(corridorCenter - oscillation, MinGapCenter, MaxGapCenter);
		float highY = Mathf.Clamp(corridorCenter + oscillation, MinGapCenter, MaxGapCenter);

		bool startHigh = Random.value > 0.5f;

		for (int i = 0; i < count; i++)
		{
			float gapCenter = ((i % 2 == 0) == startHigh) ? highY : lowY;
			SpawnBarrier(nextSpawnX, gapCenter);
			nextSpawnX += barrierWidth;
		}

		// Extra breathing room after a pattern
		nextSpawnX += patternBreatherGap;
	}

	/// <summary>
	/// Spawns a staircase of barriers with gap positions gradually shifting
	/// up or down, requiring the player to follow a diagonal path.
	/// </summary>
	private void SpawnStaircasePattern()
	{
		int count = Random.Range(3, 5);
		bool goingUp = Random.value > 0.5f;
		float stepSize = Random.Range(2.5f, 4f);

		// Calculate the total vertical range the staircase needs
		float totalShift = stepSize * (count - 1);

		// Pick a start Y that keeps the entire staircase within bounds
		float startY;
		if (goingUp)
		{
			float upperBound = Mathf.Max(MinGapCenter, MaxGapCenter - totalShift);
			startY = Random.Range(MinGapCenter, upperBound);
		}
		else
		{
			float lowerBound = Mathf.Min(MaxGapCenter, MinGapCenter + totalShift);
			startY = Random.Range(lowerBound, MaxGapCenter);
		}

		for (int i = 0; i < count; i++)
		{
			float gapCenter = startY + (goingUp ? 1f : -1f) * stepSize * i;
			gapCenter = Mathf.Clamp(gapCenter, MinGapCenter, MaxGapCenter);
			SpawnBarrier(nextSpawnX, gapCenter);
			nextSpawnX += barrierWidth;
		}

		// Extra breathing room after a pattern
		nextSpawnX += patternBreatherGap;
	}

	private void SpawnBarrier(float xPosition, float gapCenterY)
	{
		BarrierPair pair = Instantiate(barrierPairPrefab, new Vector3(xPosition, 0f, 0f), Quaternion.identity);
		pair.Configure(minY, maxY, gapCenterY, gapSize, barrierWidth);
		spawnedPairs.Add(pair);
	}

	private float RandomGapCenter()
	{
		return Random.Range(MinGapCenter, MaxGapCenter);
	}

	private void CleanupOldPairs()
	{
		float referenceX = targetCamera != null ? targetCamera.transform.position.x : player.position.x;
		float cleanupX = referenceX - cleanupBehindDistance;

		for (int i = spawnedPairs.Count - 1; i >= 0; i--)
		{
			BarrierPair pair = spawnedPairs[i];
			if (pair == null || pair.transform.position.x + pair.Width < cleanupX)
			{
				if (pair != null)
				{
					Destroy(pair.gameObject);
				}

				spawnedPairs.RemoveAt(i);
			}
		}
	}
}
