using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;

public class figure_movement : MonoBehaviour
{
	// Init
	public GameObject	field;
	public GameObject[]	prefabs;
	public Vector3[]	prefabs_rotations;
	[SerializeField] private CooldownSystem	cooldownSystem = null;
	[SerializeField] private HighScoreSaver	highScoreSaver = null;
	// interface
	public GameObject	retry_button;
	public GameObject	field_darkner;
	public GameObject	exit_button;
	public GameObject	exit_button_ingame;
	public GameObject	gameover_text;
	public GameObject	pause_button;
	public GameObject	score_text;
	public GameObject	lines_text;
	public GameObject	level_text;
	public Text			highScoreText;
	public Text			highLinesText;
	public Text			highLevelText;
	public GameObject	next_figure_display;
	// SFXs
	public AudioSource	SoundUIClick;
	public AudioSource	SoundRotate;
	public AudioSource	SoundMove;
	public AudioSource	SoundLineDelete;
	public AudioSource	SoundGameOver;
	public AudioSource	SoundGameMusic;
	// settings
	[SerializeField] private float			secondsWaitAfterButtonClick = 0.2f;
	[SerializeField] private int			linesPerLevel = 20;
	[SerializeField] private float			respawn_time = 0.5f;
	[SerializeField] private int			id_down = 1;
	[SerializeField] private float			cooldownDuration_down = 1f;
	[SerializeField] private int			id_down_forced = 2;
	[SerializeField] private float			cooldownDuration_down_forced = 0.5f;
	[SerializeField] private int			id_side = 3;
	[SerializeField] private float			cooldownDuration_side = 0.5f;

    // Start is called before the first frame update
	private void	Start()
    {
        if (!field) {
        	Debug.Log("Missing field prefab");
        }
		if (SceneManagerData.IsMusicOn == true) { 
			SoundGameMusic.Play();
			SoundGameMusic.pitch = 0.81f;
			StartCoroutine(AudioFade.FadeVolumeAudioSource(SoundGameMusic, 0.5f, 1.0f));
		}

		exit_button.GetComponent<Button>().onClick.AddListener(delegate { PressUIButton("Menu"); });
		exit_button_ingame.GetComponent<Button>().onClick.AddListener(delegate { PressUIButton("Menu"); });
		retry_button.GetComponent<Button>().onClick.AddListener(delegate { PressUIButton("GameScene"); });
		pause_button.GetComponent<Button>().onClick.AddListener(PauseGame);

		field_matrix = new GameObject[20,10];
		field_filled = new int[20];
		for (int i = 0; i < 20; i++)
		{
			field_filled[i] = 0;
			for (int j = 0; j < 10; j++)
			{
				field_matrix[i, j] = null;
			}
		}

		prefab_id = UnityEngine.Random.Range(0, prefabs.Length);
		RefreshHighScore();
    }

	// Update is called once per frame
	private void	Update()
    {
		if (!is_paused && is_spawning_new){
			StartCoroutine(Spawn_delay());
		}
		if (!is_paused && figure){
			Rotate_figure(Math.PI / 2);
			if (!cooldownSystem.IsOnCoolDown(id_down)){
				cooldownSystem.PutOnCooldown(id_down, cooldownDuration_down);
				MoveDown();
			}
		}
		if (is_new_level){
			LevelIncrease();
		}
    }

