using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using KModkit;

public class SoilClassification : MonoBehaviour {
	private static int _moduleIdCounter = 1;
	private int _moduleId = 0;

	private bool _isSolved = false;

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Info;
	public KMSelectable[] btn;
	public KMSelectable left, right, submit;
	public TextMesh Screen;

	public GameObject[] shapes;
	public GameObject modulecentre;
	public float ypos;
	private double[] xarray = {-0.0611, -0.0464, -0.0188, 0.0022, 0.0196, -0.0358, -0.0613, -0.024, -0.0051, 0.0241, 0.0136, 0.0254, -0.0209, -0.0513, -0.0363, -0.0663, -0.0499, -0.0325, -0.0085, 0.0142, 0.0037, 0.0271, 0.0207, -0.0458, -0.0213, -0.0035, -0.0382, -0.0614, -0.0318, -0.0044, 0.0095, -0.0651};
	private double[] zarray = {-0.0064, 0.002, -0.008, -0.0088, -0.0122, -0.0122, 0.014, 0.0119, 0.0051, 0.0051, 0.0156, 0.0293, 0.0269, 0.0351, 0.027, 0.0462, 0.0521, 0.0404, 0.0433, 0.0369, 0.031, 0.0506, 0.0601, 0.0156, 0.0535, 0.0645, 0.066, 0.0645, 0.0017, 0.019, 0.0513, 0.0256};
	private int[] shapeValues = new int[32];
	private int randomIndex;
	private Vector3 spawnPos;
	private int answer;
	private string[] choices = {"Sand", "Loamy@ Sand", "Sandy@ Loam", "Loam", "Sandy@ Clay Loam", "Sandy@ Clay", "Clay Loam", "Clay", "Silty Clay", "Silty@ Clay Loam", "Silt Loam", "Silt"};
	private string[] noat = {"Sand", "Loamy Sand", "Sandy Loam", "Loam", "Sandy Clay Loam", "Sandy Clay", "Clay Loam", "Clay", "Silty Clay", "Silty Clay Loam", "Silt Loam", "Silt"};
	private int choicespointer = 0;
	private int sand = 0;
	private int silt = 0;
	private int clay = 0;
	private int sandshape;
	private int siltshape;
	private int clayshape;
	private int sandshapecount = 0;
	private int siltshapecount = 0;
	private int clayshapecount = 0;

