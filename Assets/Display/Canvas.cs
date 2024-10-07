using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    ///--- 
    private Image _image_stop, _image_teach, _image_play, _image_pause;

    ///--- for Excavator Object 
    string excavator_name;
    private GameObject gob_excavator;
    private ExcavatorController excavator_controller;

    private int _operation_mode = 0; ///--- { 0:Nomal, 1:Teach, 2:Play }
    private int _state_play = 0;     ///--- { 0:Stop, 1:Play, 2:Pause }
    private string _state_name = "none";

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    ///--- Start is called before the first frame update
    void Start()
    {
        string canvas_name = transform.root.gameObject.name;
        
        ///--- Import Image
        _image_stop  = transform.Find("Image_stop").GetComponent<Image>();
        _image_teach = transform.Find("Image_teach").GetComponent<Image>();
        _image_play  = transform.Find("Image_play").GetComponent<Image>();
        _image_pause = transform.Find("Image_pause").GetComponent<Image>();
        _image_stop.enabled = _image_teach.enabled = _image_play.enabled = _image_pause.enabled = false;

        ///--- Import Excavator Object
        excavator_name       = canvas_name.Replace("Canvas","");
        gob_excavator        = GameObject.Find (excavator_name);
        excavator_controller = gob_excavator.GetComponent<ExcavatorController>();
        Debug.Log( " canvas_name:="+canvas_name+" excavator_name:="+excavator_name );
    }

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    ///--- Update is called once per frame
    void Update()
    {
        _operation_mode = excavator_controller.GetOperationMode();
        _state_play = excavator_controller.GetTeachPlayState();
        
        ///--- Normal Mode
        if( _operation_mode == 0 )
        {
            _image_stop.enabled = _image_teach.enabled = _image_play.enabled = _image_pause.enabled = false;
        }

        ///--- Teach Mode
        else if( _operation_mode == 1 )
        {
            // Debug.Log( " _state_play:="+_state_play );
            _image_play.enabled = _image_pause.enabled = false;

            ///--- Stop
            if( _state_play == 0 )
            {
                _image_stop.enabled  = true;
                _image_teach.enabled = false;
            }
            else if( _state_play == 1 )
            {
                _image_stop.enabled  = false;
                _image_teach.enabled = true;
            }
        }

        ///--- Play Mode Mode
        else if( _operation_mode == 2 )
        {
            _image_teach.enabled = false;

            ///--- Stop
            if( _state_play == 0 )
            {
                _image_stop.enabled  = true;
                _image_play.enabled = _image_pause.enabled = false;
            }

            ///--- Play
            else if( _state_play == 1 )
            {
                _image_play.enabled  = true;
                _image_stop.enabled  = _image_pause.enabled = false;
            }

            ///--- Pause
            else if( _state_play == 2 )
            {
                _image_pause.enabled = true;
                _image_stop.enabled  = _image_play.enabled  = false;
            }
        }

        ///--- Play Intervene or Experiment Mode
        else if( _operation_mode == 3 || _operation_mode == 4 )
        {
            _image_teach.enabled = false;

            ///--- Stop
            if( _state_play == 0 )
            {
                _image_stop.enabled  = true;
                _image_play.enabled = _image_pause.enabled = false;
            }

            ///--- Play
            else if( _state_play == 1 )
            {
                _state_name = excavator_controller.GetExcavatorStateName();
                if( _state_name == "Drilling" )
                    _image_play.sprite = Resources.Load<Sprite>("Image/play_buttun_blue");
                else
                    _image_play.sprite = Resources.Load<Sprite>("Image/play_buttun_green");

                _image_play.enabled  = true;
                _image_stop.enabled  = _image_pause.enabled = false;

                // Debug.Log( excavator_name+" state_name:="+_state_name );
            }

            ///--- Pause
            else if( _state_play == 2 )
            {
                _image_pause.enabled = true;
                _image_stop.enabled  = _image_play.enabled  = false;
            }
        }

        // Debug.Log( excavator_name+" "+_image_stop.enabled+" "+_image_teach.enabled+" "+_image_play.enabled+" "+_image_pause.enabled );
    }
//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    public void SetCanvasOn()
    {
        this.GetComponent<Canvas>().enabled = true;
    }

    public void SetCanvasOff()
    {
        this.GetComponent<Canvas>().enabled = false;
    }
}
