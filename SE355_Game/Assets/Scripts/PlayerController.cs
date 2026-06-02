using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	private Rigidbody2D rb;
	private GameModeManager gameManager;

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
		rb.gravityScale = gravityStrength;
		currentMoveSpeed = moveSpeed;
		gameManager = FindFirstObjectByType<GameModeManager>();
	}

	void Update()
	{
		if (isDead) return;

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
}