	// perhaps put all of this to a unique func?
	float t0Side = 0.0f;
	float t0Down = 0.0f;
	private void	OnGUI(){
		if (is_paused){ return ; }

		// down
		// click
		if (Input.GetKeyDown(Keys_holder.keys["Down"]) && !is_moving_down){
			is_moving_down = true;
			t0Down = Time.time;
			MoveDown();
		}
		// press
		if (is_moving_down && Time.time - t0Down > 0.1f){
			if (!cooldownSystem.IsOnCoolDown(id_down_forced)){
				cooldownSystem.PutOnCooldown(id_down_forced, cooldownDuration_down_forced);
				MoveDown();
			}
		}
		// release
		if (Input.GetKeyUp(Keys_holder.keys["Down"]) && is_moving_down){
			is_moving_down = false;
		}

		// left and right
		// click
		if (Input.GetKeyDown(Keys_holder.keys["Right"]) && !is_moving_right){
			is_moving_right = true;
			is_moving_left = false;
			t0Side = Time.time;
			MoveSide();
		}
		if (Input.GetKeyDown(Keys_holder.keys["Left"]) && !is_moving_left){
			is_moving_left = true;
			is_moving_right = false;
			t0Side = Time.time;
			MoveSide();
		}
		// press
		if ((is_moving_left || is_moving_right) && Time.time - t0Side > 0.1f){
			if (!cooldownSystem.IsOnCoolDown(id_side)){
				cooldownSystem.PutOnCooldown(id_side, cooldownDuration_side);
				MoveSide();
			}
		}
		// release
		if (Input.GetKeyUp(Keys_holder.keys["Right"]) && is_moving_right){
			is_moving_right = false;
		}
		if (Input.GetKeyUp(Keys_holder.keys["Left"]) && is_moving_left){
			is_moving_left = false;
		}
	}

	private void	RefreshHighScore(){
		highScoreSaver.ReadHighScore();
		highScoreText.text = "Top score:\n" + highScoreSaver.Score;
		highLinesText.text = "Top lines:\n" + highScoreSaver.Lines;
		highLevelText.text = "Top level:\n" + highScoreSaver.Level;
	}

	private void	LevelIncrease(){
		level++;
		is_new_level = false;
		if (level < 12){
			AudioFade.FadePitchAudioSource(SoundGameMusic, SoundGameMusic.pitch += SoundGameMusic.pitch / 12, 0.5f);
		}
		cooldownDuration_down -= cooldownDuration_down / 12;					// increase by 10% of current value
		level_text.GetComponent<Text>().text = "Level:\n" + level.ToString();
	}

	private void	OpenGameOverMenu(){
		StartCoroutine(AudioFade.FadeVolumeAudioSource(SoundGameMusic, 0.5f, 1.0f));
		StartCoroutine(AudioFade.FadePitchAudioSource(SoundGameMusic, 0.5f, 1.0f));
		SoundGameOver.Play();
		int _score = Math.Max(highScoreSaver.Score, score);
		int _lines = Math.Max(highScoreSaver.Lines, lines);
		int _level = Math.Max(highScoreSaver.Level, level);
		highScoreSaver.WriteNewHighScore(_score, _lines, _level);

		field_darkner.gameObject.SetActive(true);
		gameover_text.gameObject.SetActive(true);
		exit_button.gameObject.SetActive(true);
		retry_button.gameObject.SetActive(true);
	}

	private void	PressUIButton(string scene){
		SoundUIClick.Play();
		StartCoroutine(LoadScene(scene));
	}

	private IEnumerator	LoadScene(string scene){
		is_paused = true;
		yield return new WaitForSeconds(secondsWaitAfterButtonClick);
		SceneLoader.Load(scene);
	}

	private IEnumerator Spawn_delay(){
		is_spawning_new = false;
		yield return new WaitForSeconds(respawn_time);

		figure = (GameObject)Instantiate(prefabs[prefab_id], new Vector3(-0.5f, 8.5f, 0.0f), Quaternion.identity);
		prefab_id_cur = prefab_id;
		prefab_id = UnityEngine.Random.Range(0, prefabs.Length);

		if (figure_prewiew)
			Destroy(figure_prewiew);
		figure_prewiew = (GameObject)Instantiate(prefabs[prefab_id], next_figure_display.transform.position, Quaternion.identity);
		figure_prewiew.transform.Translate(new Vector2(0.5f, -0.5f));
	}

	private void	PauseGame(){
		if (!is_gameover){
			SoundUIClick.Play();
			if (is_paused){
				field_darkner.SetActive(false);
				StartCoroutine(AudioFade.FadeVolumeAudioSource(SoundGameMusic, 0.5f, 1.0f));
			}
			else{
				field_darkner.SetActive(true);
				StartCoroutine(AudioFade.FadeVolumeAudioSource(SoundGameMusic, 0.25f, 1.0f));
			}
			is_paused = !is_paused;
		}
	}

