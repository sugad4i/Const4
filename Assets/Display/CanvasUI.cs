using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;

public class CanvasUIController : MonoBehaviour
{
    ///---
    // private RectTransform _rect_transform_text_ex_number_1, _rect_transform_image_circle;

    ////////////////////////////////////////////////////////////////////////////////////////
    ///--- for Excavator Object 
    // private GameObject _gob_excavator;
    // private ExcavatorController _excavator_controller;

    // private int _operation_mode = 0; ///--- { 0:Gamepad 1:Teach 2:Play 3:Play and Intervine 4: Experiment }
    // private int _state_play = 0;     ///--- { 0:Stop, 1:Play, 2:Pause }
    // private int[] _play_data_moment;
    // private int _count_play_data = 0;
    // private float _timer_sleep_max, _timer_sleep;


    ////////////////////////////////////////////////////////////////////////////////////////
    ///--- for Excavator Object 
    const int _number_excavator = 2;
    private GameObject[] _gob_excavator = new GameObject[_number_excavator];
    private ExcavatorController[] _excavator_controller = new ExcavatorController[_number_excavator]; 
    private string[] _name_excavator = { "Excavator1", "Excavator2" };
    private int[] _operation_mode = new int[_number_excavator]{ 0, 0 }; ///--- { 0:Gamepad 1:Teach 2:Play 3:Play and Intervine 4: Experiment }
    private int[] _state_play  = new int[_number_excavator]{ 0, 0 };    ///--- { 0:Stop, 1:Play, 2:Pause }
    private int[] _count_play_data = new int[_number_excavator]{ 0, 0 };
    private float[] _timer_sleep   = new float[_number_excavator]{ 0F, 0F};
    private float[] _timer_sleep_max = new float[_number_excavator]{ 5.0F, 5.0F };

    private int[,] _play_data_point = new int[2, 4] { { 100, 200, 300, 400 }, { 500, 600, 700, 800 } };

    ///--- for Image and Text Object
    private RectTransform _rect_transform_image_circle;
    private RectTransform[] _rect_transform_text_ex_number = new RectTransform[_number_excavator];
    private string[] _name_text_ex_number = { "Text_ex_number_1", "Text_ex_number_2" };
    private float[] _radius_circle = new float[_number_excavator];

    ///--- Start is called before the first frame update
    void Start()
    {
        ///--- Import Excavator Object
        // _gob_excavator        = GameObject.Find ("Excavator1");
        // _excavator_controller = _gob_excavator.GetComponent<ExcavatorController>();
        // // _play_data_moment = new int[4];
        // // _play_data_moment = _excavator_controller.GetPlayDataMoment();
        // _timer_sleep_max  = _excavator_controller.GetTimerSleepMax();
        // // Debug.Log( " _play_data_moment[0]:="+_play_data_moment[0]+" _play_data_moment[1]:="+_play_data_moment[1]+" _play_data_moment[3]:="+_play_data_moment[3] );
        ///--- Import Excavator Object 
        for( int i=0; i<_number_excavator; i++)
        {
            _gob_excavator[i] = GameObject.Find ( _name_excavator[i] );
            _excavator_controller[i] = _gob_excavator[i].GetComponent<ExcavatorController>();
        }

        ///--- Import Image and Text Object
        _rect_transform_image_circle = GameObject.Find("Image_state_circle").GetComponent<RectTransform>();
        for( int i=0; i<_number_excavator; i++)
        {
            _rect_transform_text_ex_number[i] = GameObject.Find(_name_text_ex_number[i]).GetComponent<RectTransform>();
            _radius_circle[i] = Mathf.Pow( (_rect_transform_image_circle.position.x-_rect_transform_text_ex_number[i].position.x),2F ) + Mathf.Pow( (_rect_transform_image_circle.position.y-_rect_transform_text_ex_number[i].position.y),2F );
            _radius_circle[i] = Mathf.Sqrt( _radius_circle[i] );
        }

        // Debug.Log( " _rect_transform_image_circle.position.x:="+_rect_transform_image_circle.position.x
        //           +" _rect_transform_image_circle.position.y:="+_rect_transform_image_circle.position.y　);
        // Debug.Log( " _rect_transform_text_ex_number_1.position.x:="+_rect_transform_text_ex_number_1.position.x
        //           +" _rect_transform_text_ex_number_1.position.y:="+_rect_transform_text_ex_number_1.position.y　);

        // _radius_circle = Mathf.Pow( (_rect_transform_image_circle.position.x-_rect_transform_text_ex_number_1.position.x),2F ) + Mathf.Pow( (_rect_transform_image_circle.position.y-_rect_transform_text_ex_number_1.position.y),2F );
        // _radius_circle = Mathf.Sqrt( _radius_circle );
        // Debug.Log( " _radius_circle:="+_radius_circle　);
    }

