using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HighScoreSaver : MonoBehaviour
{
	public int Score;
	public int Lines;
	public int Level;

	public void	WriteNewHighScore(int score, int lines, int level){
		save = new HighScoreData(score, lines, level);
		string jsonStr = JsonUtility.ToJson(save);

		using (StreamWriter sw = new StreamWriter("./HighestScoreSave.json")){
			sw.WriteLine(jsonStr);
		}
	}

	public void	ReadHighScore(){
		try{
			string jsonStr;
			using (StreamReader sr = new StreamReader("./HighestScoreSave.json")){
				jsonStr = sr.ReadLine();
			}
			save = JsonUtility.FromJson<HighScoreData>(jsonStr);
		}
		catch{
			save = new HighScoreData(0, 0, 0);
			WriteNewHighScore(0, 0, 0);
		}
		Score = save.highestScore;
		Lines = save.highestLines;
		Level = save.highestLevel;
	}

	private HighScoreData	save = null;
}

[Serializable]
public class HighScoreData
{
	public int	highestScore;
	public int	highestLines;
	public int	highestLevel;

	public HighScoreData(int score, int lines, int level){
		highestScore = score;
		highestLines = lines;
		highestLevel = level;
	}
}