using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_zoom : MonoBehaviour
{
	public SpriteRenderer	screen_bound;

    // Start is called before the first frame update
    void Start()
    {
        float	screen_ratio = Screen.width / Screen.height;
		float	target_ratio = screen_bound.bounds.size.x / screen_bound.bounds.size.y;

		if (screen_ratio >= target_ratio){
			Camera.main.orthographicSize = screen_bound.bounds.size.y / 2;
		}
		else{
			float	size_difference = target_ratio / screen_ratio;
			Camera.main.orthographicSize = screen_bound.bounds.size.y / 2 * size_difference;
		}
    }
}
