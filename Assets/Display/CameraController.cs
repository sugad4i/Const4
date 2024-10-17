using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    ///--- for Excavator Object 
    const int _number_excavator = 3;
    private GameObject[] _gob_excavator = new GameObject[_number_excavator];
    private ExcavatorController[] _excavator_controller = new ExcavatorController[_number_excavator]; 
    private string[] _name_excavator = { "Excavator1", "Excavator2", "Excavator3" };
    private int _number_control_excavator = 0;

    ///--- for Canvas Object
    private GameObject[] _gob_canvas = new GameObject[_number_excavator];
    private CanvasController[] _canvas_controller = new CanvasController[_number_excavator];
    private string[] _name_canvas = { "CanvasExcavator1", "CanvasExcavator2" , "CanvasExcavator3" };

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    ///--- Start is called before the first frame update
    void Start()
    {        
        for( int i=0; i<_number_excavator; i++)
        {
            ///--- Import Excavator Object 
            _gob_excavator[i] = GameObject.Find ( _name_excavator[i] );
            _excavator_controller[i] = _gob_excavator[i].GetComponent<ExcavatorController>();

            ///--- Import Canvas Object
            _gob_canvas[i] = GameObject.Find ( _name_canvas[i] );
            _canvas_controller[i] = _gob_canvas[i].GetComponent<CanvasController>();

        }
    }

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    ///--- Update is called once per frame
    void Update()
    {
        for( int i=0; i<_number_excavator; i++)
        {
            if( i == _number_control_excavator )
            {
                _excavator_controller[i].SetCameraOn();
                _canvas_controller[i].SetCanvasOn();
            }
            else
            {
                _excavator_controller[i].SetCameraOff();
                _canvas_controller[i].SetCanvasOff();
            }
        }
    }
//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    public void ChangeControlExcavatorNumber( int change_number_control_excavator )
    {
        _number_control_excavator = change_number_control_excavator;
    }

}
