using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform player;
	public float offsetX = 5f;

	void LateUpdate()
	{
		transform.position = new Vector3(
			player.position.x + offsetX,
			player.position.y,
			-10f
		);
	}
}