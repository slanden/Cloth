using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour 
{	
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}
}