	//Twitch Plays
	private readonly string TwitchHelpMessage = "Move through the options using the commands \"!{0} left\" and \"!{0} right\". Submit your answer with \"!{0} submit\"";
	private IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToLowerInvariant ();
		if (command.Equals ("right")) {
			yield return null;
			handleRight ();
		} else if (command.Equals ("left")) {
			yield return null;
			handleLeft ();
		} else if (command.Equals ("submit")) {
			yield return null;
			handleSubmit ();
		}
	}
	// Use this for initialization
	void Start () {
		_moduleId = _moduleIdCounter++;
		begin ();
	}
	void begin() {
		//Initialising shapes
		for (int i = 0; i < xarray.Length; i++){
			int j = i;
			randomIndex = Random.Range(0, shapes.Length);
			shapeValues [j] = randomIndex;
			spawnPos = new Vector3((float)xarray[j], ypos , (float)zarray[j]);
			GameObject newshape = (GameObject)Instantiate(shapes[randomIndex], modulecentre.transform);
			newshape.transform.localPosition = spawnPos;
			//var go = Instantiate(shapes[randomIndex], transform.position + spawnPos, Quaternion.identity);
			//go.transform.parent = modulecentre.transform;
		}

		//Finding correct shapes
		if (Info.GetBatteryCount() == 0 || Info.GetBatteryCount() >= 7){
			sandshape = 7;
			siltshape = 0;
			clayshape = 11;
			Debug.LogFormat ("[Soil Classification #{0}] Sand = Red Square", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Silt = Blue Circle", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Clay = Yellow Triangle", _moduleId);
		}else if (Info.GetBatteryCount() == 1 || Info.GetBatteryCount() == 2){
			sandshape = 2;
			siltshape = 9;
			clayshape = 4;
			Debug.LogFormat ("[Soil Classification #{0}] Sand = Blue Triangle", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Silt = Yellow Circle", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Clay = Green Square", _moduleId);
		}else if (Info.GetBatteryCount() == 3 || Info.GetBatteryCount() == 4){
			sandshape = 3;
			siltshape = 10;
			clayshape = 8;
			Debug.LogFormat ("[Soil Classification #{0}] Sand = Green Circle", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Silt = Yellow Square", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Clay = Red Triangle", _moduleId);
		}else{
			sandshape = 5;
			siltshape = 6;
			clayshape = 1;
			Debug.LogFormat ("[Soil Classification #{0}] Sand = Green Triangle", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Silt = Red Circle", _moduleId);
			Debug.LogFormat ("[Soil Classification #{0}] Clay = Blue Square", _moduleId);
		}

		//Counting the shapes
		for (int i = 0; i < shapeValues.Length; i++) {
			int j = i;
			if (shapeValues[j] == sandshape) {
				sandshapecount++;
			} else if (shapeValues[j] == siltshape){
				siltshapecount++;
			} else if (shapeValues[j] == clayshape){
				clayshapecount++;
			};
		};

		//Calculating Sand Percentage
		sand += (6 * (Info.GetBatteryCount(Battery.AA) / 2));
		Debug.LogFormat ("[Soil Classification #{0}] {1} pairs of AA batteries, sand% is now {2}", _moduleId, (Info.GetBatteryCount(Battery.AA)/2), sand);
		sand += (6 * Info.GetPortCount(Port.RJ45));
		Debug.LogFormat ("[Soil Classification #{0}] {1} RJ-45 port(s), sand% is now {2}", _moduleId, Info.GetPortCount(Port.RJ45), sand);
		sand += (9 * Info.GetPortCount(Port.DVI));
		Debug.LogFormat ("[Soil Classification #{0}] {1} DVI-D port(s), sand% is now {2}", _moduleId, Info.GetPortCount(Port.DVI), sand);
		sand += (4 * Info.GetOffIndicators().Count());
		Debug.LogFormat ("[Soil Classification #{0}] {1} unlit indicator(s), sand% is now {2}", _moduleId, Info.GetOffIndicators().Count(), sand);
		sand += (13 * sandshapecount);
		Debug.LogFormat ("[Soil Classification #{0}] {1} sand shapes(s), sand% is now {2}", _moduleId, sandshapecount, sand);
		sand += (-2 * clayshapecount);
		Debug.LogFormat ("[Soil Classification #{0}] {1} clay shapes(s), sand% is now {2}", _moduleId, clayshapecount, sand);
		if(sand < 0){
			sand = sand * -2;
			Debug.LogFormat ("[Soil Classification #{0}] sand was negative, sand% is now {1}", _moduleId, sand);
		};

		//Calculating Silt Percentage
		silt += (7 * Info.GetBatteryCount(Battery.D));
		Debug.LogFormat ("[Soil Classification #{0}] {1} D battery(s), silt% is now {2}", _moduleId, Info.GetBatteryCount(Battery.D), silt);
		silt += (7 * Info.GetPortCount(Port.StereoRCA));
		Debug.LogFormat ("[Soil Classification #{0}] {1} stereo RCA port(s), silt% is now {2}", _moduleId, Info.GetPortCount(Port.StereoRCA), silt);
		silt += (8 * Info.GetPortCount(Port.Parallel));
		Debug.LogFormat ("[Soil Classification #{0}] {1} parallel port(s), silt% is now {2}", _moduleId, Info.GetPortCount(Port.Parallel), silt);
		silt += (4 * Info.GetOnIndicators().Count());
		Debug.LogFormat ("[Soil Classification #{0}] {1} lit indicator(s), silt% is now {2}", _moduleId, Info.GetOnIndicators().Count(), silt);
		silt += (12 * siltshapecount);
		Debug.LogFormat ("[Soil Classification #{0}] {1} silt shapes(s), silt% is now {2}", _moduleId, siltshapecount, silt);
		silt += (-4 * clayshapecount);
		Debug.LogFormat ("[Soil Classification #{0}] {1} clay shapes(s), silt% is now {2}", _moduleId, clayshapecount, silt);
		if(silt < 0){
			silt = silt * -2;
			Debug.LogFormat ("[Soil Classification #{0}] silt was negative, silt% is now {1}", _moduleId, sand);
		};

		//Calculating Clay Percentage
		clay = 100 - sand - silt;
		Debug.LogFormat ("[Soil Classification #{0}] Clay% = 100 - {1} - {2} = {3}%", _moduleId, sand, silt, clay);
		if (clay >= 0) {
			Debug.LogFormat ("[Soil Classification #{0}] Clay% is positive.", _moduleId);
		} else {
			clay = -1 * clay;
			sand -= clay;
			silt -= clay;
			Debug.LogFormat ("[Soil Classification #{0}] Clay% is negative, new clay% is {1}, new sand% is {2}, new silt% is {3}.", _moduleId, clay, sand, silt);
			if (sand >= 0 && silt >= 0) {
				Debug.LogFormat ("[Soil Classification #{0}] All are now positive and thus are final.", _moduleId);
			} else if (sand < 0 && silt >= 0) {
				Debug.LogFormat ("[Soil Classification #{0}] Sand is now negative, so sand is now 0.", _moduleId);
				if (clay >= silt) {
					clay += sand;
					sand = 0;
					Debug.LogFormat ("[Soil Classification #{0}] Clay is now {1}.", _moduleId, clay);
				} else {
					silt += sand;
					sand = 0;
					Debug.LogFormat ("[Soil Classification #{0}] Silt is now {1}.", _moduleId, silt);
				}
			} else if (sand >= 0 && silt < 0) {
				Debug.LogFormat ("[Soil Classification #{0}] Silt is now negative, so silt is now 0.", _moduleId);
				if (clay >= sand) {
					clay += silt;
					silt = 0;
					Debug.LogFormat ("[Soil Classification #{0}] Clay is now {1}.", _moduleId, clay);
				} else {
					sand += silt;
					silt = 0;
					Debug.LogFormat ("[Soil Classification #{0}] Sand is now {1}.", _moduleId, sand);
				}
			} else {
				Debug.LogFormat ("[Soil Classification #{0}] Looks like something went wrong, please send your log to Zappyjro#6965 on discord!", _moduleId);
			};
		}

		//Determining the answer
		if (sand >= 90) {
			answer = 0; 
		} else if (clay > 60) {
			answer = 7;
		} else if (clay > 55) {
			if (silt > 40) {
				answer = 8;
			} else {
				answer = 7;
			}
			;
		} else if (clay > 40) {
			if (sand >= 45) {
				answer = 5;
			} else if (silt > 40) {
				answer = 8;
			} else {
				answer = 7;
			}
			;
		} else if (clay > 35) {
			if (sand >= 45) {
				answer = 5;
			} else if (sand < 20) {
				answer = 9;
			} else {
				answer = 6;
			}
			;
		} else if (clay > 27) {
			if (sand >= 45) {
				answer = 4;
			} else if (sand < 20) {
				answer = 9;
			} else {
				answer = 6;
			}
			;
		} else if (clay > 20) {
			if (silt <= 27) {
				answer = 4;
			} else if (silt > 50) {
				answer = 10;
			} else {
				answer = 3;
			}
			;
		} else if (clay > 15) {
			if (sand > 52) {
				answer = 2;
			} else if (silt > 50) {
				answer = 10;
			} else {
				answer = 3;
			}
			;
		} else if (clay > 12) {
			if (silt > 50) {
				answer = 10;
			} else if (sand < 53) {
				answer = 3;
			} else if (sand >= (70 + clay)) {
				answer = 1;
			} else {
				answer = 2;
			}
			;
		} else if (clay > 7) {
			if (silt > 80) {
				answer = 11;
			} else if (silt > 50) {
				answer = 10;
			} else if (sand < 53) {
				answer = 3;
			} else {
				if (clay > 10) {
					if (sand >= (70 + clay)) {
						answer = 1;
					} else {
						answer = 2;
					}
					;
				} else {
					if (clay <= ((sand - 85) * 2)) {
						answer = 0;
					} else if (sand >= (70 + clay)) {
						answer = 1;
					} else {
						answer = 2;
					}
					;
				}
				;
			}
			;
		} else {
			if (silt > 80) {
				answer = 11;
			} else if (silt > 50) {
				answer = 10;
			} else if (clay <= ((sand - 85) * 2)) {
				answer = 0;
			} else if (sand >= (70 + clay)) {
				answer = 1;
			} else {
				answer = 2;
			};
		};
		Debug.LogFormat ("[Soil Classification #{0}] The soil type is {1}", _moduleId, noat[answer]);

		//Button Handling
		left.OnInteract += delegate () {
			handleLeft();
			return false;
		};
		right.OnInteract += delegate () {
			handleRight();
			return false;
		};
		submit.OnInteract += delegate () {
			handleSubmit();
			return false;
		};

	}

	void handleLeft() {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left.transform);
		left.AddInteractionPunch();

		if (_isSolved)
			return;

		if (choicespointer == 0) {
			choicespointer = 11;
		} else {
			choicespointer = choicespointer - 1;
		}

		return;
	}
	void handleRight() {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, right.transform);
		right.AddInteractionPunch();

		if (_isSolved)
			return;

		if (choicespointer == 11) {
			choicespointer = 0;
		} else {
			choicespointer = choicespointer + 1;
		}

		return;
	}
	void handleSubmit() {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
		right.AddInteractionPunch();

		if (_isSolved)
			return;

		if (choicespointer == answer) {
			Debug.LogFormat ("[Soil Classification #{0}] Correct answer submitted, module disarmed.", _moduleId);
			Audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.CorrectChime, submit.transform);
			Module.HandlePass ();
			_isSolved = true;
		} else {
			Debug.LogFormat ("[Soil Classification #{0}] {1} submitted, this is incorrect, strike issued.", _moduleId, noat [choicespointer]);
			Module.HandleStrike();
		}

		return;
	}
	
	// Update is called once per frame
	void Update () {
		choices [choicespointer] = choices [choicespointer].Replace ("@", System.Environment.NewLine);
		Screen.text = choices [choicespointer];
	}
}
