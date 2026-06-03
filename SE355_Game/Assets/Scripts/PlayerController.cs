using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	private Rigidbody2D rb;
	private GameModeManager gameManager;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	[Header("Movement")]
	public float moveSpeed = 5f;
	public float maxMoveSpeed = 22f;
	public float acceleration = 0.02f;

	[Header("Gravity")]
	public float gravityStrength = 5f;
	public float maxVerticalSpeed = 10f;

	private bool gravityUp = false;
	private float currentMoveSpeed;
	private bool isDead = false;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		rb.gravityScale = gravityStrength;
		currentMoveSpeed = moveSpeed;
		gameManager = FindFirstObjectByType<GameModeManager>();
	}

	void Update()
	{
		if (isDead) return;

		// Don't process input while in main menu
		if (gameManager != null && gameManager.CurrentState == GameModeManager.GameState.MainMenu) return;

		// Flip gravity with Space
		if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
		{
			gravityUp = !gravityUp;
			rb.gravityScale = gravityUp ? -gravityStrength : gravityStrength;

			// Zero out vertical velocity on flip for instant, responsive
			// direction changes. Gravity alone handles acceleration, so
			// the player can tap Space rapidly without any forced commitment.
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
		}
	}

	void FixedUpdate()
	{
		if (isDead) return;

		// Don't move while in main menu
		if (gameManager != null && gameManager.CurrentState == GameModeManager.GameState.MainMenu) return;

		currentMoveSpeed = Mathf.MoveTowards(
			currentMoveSpeed,
			maxMoveSpeed,
			acceleration * Time.fixedDeltaTime
		);

		// Clamp vertical speed so the player can't rocket through gaps
		float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxVerticalSpeed, maxVerticalSpeed);

		// Constant movement to the right
		rb.linearVelocity = new Vector2(currentMoveSpeed, clampedY);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (isDead) return;

		// Only die when hitting barriers, not ground/ceiling boundaries
		if (collision.gameObject.GetComponentInParent<BarrierPair>() == null) return;

		isDead = true;

		// Freeze the player in place
		rb.linearVelocity = Vector2.zero;
		rb.gravityScale = 0f;
		rb.bodyType = RigidbodyType2D.Kinematic;

		if (gameManager != null)
		{
			gameManager.TriggerGameOver();
		}
	}

	/// <summary>
	/// Called by GameModeManager when a new level starts.
	/// Resets the current speed to the new base speed for the level.
	/// </summary>
	public void SetBaseSpeed(float newBaseSpeed)
	{
		moveSpeed = newBaseSpeed;
		currentMoveSpeed = newBaseSpeed;
	}

	/// <summary>
	/// Hides the knight during menu screens.
	/// Disables the sprite, animator, and freezes the rigidbody.
	/// </summary>
	public void HidePlayer()
	{
		if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
		if (animator == null) animator = GetComponent<Animator>();
		if (rb == null) rb = GetComponent<Rigidbody2D>();

		if (spriteRenderer != null) spriteRenderer.enabled = false;
		if (animator != null) animator.enabled = false;

		rb.linearVelocity = Vector2.zero;
		rb.gravityScale = 0f;
		rb.bodyType = RigidbodyType2D.Kinematic;
	}

	/// <summary>
	/// Shows the knight when a level starts.
	/// Re-enables the sprite, animator, and restores the rigidbody.
	/// </summary>
	public void ShowPlayer()
	{
		if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
		if (animator == null) animator = GetComponent<Animator>();
		if (rb == null) rb = GetComponent<Rigidbody2D>();

		if (spriteRenderer != null) spriteRenderer.enabled = true;
		if (animator != null) animator.enabled = true;

		rb.bodyType = RigidbodyType2D.Dynamic;
		rb.gravityScale = gravityStrength;
		gravityUp = false;
		isDead = false;
	}
}
