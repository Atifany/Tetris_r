using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_navigator : MonoBehaviour
{
	// Interface
	public Text			left_key_prewiew;
	public Button		assign_left;
	public Text			right_key_prewiew;
	public Button		assign_right;
	public Text			down_key_prewiew;
	public Button		assign_down;
	public Text			rotate_key_prewiew;
	public Button		assign_rotate;
	public Text			music_switch_prewiew;
	public Button		assign_music_switch;
	public Button		play_button;
	public Button		exit_game;
	// Sounds
	public AudioSource	UIClick;
	public AudioSource	MenuMusic;
	// Settings
	[SerializeField] private float	secondsWaitAfterButtonClick = 0.2f;

    // Start is called before the first frame update
    void Start(){
		exit_game.onClick.AddListener(Exit_Game);
        play_button.onClick.AddListener(PlayButtonClicked);
		assign_left.onClick.AddListener(delegate {Assign("Left", assign_left);});
		assign_right.onClick.AddListener(delegate {Assign("Right", assign_right);});
		assign_down.onClick.AddListener(delegate {Assign("Down", assign_down);});
		assign_rotate.onClick.AddListener(delegate {Assign("Rotate", assign_rotate);});
		assign_music_switch.onClick.AddListener(SwitchMusicClicked);
		Keys_holder.ReadBindings();
		Refresh_display();
    }

	void OnGUI(){
		// listen for key rebinding
		if (is_listening){
			Event e = Event.current;
			if (e.isKey){
				Keys_holder.keys[listening_dir] = e.keyCode;
				Keys_holder.WriteNewBindings();
				Refresh_display();
				is_listening = false;
			}
		}
	}

	private void Exit_Game()
    {
		Application.Quit();
    }

	private void Refresh_display(){
		left_key_prewiew.text = Keys_holder.keys["Left"].ToString();
		assign_left.transform.GetChild(0).GetComponent<Text>().text = "Move left key";

		right_key_prewiew.text = Keys_holder.keys["Right"].ToString();
		assign_right.transform.GetChild(0).GetComponent<Text>().text = "Move right key";

		down_key_prewiew.text = Keys_holder.keys["Down"].ToString();
		assign_down.transform.GetChild(0).GetComponent<Text>().text = "Move down key";

		rotate_key_prewiew.text = Keys_holder.keys["Rotate"].ToString();
		assign_rotate.transform.GetChild(0).GetComponent<Text>().text = "Rotate key";

		if (SceneManagerData.IsMusicOn)
			music_switch_prewiew.text = "ON";
		else
			music_switch_prewiew.text = "OFF";
	}

	private void Assign(string key, Button button_clicked){
		if (!is_interactable){ return ; }
		UIClick.Play();
		if (!is_listening){
			is_listening = true;
			listening_dir = key;
			button_clicked.transform.GetChild(0).GetComponent<Text>().text = "Press any key";
		}
		else{
			Refresh_display();
			is_listening = false;
		}
	}

	private void SwitchMusicClicked()
	{
		if (!is_interactable) { return; }
		UIClick.Play();

		SceneManagerData.IsMusicOn = !SceneManagerData.IsMusicOn;
		if (SceneManagerData.IsMusicOn)
			music_switch_prewiew.text = "ON";
		else
			music_switch_prewiew.text = "OFF";
	}

	private void PlayButtonClicked(){
		if (!is_interactable){ return ; }
		UIClick.Play();
		StartCoroutine(Call_Scene_Load());
	}

	private IEnumerator Call_Scene_Load(){
		is_interactable = false;
		yield return new WaitForSeconds(secondsWaitAfterButtonClick);
		SceneLoader.Load("GameScene");
	}
	
	private bool		is_interactable = true;
	private	bool		is_listening = false;
	private string		listening_dir = "left";
}
