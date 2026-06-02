using UnityEngine;

public class BarrierPair : MonoBehaviour
{
	[SerializeField] private SpriteRenderer topBarrier;
	[SerializeField] private SpriteRenderer bottomBarrier;
	[SerializeField] private BoxCollider2D topCollider;
	[SerializeField] private BoxCollider2D bottomCollider;

	public float Width { get; private set; }

	public void Configure(float minY, float maxY, float gapCenterY, float gapSize, float width)
	{
		Width = width;

		float halfGap = gapSize * 0.5f;
		float gapBottom = Mathf.Clamp(gapCenterY - halfGap, minY, maxY);
		float gapTop = Mathf.Clamp(gapCenterY + halfGap, minY, maxY);

		ConfigureSection(bottomBarrier, bottomCollider, minY, gapBottom, width);
		ConfigureSection(topBarrier, topCollider, gapTop, maxY, width);
	}

	private void ConfigureSection(SpriteRenderer spriteRenderer, BoxCollider2D collider, float bottomY, float topY, float width)
	{
		float height = Mathf.Max(0.1f, topY - bottomY);
		float centerY = (bottomY + topY) * 0.5f;

		spriteRenderer.size = new Vector2(width, height);
		// Position at the center of the section since the sprite uses center pivot
		spriteRenderer.transform.localPosition = new Vector3(0f, centerY, 0f);

		collider.size = new Vector2(width, height);
		// Collider centered on the transform (matching the center-pivot sprite)
		collider.offset = Vector2.zero;
	}
}
