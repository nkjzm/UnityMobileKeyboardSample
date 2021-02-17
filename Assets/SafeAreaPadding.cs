using UnityEngine;

// cf: http://tsubakit1.hateblo.jp/entry/2019/10/30/235150
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class SafeAreaPadding : MonoBehaviour
{
	private Rect area = Rect.zero;

	void Update()
	{
		if (area == Screen.safeArea) return;
		
		area = Screen.safeArea;

		var rect = GetComponent<RectTransform>();
		var resolution = Screen.currentResolution;
		rect.sizeDelta = Vector2.zero;
		rect.anchorMax = new Vector2(area.xMax / resolution.width, area.yMax / resolution.height);
		rect.anchorMin = new Vector2(area.xMin / resolution.width, area.yMin / resolution.height);
		Canvas.ForceUpdateCanvases();
	}
}