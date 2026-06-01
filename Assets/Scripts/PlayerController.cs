using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private Rigidbody2D rb;

	[Header("Movement")]
	public float moveSpeed = 5f;

	[Header("Speed Scaling")]
	public float speedIncreaseInterval = 5f;
	public float speedMultiplier = 2f;

	[Header("Gravity")]
	public float gravityStrength = 3f;

	private bool gravityUp = false;
	private float timer = 0f;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = gravityStrength;
	}

	void Update()
	{
		// Flip gravity with Space
		if (Input.GetKeyDown(KeyCode.Space))
		{
			gravityUp = !gravityUp;

			if (gravityUp)
			{
				rb.gravityScale = -gravityStrength;
			}
			else
			{
				rb.gravityScale = gravityStrength;
			}
		}

		// Speed increase timer
		timer += Time.deltaTime;

		if (timer >= speedIncreaseInterval)
		{
			moveSpeed *= speedMultiplier;
			timer = 0f;
		}
	}

	void FixedUpdate()
	{
		// Constant movement to the right
		rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
	}
}