	private void	Place_tiles(){
		GameObject	child_tile = null;

		for (int k = 0; k < 4; k++)
		{
			child_tile = figure.transform.GetChild(0).gameObject;
			child_tile.transform.parent = field.transform;
			child_tile.transform.rotation = Quaternion.identity;
			int j = (int)Math.Round(child_tile.transform.position.x + 4.5f);
			int i = (int)Math.Round(child_tile.transform.position.y + 9.5f);
			field_matrix[i, j] = child_tile;
			field_filled[i]++;
			if (i >= 17){
				is_paused = true;
				is_gameover = true;
			}
		}
		Destroy(figure);
		figure = null;
		is_spawning_new = true;
		StartCoroutine(Destroy_Line());
		if (is_gameover){
			OpenGameOverMenu();
		}
	}

	private void	MoveSide(){
		if (is_moving_left && !is_moving_right && !Check_Collision("left") && figure){
			SoundMove.Play();
			figure.transform.position -= new Vector3(tile_size, 0, 0);
		}
		if (is_moving_right && !is_moving_left && !Check_Collision("right") && figure){
			SoundMove.Play();
			figure.transform.position += new Vector3(tile_size, 0, 0);
		}
	}

	private void	MoveDown(){
		if (!Check_Collision("bottom") && figure){
       		figure.transform.position -= new Vector3(0, tile_size, 0);
		}
		else if (figure){
			Place_tiles();
		}
	}

	private void	Rotate_Tiles_Local(double angle, float[] i, float[] j)
	{
		GameObject	child_tile = null;
		float sine = (float)Math.Round(Math.Sin(angle), 1);
		float cosine = (float)Math.Round(Math.Cos(angle), 1);
		// rotate tile
		for (int k = 0; k < 4; k++)
		{
			child_tile = figure.transform.GetChild(k).gameObject;
			float x = child_tile.transform.localPosition.x - prefabs_rotations[prefab_id_cur].x;
			float y = child_tile.transform.localPosition.y - prefabs_rotations[prefab_id_cur].y;
			float new_x = x * cosine - y * sine + prefabs_rotations[prefab_id_cur].y;
			float new_y = x * sine + y * cosine + prefabs_rotations[prefab_id_cur].x;
			i[k] = new_y;
			j[k] = new_x;
		}
	}

	private Vector2	Rotate_Collision_Walls(float[] i, float[] j){
		GameObject	child_tile = null;
		int 		shift_i = 0;
		int 		shift_j = 0;

		// collsion with walls
		for (int k = 0; k < 4; k++)
		{
			child_tile = figure.transform.GetChild(k).gameObject;
			int new_j = (int)Math.Round(child_tile.transform.position.x + j[k] - child_tile.transform.localPosition.x + 4.5f);
			int new_i = (int)Math.Round(child_tile.transform.position.y + i[k] - child_tile.transform.localPosition.y + 9.5f);
			if (new_j < 0 && new_j < shift_j){
				shift_j = new_j;
			}
			if (new_j > 9 && new_j - 9 > shift_j){
				shift_j = new_j - 9;
			}
			if (new_i < 0 && new_i < shift_i){
				shift_i = new_i;
			}
			if (new_i > 19 && new_i - 19 > shift_i){
				shift_i = new_i - 19;
			}
		}
		return (new Vector2(shift_i, shift_j));
	}

