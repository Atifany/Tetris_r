using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Keys_holder
{
	public static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

	public static void WriteNewBindings(){
		save = new Keys_holder_saver(keys["Left"], keys["Right"], keys["Down"], keys["Rotate"]);
		string jsonStr = JsonUtility.ToJson(save);

		using (StreamWriter sw = new StreamWriter("./keysBindingsSave.json")){
			sw.WriteLine(jsonStr);
		}
	}

	public static void ReadBindings(){
		string jsonStr;

		try{
			using (StreamReader sr = new StreamReader("./keysBindingsSave.json")){
				jsonStr = sr.ReadLine();
			}
			save = JsonUtility.FromJson<Keys_holder_saver>(jsonStr);
		}
		catch{
			save = new Keys_holder_saver(KeyCode.A, KeyCode.D, KeyCode.S, KeyCode.R);
			if (!keys.ContainsKey("Left")){		// if any key is missing that would mean that the whole dictionary is empty
				keys.Add("Left", save.left);
				keys.Add("Right", save.right);
				keys.Add("Down", save.down);
				keys.Add("Rotate", save.rotate);
			}
			WriteNewBindings();
		}
		keys["Left"] = save.left;
		keys["Right"] = save.right;
		keys["Down"] = save.down;
		keys["Rotate"] = save.rotate;
	}

	private static Keys_holder_saver save;
}

[Serializable]
public class Keys_holder_saver
{
	public KeyCode left;
	public KeyCode right;
	public KeyCode down;
	public KeyCode rotate;

	public Keys_holder_saver(KeyCode _left, KeyCode _right, KeyCode _down, KeyCode _rotate){
		left = _left;
		right = _right;
		down = _down;
		rotate = _rotate;
	}
}