    ///--- Update is called once per frame
    void Update()
    {
        for( int i=0; i<_number_excavator; i++)
        {
            _operation_mode[i] = _excavator_controller[i].GetOperationMode();
            _state_play[i] = _excavator_controller[i].GetTeachPlayState();

            ///--- Play Mode or Play Intervene Mode
            if( _operation_mode[i] == 2 ||  _operation_mode[i] == 3 ||  _operation_mode[i] == 4 )
            {
                _count_play_data[i] = _excavator_controller[i].GetCountPlayData();
                float theta = 0F;

                if( _count_play_data[i] < _play_data_point[i, 0] )
                {
                    int max_count = _play_data_point[i, 0] + _play_data_point[i, 3] - _play_data_point[i, 1];
                    int offset_count = max_count - _play_data_point[i, 0];

                    theta = 120F * ( _count_play_data[i] + offset_count ) / max_count / 180F * 3.14146F ;
                }

                else if( _play_data_point[i, 1] < _count_play_data[i] && _count_play_data[i] < _play_data_point[i, 3] )
                {
                    int max_count = _play_data_point[i, 0] + _play_data_point[i, 3] - _play_data_point[i, 1];
                    int offset_count = _play_data_point[i, 1];

                    theta = 120F * ( _count_play_data[i] - offset_count ) / max_count / 180F * 3.14146F ;
                }

                else if( _count_play_data[i] == _play_data_point[i, 0] )
                {
                    _timer_sleep[i] = _excavator_controller[i].GetTimerSleep();
                    theta = (120F *  _timer_sleep[i] / _timer_sleep_max[i] + 120F ) / 180F * 3.14146F ;
                }

                else if( _play_data_point[i, 0] < _count_play_data[i] && _count_play_data[i] < _play_data_point[i, 1] )
                {
                    int max_count = _play_data_point[i, 1] - _play_data_point[i, 0];
                    int offset_count = _play_data_point[i, 0];

                    theta = (120F * ( _count_play_data[i] - offset_count ) / max_count + 240F) / 180F * 3.14146F ;
                }

                Vector2 pos;
                pos.x = _radius_circle[i] * Mathf.Sin( theta ) + _rect_transform_image_circle.position.x;
                pos.y = _radius_circle[i] * Mathf.Cos( theta ) + _rect_transform_image_circle.position.y;
                _rect_transform_text_ex_number[i].position = pos;
            }
        }

        

        // _operation_mode = _excavator_controller.GetOperationMode();
        // _state_play = _excavator_controller.GetTeachPlayState();

        // ///--- Play Mode or Play Intervene Mode
        // if( _operation_mode == 2 ||  _operation_mode == 3 ||  _operation_mode == 4 )
        // {
        //     _count_play_data = _excavator_controller.GetCountPlayData();
        //     float theta = 0F;

        //     if( _count_play_data < _play_data_moment[0] )
        //     {
        //         int max_count = _play_data_moment[0] + _play_data_moment[3] - _play_data_moment[1];
        //         int offset_count = max_count - _play_data_moment[0];

        //         theta = 120F * ( _count_play_data + offset_count ) / max_count / 180F * 3.14146F ;
        //     }

        //     else if( _play_data_moment[1] < _count_play_data && _count_play_data < _play_data_moment[3] )
        //     {
        //         int max_count = _play_data_moment[0] + _play_data_moment[3] - _play_data_moment[1];
        //         int offset_count = _play_data_moment[1];

        //         theta = 120F * ( _count_play_data - offset_count ) / max_count / 180F * 3.14146F ;
        //     }

        //     else if( _count_play_data == _play_data_moment[0] )
        //     {
        //         _timer_sleep = _excavator_controller.GetTimerSleep();
        //         theta = (120F *  _timer_sleep / _timer_sleep_max + 120F ) / 180F * 3.14146F ;
        //     }

        //     else if( _play_data_moment[0] < _count_play_data && _count_play_data < _play_data_moment[1] )
        //     {
        //         int max_count = _play_data_moment[1] - _play_data_moment[0];
        //         int offset_count = _play_data_moment[0];

        //         theta = (120F * ( _count_play_data - offset_count ) / max_count + 240F) / 180F * 3.14146F ;
        //     }

        //     Vector2 pos;
        //     pos.x = _radius_circle * Mathf.Sin( theta ) + _rect_transform_image_circle.position.x;
        //     pos.y = _radius_circle * Mathf.Cos( theta ) + _rect_transform_image_circle.position.y;
        //     _rect_transform_text_ex_number_1.position = pos;
        // }
        
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetPlayDataPoint( int number_excavator, int number_point, int data_point )
    {
        _play_data_point[number_excavator, number_point] = data_point;
    }

    public void SetTimerSleepMax( int number_excavator, float set_timer_sleep_max )
    {
        _timer_sleep_max[number_excavator] = set_timer_sleep_max;
    }
}