	private void	Rotate_figure(double angle){
		GameObject	child_tile = null;
		float[] i;
		float[] j;
		int shift_i = 0;
		int shift_j = 0;

		i = new float[4];
		j = new float[4];
		if (Input.GetKeyDown(Keys_holder.keys["Rotate"])){
			SoundRotate.Play();
			
			Rotate_Tiles_Local(angle, i, j);
			Vector2 transfer = Rotate_Collision_Walls(i, j);
			shift_i = (int)transfer.x; shift_j = (int)transfer.y;
			// Collision with blocks
			for (int k = 0; k < 4; k++)
			{
				child_tile = figure.transform.GetChild(k).gameObject;
				int new_j = (int)Math.Round(child_tile.transform.position.x + j[k] - shift_j - child_tile.transform.localPosition.x + 4.5f);
				int new_i = (int)Math.Round(child_tile.transform.position.y + i[k] - shift_i - child_tile.transform.localPosition.y + 9.5f);
				if (field_matrix[new_i, new_j]){
					return ;
				}
			}
			for (int k = 0; k < 4; k++)
			{
				child_tile = figure.transform.GetChild(k).gameObject;
				child_tile.transform.localPosition = new Vector3(j[k], i[k], 0.0f);
			}
			figure.transform.position -= new Vector3(shift_j, shift_i, 0.0f);
		}
	}

	private bool	Check_Collision(string dir){
		GameObject	child_tile = null;
		bool		ret = false;

		if (!figure)
			return (false);
		for (int k = 0; k < 4; k++)
			{
				child_tile = figure.transform.GetChild(k).gameObject;
				int j = (int)Math.Round(child_tile.transform.position.x + 4.5f);
				int i = (int)Math.Round(child_tile.transform.position.y + 9.5f);
				if (dir == "left"){
					if (j == 0 || (j != 0 && field_matrix[i, j - 1])){
						ret = true;
					}
				}
				else if (dir == "right"){
					if (j == 9 || (j != 9 && field_matrix[i, j + 1])){
						ret = true;
					}
				}
				else if (dir == "bottom"){
					if (i == 0 || (i != 0 && field_matrix[i - 1, j])){
						ret = true;
					}
				}
			}
		return (ret);
	}

	private IEnumerator	Play_All_Anims(int row)
    {
		for (int col = 0; col < 10; col++)
		{
			field_matrix[row, col].GetComponent<Animator>().SetBool("is_deleting", true);
		}
		SoundLineDelete.Play();
		yield return new WaitForSeconds(0.1f); // just an adjastable value between line deleteions
	}

	private IEnumerator Destroy_Line(){
		int		to_add = 0;
		// delete line
		for (int i = 0; i < 20; i++)
		{
			if (field_filled[i] == 10){
				yield return StartCoroutine(Play_All_Anims(i));
				for (int j = 0; j < 10; j++)
				{
					Destroy(field_matrix[i, j]);
					field_matrix[i, j] = null;
				}
				field_filled[i] = 0;
				to_add+=10;
				lines++;
				if (lines != 0 && lines % linesPerLevel == 0){
					is_new_level = true;
				}
			}
		}
		if (to_add == 30){
			to_add = 50;
		}
		if (to_add == 40){
			to_add = 70;
		}
		score += to_add;
		score_text.GetComponent<Text>().text = ("Score:\n" + score.ToString());
		lines_text.GetComponent<Text>().text = ("Lines:\n" + lines.ToString());
		// move lines down
		for (int i = 1; i < 20; i++)
		{
			int j = i;
			while (field_filled[j] > 0 && j > 0 && field_filled[j - 1] == 0){
				for (int k = 0; k < 10; k++)
				{
					if (field_matrix[j, k]){
						field_matrix[j, k].transform.Translate(new Vector3(0, -1.0f, 0));
						field_matrix[j - 1, k] = field_matrix[j, k];
						field_matrix[j, k] = null;
						field_filled[j]--;
						field_filled[j - 1]++;
					}
				}
				j--;
			}
		}
	}

	// private use
	private GameObject			figure;
	private GameObject			figure_prewiew = null;

	private float				tile_size = 1.0f;
	private bool				is_spawning_new = true;
	private GameObject[,]		field_matrix;
	private int[]				field_filled;
	private int 				prefab_id = 0;
	private int 				prefab_id_cur = 0;
	private bool				is_paused = false;
	private bool				is_gameover = false;
	private bool				is_new_level = false;
	private int					score = 0;
	private int					lines = 0;
	private int					level = 1;
	
	private bool	is_moving_right = false;
	private bool	is_moving_left = false;
	private bool	is_moving_down = false;

}
