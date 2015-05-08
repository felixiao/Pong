using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Prediction{
	public static List<Vector2> recentPos=new List<Vector2>();
	public static List<float> deltaTime=new List<float>();
	public static void Add(Vector2 pos,float time){

		if (recentPos.Count > 4) {
			recentPos.RemoveAt(0);
			deltaTime.RemoveAt(0);
		}
		recentPos.Add (pos);
		deltaTime.Add (time);
	}
	public static Vector2 GetLinear(float deltaT){
		int count = recentPos.Count;
		if (count <= 1)	return Vector2.zero;
		float x = recentPos[count - 1].x - recentPos[count - 2].x;
		float y = recentPos[count - 1].y - recentPos[count - 2].y;
		float dt = deltaTime [deltaTime.Count - 1] - deltaTime [deltaTime.Count - 2];
		return new Vector2 (recentPos[count - 2].x+CalcLinear (dt, x, deltaT+dt), recentPos[count - 2].y+CalcLinear (dt, y, deltaT+dt));
	}
	public static float CalcLinear(float x1,float y1,float x2){
		return x2 * y1 / x1;
	}

	public static Vector2 GetQuadratic(float deltaT){
		int count = recentPos.Count;
		if (count <= 1)	return Vector2.zero;
		else if (count == 2) return GetLinear (deltaT);
		float x1=recentPos [count - 2].x - recentPos [count - 3].x;
		float y1 = recentPos [count- 2].y - recentPos [count - 3].y;
		float x2=recentPos [count - 1].x - recentPos [count - 3].x;
		float y2 = recentPos [count- 1].y - recentPos [count - 3].y;
		float dt1 = deltaTime [deltaTime.Count - 2] - deltaTime [deltaTime.Count - 3];
		float dt2 = deltaTime [deltaTime.Count - 1] - deltaTime [deltaTime.Count - 3];
		float newx = recentPos [count - 3].x + CalcQuadratic (dt1, x1, dt2, x2, deltaT + dt2);
		float newy = recentPos [count - 3].y + CalcQuadratic (dt1, y1, dt2, y2, deltaT + dt2);
		return new Vector2 (newx,newy);
	}
	public static float CalcQuadratic(float x1,float y1,float x2,float y2,float x3){
		float a = (y1 / x1 - y2 / x2) / (x1 - x2);
		float b = y1 / x1 - a * x1;
		return a*x3*x3+b*x3;
	}
}
