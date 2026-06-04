using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
	public enum GameState
	{
		MainMenu,
		Playing,
		LevelComplete,
		GameOver,
		GameWon
	}

	public enum MenuScreen
	{
		Play,
		LevelSelect
	}

	public enum GameMode
	{
		Level1,
		Level2,
		Level3,
		Infinite
	}

	[Header("Level Settings")]
	[SerializeField] private float levelDuration = 30f;

	[Header("Base Speed Per Level")]
	[SerializeField] private float level1BaseSpeed = 5f;
	[SerializeField] private float level2BaseSpeed = 9f;
	[SerializeField] private float level3BaseSpeed = 13f;
	[SerializeField] private float infiniteBaseSpeed = 5f;

	[Header("UI Textures (assign in Inspector or loaded from Resources)")]
	[SerializeField] private Texture2D playButtonTexture;
	[SerializeField] private Texture2D level1ButtonTexture;
	[SerializeField] private Texture2D level2ButtonTexture;
	[SerializeField] private Texture2D level3ButtonTexture;

	private GameState currentState = GameState.MainMenu;
	private MenuScreen currentMenuScreen = MenuScreen.Play;
	private GameMode currentMode;
	private float elapsedTime;
	private float levelCompleteTimer;
	private const float LEVEL_COMPLETE_DISPLAY_DURATION = 3f;

	/// <summary>
	/// Static flag so that when the scene reloads after a level ends,
	/// the menu opens directly on the level-select screen instead of
	/// the play screen.
	/// </summary>
	private static MenuScreen nextSceneMenuScreen = MenuScreen.Play;

	/// <summary>
	/// When set, the scene reload triggered by R will auto-start this mode
	/// instead of showing the main menu.
	/// </summary>
	private static GameMode? pendingRestartMode = null;

	/// <summary>
	/// Tracks the highest level the player has unlocked (1 = only Level 1,
	/// 2 = Level 1 & 2, 3 = all). Persists across scene reloads via static.
	/// </summary>
	private static int highestUnlockedLevel = 1;

	// Cached GUI styles
	private GUIStyle labelStyle;
	private GUIStyle titleStyle;
	private GUIStyle subtitleStyle;
	private GUIStyle gameOverStyle;
	private GUIStyle gameOverSubStyle;
	private GUIStyle levelCompleteStyle;
	private GUIStyle levelInfoStyle;
	private GUIStyle backButtonStyle;
	private Texture2D overlayTex;
	private Texture2D semiOverlayTex;

	// Public accessors
	public GameState CurrentState => currentState;
	public GameMode CurrentMode => currentMode;
	public bool IsInMenu => currentState == GameState.MainMenu;

	private PlayerController playerController;

	private void Start()
	{
		playerController = FindFirstObjectByType<PlayerController>();
		LoadButtonTextures();

		// If R was pressed to restart, jump straight into the same mode
		if (pendingRestartMode.HasValue)
		{
			GameMode modeToRestart = pendingRestartMode.Value;
			pendingRestartMode = null;
			StartPlaying(modeToRestart);
			return;
		}

		currentState = GameState.MainMenu;
		currentMenuScreen = nextSceneMenuScreen;
		nextSceneMenuScreen = MenuScreen.Play; // reset for future loads
		Time.timeScale = 0f;

		// Hide the knight during menus
		if (playerController != null)
		{
			playerController.HidePlayer();
		}
	}

	private void LoadButtonTextures()
	{
		if (playButtonTexture == null)
			playButtonTexture = Resources.Load<Texture2D>("play_button");
		if (level1ButtonTexture == null)
			level1ButtonTexture = Resources.Load<Texture2D>("Level1_Buton");
		if (level2ButtonTexture == null)
			level2ButtonTexture = Resources.Load<Texture2D>("Level2_Buton");
		if (level3ButtonTexture == null)
			level3ButtonTexture = Resources.Load<Texture2D>("Level3_Buton");
	}

	private void Update()
	{
		Keyboard keyboard = Keyboard.current;

		// Restart with R key from game over, game won, or while playing
		if (keyboard != null && keyboard.rKey.wasPressedThisFrame
			&& (currentState == GameState.GameOver
				|| currentState == GameState.GameWon
				|| currentState == GameState.Playing))
		{
			pendingRestartMode = currentMode;
			Time.timeScale = 1f;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			return;
		}

		// Escape: go back one step
		if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
		{
			if (currentState == GameState.MainMenu && currentMenuScreen == MenuScreen.LevelSelect)
			{
				currentMenuScreen = MenuScreen.Play;
			}
			else if (currentState != GameState.MainMenu)
			{
				Time.timeScale = 1f;
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
			return;
		}

		// Level complete screen — wait then go back to level select
		if (currentState == GameState.LevelComplete)
		{
			levelCompleteTimer -= Time.unscaledDeltaTime;
			if (levelCompleteTimer <= 0f)
			{
				ReturnToLevelSelect();
			}
			return;
		}

		if (currentState != GameState.Playing) return;

		elapsedTime += Time.deltaTime;

		if (currentMode != GameMode.Infinite && elapsedTime >= levelDuration)
		{
			OnLevelTimerExpired();
		}
	}

	public void TriggerGameOver()
	{
		if (currentState == GameState.GameOver || currentState == GameState.LevelComplete) return;
		currentState = GameState.GameOver;
		Time.timeScale = 0f;
	}

	public float GetCurrentBaseSpeed()
	{
		switch (currentMode)
		{
			case GameMode.Level1: return level1BaseSpeed;
			case GameMode.Level2: return level2BaseSpeed;
			case GameMode.Level3: return level3BaseSpeed;
			default: return infiniteBaseSpeed;
		}
	}

	private void OnLevelTimerExpired()
	{
		// Unlock the next level
		int completedLevel = currentMode == GameMode.Level1 ? 1
			: currentMode == GameMode.Level2 ? 2 : 3;
		if (completedLevel >= highestUnlockedLevel && completedLevel < 3)
		{
			highestUnlockedLevel = completedLevel + 1;
		}

		if (currentMode == GameMode.Level3)
		{
			currentState = GameState.GameWon;
			Time.timeScale = 0f;
		}
		else
		{
			currentState = GameState.LevelComplete;
			levelCompleteTimer = LEVEL_COMPLETE_DISPLAY_DURATION;
			Time.timeScale = 0f;
		}
	}

	/// <summary>
	/// Reloads the scene and opens directly on the level-select screen.
	/// </summary>
	private void ReturnToLevelSelect()
	{
		nextSceneMenuScreen = MenuScreen.LevelSelect;
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void StartPlaying(GameMode mode)
	{
		currentMode = mode;
		elapsedTime = 0f;
		currentState = GameState.Playing;
		Time.timeScale = 1f;

		// Show the knight
		if (playerController == null)
			playerController = FindFirstObjectByType<PlayerController>();

		if (playerController != null)
		{
			playerController.ShowPlayer();
			playerController.SetBaseSpeed(GetCurrentBaseSpeed());
		}
	}

	private string GetModeName()
	{
		switch (currentMode)
		{
			case GameMode.Level1: return "Level 1";
			case GameMode.Level2: return "Level 2";
			case GameMode.Level3: return "Level 3";
			default: return "Infinite";
		}
	}

	// ─────────────────────────── GUI ───────────────────────────

	private void InitStyles()
	{
		if (labelStyle != null) return;

		labelStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 28,
			fontStyle = FontStyle.Bold
		};
		labelStyle.normal.textColor = Color.white;

		titleStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 72,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};
		titleStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

		subtitleStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 36,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};
		subtitleStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);

		gameOverStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 64,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};
		gameOverStyle.normal.textColor = new Color(0.9f, 0.2f, 0.2f);

		gameOverSubStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 28,
			alignment = TextAnchor.MiddleCenter
		};
		gameOverSubStyle.normal.textColor = Color.white;

		levelCompleteStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 52,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};
		levelCompleteStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);

		levelInfoStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 32,
			alignment = TextAnchor.MiddleCenter
		};
		levelInfoStyle.normal.textColor = Color.white;

		backButtonStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 24,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};
		backButtonStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

		overlayTex = new Texture2D(1, 1);
		overlayTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.45f));
		overlayTex.Apply();

		semiOverlayTex = new Texture2D(1, 1);
		semiOverlayTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.5f));
		semiOverlayTex.Apply();
	}

	private void OnGUI()
	{
		InitStyles();

		switch (currentState)
		{
			case GameState.MainMenu:
				if (currentMenuScreen == MenuScreen.Play)
					DrawPlayScreen();
				else
					DrawLevelSelectScreen();
				break;
			case GameState.Playing:
				DrawHUD();
				break;
			case GameState.LevelComplete:
				DrawHUD();
				DrawLevelComplete();
				break;
			case GameState.GameOver:
				DrawHUD();
				DrawGameOver();
				break;
			case GameState.GameWon:
				DrawHUD();
				DrawGameWon();
				break;
		}
	}

	private void DrawPlayScreen()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTex);

		float centerX = Screen.width * 0.5f;
		float centerY = Screen.height * 0.5f;

		GUI.Label(new Rect(centerX - 400f, centerY - 180f, 800f, 100f), "GRAVITY KNIGHT", titleStyle);

		float btnW = 240f;
		float btnH = 72f;
		float playY = centerY - 10f;

		if (DrawTextureButton(new Rect(centerX - btnW * 0.5f, playY, btnW, btnH), playButtonTexture, "PLAY"))
		{
			currentMenuScreen = MenuScreen.LevelSelect;
		}

		GUIStyle instrStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 18,
			alignment = TextAnchor.MiddleCenter
		};
		instrStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
		GUI.Label(new Rect(centerX - 300f, playY + btnH + 60f, 600f, 30f), "Press SPACE to flip gravity", instrStyle);
	}

	private void DrawLevelSelectScreen()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTex);

		float centerX = Screen.width * 0.5f;
		float centerY = Screen.height * 0.5f;

		GUI.Label(new Rect(centerX - 400f, centerY - 180f, 800f, 80f), "SELECT LEVEL", subtitleStyle);

		float btnW = 200f;
		float btnH = 60f;
		float spacing = 30f;
		float totalWidth = btnW * 3f + spacing * 2f;
		float startX = centerX - totalWidth * 0.5f;
		float levelY = centerY - 30f;

		// Level 1 — always unlocked
		if (DrawTextureButton(new Rect(startX, levelY, btnW, btnH), level1ButtonTexture, "LEVEL 1", true))
		{
			StartPlaying(GameMode.Level1);
		}

		// Level 2 — unlocked after completing Level 1
		bool level2Unlocked = highestUnlockedLevel >= 2;
		if (DrawTextureButton(new Rect(startX + btnW + spacing, levelY, btnW, btnH), level2ButtonTexture, "LEVEL 2", level2Unlocked))
		{
			StartPlaying(GameMode.Level2);
		}

		// Level 3 — unlocked after completing Level 2
		bool level3Unlocked = highestUnlockedLevel >= 3;
		if (DrawTextureButton(new Rect(startX + (btnW + spacing) * 2f, levelY, btnW, btnH), level3ButtonTexture, "LEVEL 3", level3Unlocked))
		{
			StartPlaying(GameMode.Level3);
		}

		// Back button
		float backY = levelY + btnH + 50f;
		Rect backRect = new Rect(centerX - 60f, backY, 120f, 36f);
		bool backHover = backRect.Contains(Event.current.mousePosition);
		GUIStyle backStyle = new GUIStyle(backButtonStyle);
		backStyle.normal.textColor = backHover ? Color.white : new Color(0.7f, 0.7f, 0.7f);
		if (GUI.Button(backRect, "← BACK", backStyle))
		{
			currentMenuScreen = MenuScreen.Play;
		}

		GUIStyle instrStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 18,
			alignment = TextAnchor.MiddleCenter
		};
		instrStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
		GUI.Label(new Rect(centerX - 300f, backY + 50f, 600f, 30f), "R to restart  |  ESC to go back", instrStyle);
	}

	private bool DrawTextureButton(Rect rect, Texture2D texture, string fallbackText, bool enabled = true)
	{
		if (texture != null)
		{
			float texAspect = (float)texture.width / texture.height;
			float rectAspect = rect.width / rect.height;

			Rect drawRect = rect;
			if (texAspect > rectAspect)
			{
				float newHeight = rect.width / texAspect;
				drawRect = new Rect(rect.x, rect.y + (rect.height - newHeight) * 0.5f, rect.width, newHeight);
			}
			else
			{
				float newWidth = rect.height * texAspect;
				drawRect = new Rect(rect.x + (rect.width - newWidth) * 0.5f, rect.y, newWidth, rect.height);
			}

			Color savedColor = GUI.color;

			if (!enabled)
			{
				// Locked — draw darkened and don't respond to clicks
				GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
				GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit);
				GUI.color = savedColor;
				return false;
			}

			bool isHovering = drawRect.Contains(Event.current.mousePosition);
			if (isHovering)
			{
				GUI.color = new Color(1.2f, 1.2f, 1.2f, 1f);
				float grow = 4f;
				drawRect = new Rect(drawRect.x - grow, drawRect.y - grow, drawRect.width + grow * 2f, drawRect.height + grow * 2f);
			}

			GUI.DrawTexture(drawRect, texture, ScaleMode.ScaleToFit);
			GUI.color = savedColor;

			GUIStyle invisible = new GUIStyle();
			return GUI.Button(drawRect, GUIContent.none, invisible);
		}
		else
		{
			GUI.enabled = enabled;
			GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 22,
				fontStyle = FontStyle.Bold
			};
			bool clicked = GUI.Button(rect, fallbackText, btnStyle);
			GUI.enabled = true;
			return clicked;
		}
	}

	private void DrawHUD()
	{
		float timeLeft = currentMode == GameMode.Infinite
			? elapsedTime
			: Mathf.Max(0f, levelDuration - elapsedTime);

		string timerText = currentMode == GameMode.Infinite
			? $"Time: {FormatTime(elapsedTime)}"
			: $"Time Left: {FormatTime(timeLeft)}";

		Texture2D hudBg = new Texture2D(1, 1);
		hudBg.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.15f));
		hudBg.Apply();
		GUI.DrawTexture(new Rect(0, 0, Screen.width, 50f), hudBg);

		GUI.Label(new Rect(20f, 10f, 500f, 40f), $"{GetModeName()}   {timerText}", labelStyle);
	}

	private void DrawLevelComplete()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), semiOverlayTex);

		float centerX = Screen.width * 0.5f;
		float centerY = Screen.height * 0.5f;

		GUI.Label(new Rect(centerX - 400f, centerY - 80f, 800f, 80f), $"{GetModeName()} Complete!", levelCompleteStyle);
		GUI.Label(new Rect(centerX - 400f, centerY + 10f, 800f, 50f),
			"Returning to level select...", levelInfoStyle);
	}

	private void DrawGameOver()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), semiOverlayTex);

		float centerX = Screen.width * 0.5f;
		float centerY = Screen.height * 0.5f;

		GUI.Label(new Rect(centerX - 300f, centerY - 60f, 600f, 80f), "GAME OVER", gameOverStyle);
		GUI.Label(new Rect(centerX - 300f, centerY + 20f, 600f, 40f), "Press R to restart  |  ESC for menu", gameOverSubStyle);
	}

	private void DrawGameWon()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), semiOverlayTex);

		float centerX = Screen.width * 0.5f;
		float centerY = Screen.height * 0.5f;

		GUIStyle wonStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 64,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter
		};
		wonStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

		GUI.Label(new Rect(centerX - 400f, centerY - 80f, 800f, 80f), "YOU WIN!", wonStyle);
		GUI.Label(new Rect(centerX - 300f, centerY + 20f, 600f, 40f), "All levels completed!  Press R to restart", gameOverSubStyle);
	}

	private string FormatTime(float seconds)
	{
		int totalSeconds = Mathf.FloorToInt(seconds);
		int minutes = totalSeconds / 60;
		int remainingSeconds = totalSeconds % 60;
		return $"{minutes:00}:{remainingSeconds:00}";
	}
}
