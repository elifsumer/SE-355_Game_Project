using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
	public enum GameMode
	{
		Level1,
		Level2,
		Level3,
		Infinite
	}

	[SerializeField] private GameMode startingMode = GameMode.Level1;
	[SerializeField] private float level1Duration = 30f;
	[SerializeField] private float level2Duration = 60f;
	[SerializeField] private float level3Duration = 120f;

	private GameMode currentMode;
	private float elapsedTime;
	private bool levelComplete;
	private bool isGameOver;
	private GUIStyle labelStyle;
	private GUIStyle gameOverStyle;
	private GUIStyle gameOverSubStyle;

	private void Start()
	{
		StartMode(startingMode);
	}

	private void Update()
	{
		Keyboard keyboard = Keyboard.current;
		if (keyboard != null)
		{
			if (keyboard.digit1Key.wasPressedThisFrame)
			{
				StartMode(GameMode.Level1);
			}
			else if (keyboard.digit2Key.wasPressedThisFrame)
			{
				StartMode(GameMode.Level2);
			}
			else if (keyboard.digit3Key.wasPressedThisFrame)
			{
				StartMode(GameMode.Level3);
			}
			else if (keyboard.digit4Key.wasPressedThisFrame)
			{
				StartMode(GameMode.Infinite);
			}
			else if (keyboard.rKey.wasPressedThisFrame)
			{
				Time.timeScale = 1f;
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
		}

		if (levelComplete || isGameOver)
		{
			return;
		}

		elapsedTime += Time.deltaTime;

		if (currentMode != GameMode.Infinite && elapsedTime >= GetCurrentDuration())
		{
			AdvanceLevel();
		}
	}

	/// <summary>
	/// Called by PlayerController when the player collides with an obstacle.
	/// Freezes gameplay and displays the game over screen.
	/// </summary>
	public void TriggerGameOver()
	{
		if (isGameOver || levelComplete) return;
		isGameOver = true;
		Time.timeScale = 0f;
	}

	private void AdvanceLevel()
	{
		if (currentMode == GameMode.Level1)
		{
			StartMode(GameMode.Level2);
		}
		else if (currentMode == GameMode.Level2)
		{
			StartMode(GameMode.Level3);
		}
		else
		{
			elapsedTime = GetCurrentDuration();
			levelComplete = true;
			Time.timeScale = 0f;
		}
	}

	private void StartMode(GameMode mode)
	{
		currentMode = mode;
		elapsedTime = 0f;
		levelComplete = false;
		isGameOver = false;
		Time.timeScale = 1f;
	}

	private float GetCurrentDuration()
	{
		switch (currentMode)
		{
			case GameMode.Level1:
				return level1Duration;
			case GameMode.Level2:
				return level2Duration;
			case GameMode.Level3:
				return level3Duration;
			default:
				return 0f;
		}
	}

	private string GetModeName()
	{
		switch (currentMode)
		{
			case GameMode.Level1:
				return "Level 1";
			case GameMode.Level2:
				return "Level 2";
			case GameMode.Level3:
				return "Level 3";
			default:
				return "Infinite";
		}
	}

	private void OnGUI()
	{
		if (labelStyle == null)
		{
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.fontSize = 28;
			labelStyle.normal.textColor = Color.white;
		}

		if (gameOverStyle == null)
		{
			gameOverStyle = new GUIStyle(GUI.skin.label);
			gameOverStyle.fontSize = 64;
			gameOverStyle.fontStyle = FontStyle.Bold;
			gameOverStyle.normal.textColor = new Color(0.9f, 0.2f, 0.2f);
			gameOverStyle.alignment = TextAnchor.MiddleCenter;
		}

		if (gameOverSubStyle == null)
		{
			gameOverSubStyle = new GUIStyle(GUI.skin.label);
			gameOverSubStyle.fontSize = 28;
			gameOverSubStyle.normal.textColor = Color.white;
			gameOverSubStyle.alignment = TextAnchor.MiddleCenter;
		}

		string timerText = currentMode == GameMode.Infinite
			? $"Time: {FormatTime(elapsedTime)}"
			: $"Time: {FormatTime(elapsedTime)} / {FormatTime(GetCurrentDuration())}";

		GUI.Label(new Rect(20f, 20f, 500f, 40f), $"{GetModeName()}   {timerText}", labelStyle);

		if (levelComplete)
		{
			GUI.Label(new Rect(20f, 62f, 500f, 40f), "Level complete", labelStyle);
		}

		if (isGameOver)
		{
			float centerX = Screen.width * 0.5f;
			float centerY = Screen.height * 0.5f;

			// Semi-transparent dark overlay
			Texture2D overlayTex = new Texture2D(1, 1);
			overlayTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.5f));
			overlayTex.Apply();
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTex);

			GUI.Label(new Rect(centerX - 300f, centerY - 60f, 600f, 80f), "GAME OVER", gameOverStyle);
			GUI.Label(new Rect(centerX - 300f, centerY + 20f, 600f, 40f), "Press R to restart", gameOverSubStyle);
		}
	}

	private string FormatTime(float seconds)
	{
		int totalSeconds = Mathf.FloorToInt(seconds);
		int minutes = totalSeconds / 60;
		int remainingSeconds = totalSeconds % 60;

		return $"{minutes:00}:{remainingSeconds:00}";
	}
}
