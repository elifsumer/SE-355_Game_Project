using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform player;
	public float offsetX = 5f;

	private float fixedY;

	void Start()
	{
		fixedY = transform.position.y;
	}

	void LateUpdate()
	{
		transform.position = new Vector3(
			player.position.x + offsetX,
			fixedY,
			-10f
		);
	}
}