using UnityEngine;
using System.Collections;

public class SliderControl : MonoBehaviour {
	Vector2 origin=new Vector2(-2.8f,-4.875f);
	float dy=9.75f,yLimit=4f;
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0)){
			float y=origin.y+dy*(Input.mousePosition.y / Screen.height);
			if(y>yLimit)
				y=yLimit;
			if(y<-yLimit)
				y=-yLimit;
			transform.position=new Vector3(transform.position.x,y,transform.position.z);
		}
		if(Input.touchCount>0){
			if(Input.GetTouch(0).phase==TouchPhase.Moved){
				float y=origin.y+dy*(Input.mousePosition.y / Screen.height);
				if(y>yLimit)
					y=yLimit;
				if(y<-yLimit)
					y=-yLimit;
				transform.position=new Vector3(transform.position.x,y,transform.position.z);
			}
		}
	}
}
