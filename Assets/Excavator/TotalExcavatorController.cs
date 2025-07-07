using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;          // for Guid,Console
using System.Text;     // for OutPutText
using System.IO;       // for OutPutText

using Const;
public class InputCommand
{
    private string _name;
    private float _delta_swing;
    private float _delta_boom;
    private float _delta_arm;
    private float _delta_bucket;
    private float _delta_trackRight;
    private float _delta_trackLeft;

    private float _delta_pos_x;
    private float _delta_pos_y;
    private float _delta_pos_z;

    public InputCommand( string name )
    {
        this._name = name;
        this.Clear();
    }

    public InputCommand()
    {
        Clear();
    }

    public void Clear()
    {
        ///--- Joint Control
        this._delta_swing = 0F;
        this._delta_boom = 0F;
        this._delta_arm = 0F;
        this._delta_bucket = 0F;
        this._delta_trackRight = 0F;
        this._delta_trackLeft = 0F;

        ///--- Position Control
        this._delta_pos_x = 0F;
        this._delta_pos_y = 0F;
    }

    public float delta_swing
    {
        set { _delta_swing = value; }
        get { return _delta_swing; }
    }

    public float delta_boom
    {
        set { _delta_boom = value; }
        get => _delta_boom;
    }

    public float delta_arm
    {
        set { _delta_arm = value; }
        get => _delta_arm;
    }

    public float delta_bucket
    {
        set { _delta_bucket = value; }
        get => _delta_bucket;
    }

    public float delta_trackRight
    {
        set { _delta_trackRight = value; }
        get => _delta_trackRight;
    }

    public float delta_trackLeft
    {
        set { _delta_trackLeft = value; }
        get => _delta_trackLeft;
    }

    public float delta_pos_x
    {
        set { _delta_pos_x = value; }
        get => _delta_pos_x;
    }
    
    public float delta_pos_y
    {
        set { _delta_pos_y = value; }
        get => _delta_pos_y;
    }

    public float delta_pos_z
    {
        set { _delta_pos_z = value; }
        get => _delta_pos_z;
    }
}

public enum ExcavatorControlState
{
    AUTO,
    WAITING,
    INTERVENING
}
public class TotalExcavatorController : MonoBehaviour
{
    ///--- for Excavator Object 

    const int _number_excavator = 3;
    //private int[] excavator_state;
    private ExcavatorControlState[] excavator_state = new ExcavatorControlState[_number_excavator];
    private int _currentExcavator = -1; // 現在介入中の掘削機のインデックス
    private Queue<int> _waitingQueue = new Queue<int>(); // 掘削機の待機キューを定義

    private GameObject[] _gob_excavator = new GameObject[_number_excavator];
    private ExcavatorController[] _excavator_controller = new ExcavatorController[_number_excavator];
    private ExcavatorState _excavator_state = new ExcavatorState();
    private string[] _name_excavator = { "Excavator1", "Excavator2", "Excavator3" };
    private int _number_control_excavator = 0;

    ///--- for Camera Object
    private GameObject _gob_camera_controller;
    private CameraController _camera_controller;

    ///--- for CanvsUI Object
    private GameObject _gob_canvas_ui;
    private CanvasUIController _canvas_ui_controller;

    ///--- for Comand Values
    private InputCommand input_command = new InputCommand();
    private InputCommand input_command_off = new InputCommand();
    private InputCommand[] input_command_ex = new InputCommand[3];
    // bool travel = false;
    private int _operation_mode = 0; ///--- { 0:Gamepad 1:Teach 2:Play 3:Play and Intervine 4: Experiment }
    private int _control_mode = 0;   ///--- { 0:Joint Control by Gamepad, 1:Position Control by Gamepad }

    ///--- for Teach and Play
    private float _timer_laps = 0F;
    bool flag_start_teach = false;
    bool flag_teach_init = false;

    ///--- Play Data Point of Excavator State Change
    private static readonly int[,] _play_data_point = new int[_number_excavator, 4] { { 4, 718, 1149, 1711 }, { 4, 718, 1149, 1711 }, { 4, 718, 1149, 1711 } };
    private static readonly float[] _timer_sleep_max = new float[_number_excavator] { 0.1F, 0.1F, 0.1F };

    //////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////

    ///--- Start is called before the first frame update
    void Start()
    {


        ///--- Set Target FPS at 60
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        ///--- Import Excavator Object 
        for (int i = 0; i < _number_excavator; i++)
        {
            _gob_excavator[i] = GameObject.Find(_name_excavator[i]);
            _excavator_controller[i] = _gob_excavator[i].GetComponent<ExcavatorController>();

            input_command_ex[i] = new InputCommand(_name_excavator[i]);
        }
        input_command_off.Clear();

        excavator_state = new ExcavatorControlState[_number_excavator];

        // 各機体の初期状態を0（自動区間）に設定
        for (int i = 0; i < _number_excavator; i++)
        {
            excavator_state[i] = ExcavatorControlState.AUTO;
        }

        ///--- Import Camera Object 
        _gob_camera_controller = GameObject.Find("CameraController");
        _camera_controller = _gob_camera_controller.GetComponent<CameraController>();

        ///--- Import CanvsUI Object 
        _gob_canvas_ui = GameObject.Find("CanvasUI");
        _canvas_ui_controller = _gob_canvas_ui.GetComponent<CanvasUIController>();

        ///--- Set Play Data Point
        for (int i = 0; i < _number_excavator; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                _excavator_controller[i].SetPlayDataPoint(j, _play_data_point[i, j]);
                _canvas_ui_controller.SetPlayDataPoint(i, j, _play_data_point[i, j]);
            }

            _excavator_controller[i].SetTimerSleepMax(_timer_sleep_max[i]);
            _canvas_ui_controller.SetTimerSleepMax(i, _timer_sleep_max[i]);
        }


    }

    ///--- Update is called once per frame
    void Update()
    {

        //////////   Change Excavator Operation Mode   /////////////////////////////////////////////////////////////////////////////////////
        ///--- Change Teach and Play Flag
        if (Input.GetKey(KeyCode.Q))
        {
            _operation_mode = 0;
            _control_mode = 0;
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetCommandZero();
                input_command_ex[i].Clear();

                _excavator_controller[i].SetOperationMode(_operation_mode);
                _excavator_controller[i].SetControlMode(_control_mode);
            }
            Debug.Log(" Change Normal Control Mode.");
        }

        else if (Input.GetKey(KeyCode.W))
        {
            _operation_mode = 1;
            _control_mode = 1;
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetCommandZero();
                input_command_ex[i].Clear();

                _excavator_controller[i].SetOperationMode(_operation_mode);
                _excavator_controller[i].SetControlMode(_control_mode);
            }

            flag_start_teach = flag_teach_init = false;

            Debug.Log(" Change Teach Mode.");
        }
        else if (Input.GetKey(KeyCode.E))
        {
            _operation_mode = 2;
            _control_mode = 1;
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetCommandZero();
                input_command_ex[i].Clear();

                _excavator_controller[i].SetOperationMode(_operation_mode);
                _excavator_controller[i].SetControlMode(_control_mode);
            }

            Debug.Log(" Change Play Mode.");
        }
        else if (Input.GetKey(KeyCode.R))
        {
            _operation_mode = 3;
            _control_mode = 1;
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetCommandZero();
                input_command_ex[i].Clear();

                _excavator_controller[i].SetOperationMode(_operation_mode);
                _excavator_controller[i].SetControlMode(_control_mode);
            }

            Debug.Log(" Change Play and Intervine Mode.");
        }
        else if (Input.GetKey(KeyCode.T))
        {
            _operation_mode = 4;
            _control_mode = 1;
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetCommandZero();
                input_command_ex[i].Clear();

                _excavator_controller[i].SetOperationMode(_operation_mode);
                _excavator_controller[i].SetControlMode(_control_mode);
            }

            Debug.Log(" Change Experimet Mode.");
        }
        else if (Input.GetKey(KeyCode.Y))
        {
            _operation_mode = 5;
            _control_mode = 1;
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetCommandZero();
                input_command_ex[i].Clear();

                _excavator_controller[i].SetOperationMode(_operation_mode);
                _excavator_controller[i].SetControlMode(_control_mode);
            }

            Debug.Log(" Change FIFO Mode.");
        }

        //////////   Control in Each Operation Mode   /////////////////////////////////////////////////////////////////////////////////////
        ///--- Normal Gamepad Control Mode
        if (_operation_mode == 0)
        {
            GamepadControlMode();
        }
        ///--- Teach Mode
        else if (_operation_mode == 1)
        {
            TeachMode();
        }
        ///--- Play Mode
        else if (_operation_mode == 2)
        {
            PlayMode();
        }
        ///--- Play and Intervene Mode
        else if (_operation_mode == 3)
        {
            PlayInterveneMode();
        }
        ///--- Experiment Mode
        else if (_operation_mode == 4)
        {
            ExperimentMode();
        }
        ///--- FIFO Mode
        else if (_operation_mode == 5)
        {
            FIFOMode();
            Debug.Log("FIFOMODE");
        }

        int intervention = InterventionMode();
        // interventionが1なら介入モード、0なら通常モード
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////

    public int InterventionMode()
    {
        // movemode, bluemodeが0以外なら1、両方0なら0
        for (int i = 0; i < stopper.movemode.Length; i++)
        {
            if (stopper.movemode[i] != 0 || stopper.bluemode[i] != 0)
            {
                Debug.Log("介入中");
                return 1; // 1台でも0以外なら介入モード

            }
        }
        Debug.Log("自動運転中");
        return 0; // 全て0なら通常モード
    }

    private void GamepadControlMode()
    {
        CheckControlExcavatorNumber();
        CheckCameraMode();

        ///--- Change Control Mode
        if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.P))
        {
            if (Input.GetKey(KeyCode.J))
                _control_mode = 0;
            else if (Input.GetKey(KeyCode.P))
                _control_mode = 1;

            for (int i = 0; i < _number_excavator; i++)
                _excavator_controller[i].SetControlMode(_control_mode);
        }

        ///--- Joint control mode
        if (_control_mode == 0)
        {
            CalculateJointControlCommandByGamepad();

            // Debug.Log( "0 _num_ce:= "+_number_control_excavator+" input_command:="+input_command.delta_swing+" ex[0].delta_swing:="+input_command_ex[0].delta_swing+" ex[1].delta_swing:="+input_command_ex[1].delta_swing );
            for (int i = 0; i < _number_excavator; i++)
            {
                if (i == _number_control_excavator)
                    input_command_ex[i] = input_command;
                else
                    input_command_ex[i] = input_command_off;

                _excavator_controller[i].SetInputCommand(input_command_ex[i]);
            }
        }

        ///--- Position control mode
        else if (_control_mode == 1)
        {
            CalculatePositionControlCommandByGamepad();
            // Debug.Log( "0 _num_ce:= "+_number_control_excavator+" input_command:="+input_command.delta_swing+" ex[0].delta_swing:="+input_command_ex[0].delta_swing+" ex[1].delta_swing:="+input_command_ex[1].delta_swing );
            for (int i = 0; i < _number_excavator; i++)
            {
                if (i == _number_control_excavator)
                    input_command_ex[i] = input_command;
                else
                    input_command_ex[i] = input_command_off;

                _excavator_controller[i].SetInputCommand(input_command_ex[i]);
            }
        }
    }

    private void TeachMode()
    {
        if (flag_start_teach == false)
            CheckControlExcavatorNumber();
        CheckCameraMode();

        ///--- Check Start Save Data Flag
        if (flag_start_teach == false && Input.GetKey(KeyCode.S))
        {
            _timer_laps = 0F;
            flag_start_teach = true;
            _excavator_controller[_number_control_excavator].StartTeachPlay();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Start Save Excavator State Data.");
        }
        else if (flag_start_teach == true && Input.GetKey(KeyCode.F))
        {
            flag_start_teach = flag_teach_init = false;
            _excavator_controller[_number_control_excavator].StopTeachPlay();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Stop Save Excavator State Data. Time:=" + _timer_laps);
        }

        ///--- Save Data
        if (flag_start_teach)
        {
            ///--- Update Lap Time
            _timer_laps += Time.deltaTime;

            string file_name = _name_excavator[_number_control_excavator] + "_trajectory_data.csv";
            if (!flag_teach_init)
            {
                ///--- Prepare Save Data    
                StreamWriter sw_title = new StreamWriter(@"Assets/Excavator/TrajectoryData/Teach_" + file_name, false, Encoding.GetEncoding("Shift_JIS"));
                string[] s1 = { "pos_x", "pos_y", "pos_z", "swing", "arm", "boom", "bucket", "trackRight", "trackLeft" };
                string s2 = string.Join(",", s1);
                sw_title.WriteLine(s2);
                sw_title.Close();

                flag_teach_init = true;
            }

            ///--- Update Excavator State
            _excavator_state = _excavator_controller[_number_control_excavator].GetExcavatorState();
            Debug.Log("  _excavator_state.swing:=" + _excavator_state.swing + " _input_swing:=");

            ///--- Save Excavator State Data
            StreamWriter sw = new StreamWriter(@"Assets/Excavator/TrajectoryData/Teach_" + file_name, true, Encoding.GetEncoding("Shift_JIS"));
            string[] str = { _excavator_state.pos_x.ToString("f4"), _excavator_state.pos_y.ToString("f4"), _excavator_state.pos_z.ToString("f4"),
                             _excavator_state.swing.ToString("f4"), _excavator_state.arm.ToString("f4"), _excavator_state.boom.ToString("f4"),
                             _excavator_state.bucket.ToString("f4"), _excavator_state.trackRight.ToString("f4"), _excavator_state.trackLeft.ToString("f4") };
            string str2 = string.Join(",", str);
            sw.WriteLine(str2);
            sw.Close();

            ///--- Position Control bu Gamepad
            CalculatePositionControlCommandByGamepad();
            for (int i = 0; i < _number_excavator; i++)
            {
                if (i == _number_control_excavator)
                    input_command_ex[i] = input_command;
                else
                    input_command_ex[i] = input_command_off;

                _excavator_controller[i].SetInputCommand(input_command_ex[i]);
            }
        }

    }

    private void PlayMode()
    {
        CheckControlExcavatorNumber();
        CheckCameraMode();

        ///--- Go To Initial Position
        if (Input.GetKey(KeyCode.A))
        {
            _excavator_controller[_number_control_excavator].SetInitialPosition();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Start Play Excavator Data.");
        }
        ///--- Start Play Data
        if (Input.GetKey(KeyCode.S))
        {
            _excavator_controller[_number_control_excavator].StartTeachPlay();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Start Play Excavator Data.");
        }
        ///--- Pause Play Data
        else if (Input.GetKey(KeyCode.D))
        {
            _excavator_controller[_number_control_excavator].PuasePlayData();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Pause Play Excavator Data.");
        }
        ///--- Stop Play Data
        else if (Input.GetKey(KeyCode.F))
        {
            _excavator_controller[_number_control_excavator].StopPlayData();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Stop Play Excavator Data.");
        }
    }

    private void PlayInterveneMode()
    {
        CheckControlExcavatorNumber();
        CheckCameraMode();

        ///--- Go To Initial Position
        if (Input.GetKey(KeyCode.A))
        {
            for (int i = 0; i < _number_excavator; i++)
                _excavator_controller[_number_control_excavator].SetInitialPosition();
        }
        ///--- Start Play Data
        if (Input.GetKey(KeyCode.S))
        {
            _excavator_controller[_number_control_excavator].StartTeachPlay();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Start Play Excavator Data.");
        }
        ///--- Pause Play Data
        else if (Input.GetKey(KeyCode.D))
        {
            _excavator_controller[_number_control_excavator].PuasePlayData();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Pause Play Excavator Data.");
        }
        ///--- Stop Play Data
        else if (Input.GetKey(KeyCode.F))
        {
            _excavator_controller[_number_control_excavator].StopPlayData();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Stop Play Excavator Data.");
        }
        ///--- Start Intervene
        if (Input.GetKeyDown("joystick button 7")) ///--- Push Joystick Start Buttun
        {
            _excavator_controller[_number_control_excavator].SetFlagIntervene();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Start Intervene Excavator.");
        }

        ///--- Set Intervene Input Value
        CalculatePositionControlCommandByGamepad();
        for (int i = 0; i < _number_excavator; i++)
        {
            if (i == _number_control_excavator)
                input_command_ex[i] = input_command;
            else
                input_command_ex[i] = input_command_off;

            _excavator_controller[i].SetInputCommand(input_command_ex[i]);
        }

    }

    private void ExperimentMode()
    {
        CheckControlExcavatorNumber();
        CheckCameraMode();

        ///--- Go To Initial Position
        if (Input.GetKey(KeyCode.A))
        {
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetInitialPosition();
                Debug.Log(" Excavator" + (i + 1) + " Start Play Excavator Data.");
            }
        }
        ///--- Start Play Data
        if (Input.GetKey(KeyCode.S))
        {
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].StartTeachPlay();
                Debug.Log(" Excavator" + (i + 1) + " Start Play Excavator Data.");
            }
        }
        ///--- Pause Play Data
        else if (Input.GetKey(KeyCode.D))
        {
            _excavator_controller[_number_control_excavator].PuasePlayData();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Pause Play Excavator Data.");

        }
        ///--- Stop Play Data
        else if (Input.GetKey(KeyCode.F))
        {
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].StopPlayData();
                Debug.Log(" Excavator" + (i + 1) + " Stop Play Excavator Data.");
            }
        }


        if (Input.GetKey(KeyCode.Z))
        {
            _excavator_controller[0].StartTeachPlay();
            Debug.Log(" Excavator" + (1) + " Start Play Excavator Data.");
        }
        if (Input.GetKey(KeyCode.X))
        {
            _excavator_controller[1].StartTeachPlay();
            Debug.Log(" Excavator" + (2) + " Start Play Excavator Data.");
        }
        if (Input.GetKey(KeyCode.V))
        {
            _excavator_controller[2].StartTeachPlay();
            Debug.Log(" Excavator" + (3) + " Start Play Excavator Data.");
        }


        ///--- Start Intervene
        if (Input.GetKeyDown("joystick button 7")) ///--- Push Joystick Start Buttun
        {
            _excavator_controller[_number_control_excavator].SetFlagIntervene();
            Debug.Log(" Excavator" + (_number_control_excavator + 1) + " Start Intervene Excavator.");
        }

        ///--- Set Intervene Input Value
        CalculatePositionControlCommandByGamepad();
        for (int i = 0; i < _number_excavator; i++)
        {
            if (i == _number_control_excavator)
                input_command_ex[i] = input_command;
            else
                input_command_ex[i] = input_command_off;

            _excavator_controller[i].SetInputCommand(input_command_ex[i]);
        }
    }

    private void FIFOMode()
    {
        CheckControlExcavatorNumber();
        CheckCameraMode();

        ///--- Go To Initial Position
        if (Input.GetKey(KeyCode.A))
        {
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].SetInitialPosition();
                Debug.Log(" Excavator" + (i + 1) + " Set initial position.");
            }
        }
        ///--- Start Play Data
        if (Input.GetKey(KeyCode.S))
        {
            for (int i = 0; i < _number_excavator; i++)
            {
                _excavator_controller[i].StartTeachPlay();
                Debug.Log(" Excavator" + (i + 1) + " Start Play Excavator Data.");
            }

        }
        if (Input.GetKey(KeyCode.Z))
        {
            _excavator_controller[0].StartTeachPlay();
            Debug.Log(" Excavator" + (1) + " Start Play Excavator Data.");
        }
        if (Input.GetKey(KeyCode.X))
        {
            _excavator_controller[1].StartTeachPlay();
            Debug.Log(" Excavator" + (2) + " Start Play Excavator Data.");
        }
        if (Input.GetKey(KeyCode.V))
        {
            _excavator_controller[2].StartTeachPlay();
            Debug.Log(" Excavator" + (3) + " Start Play Excavator Data.");
        }

        ///--- Change Control Mode
        if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.P))
        {
            if (Input.GetKey(KeyCode.J))
                _control_mode = 0;
            else if (Input.GetKey(KeyCode.P))
                _control_mode = 1;

            for (int i = 0; i < _number_excavator; i++)
                _excavator_controller[i].SetControlMode(_control_mode);
        }

        ///--- Joint control mode
        if (_control_mode == 0)
        {
            CalculateJointControlCommandByGamepad();

            // Debug.Log( "0 _num_ce:= "+_number_control_excavator+" input_command:="+input_command.delta_swing+" ex[0].delta_swing:="+input_command_ex[0].delta_swing+" ex[1].delta_swing:="+input_command_ex[1].delta_swing );
            for (int i = 0; i < _number_excavator; i++)
            {
                if (i == _number_control_excavator)
                    input_command_ex[i] = input_command;
                else
                    input_command_ex[i] = input_command_off;

                _excavator_controller[i].SetInputCommand(input_command_ex[i]);
            }
        }

        ///--- Position control mode
        else if (_control_mode == 1)
        {
            CalculatePositionControlCommandByGamepad();
            // Debug.Log( "0 _num_ce:= "+_number_control_excavator+" input_command:="+input_command.delta_swing+" ex[0].delta_swing:="+input_command_ex[0].delta_swing+" ex[1].delta_swing:="+input_command_ex[1].delta_swing );
            for (int i = 0; i < _number_excavator; i++)
            {
                if (i == _number_control_excavator)
                    input_command_ex[i] = input_command;
                else
                    input_command_ex[i] = input_command_off;

                _excavator_controller[i].SetInputCommand(input_command_ex[i]);
            }
        }

    }

    public void RequestIntervention(int id)
{
    if (excavator_state[id] == ExcavatorControlState.AUTO)
    {
        excavator_state[id] = ExcavatorControlState.WAITING;
        _waitingQueue.Enqueue(id);
        Debug.Log($"Excavator {id + 1} is waiting for intervention.");
        StartNextExcavator();
    }
}

    private void StartNextExcavator()
    {
        if (_currentExcavator == -1 && _waitingQueue.Count > 0)
        {
            int nextExcavator = _waitingQueue.Dequeue();
            excavator_state[nextExcavator] = ExcavatorControlState.INTERVENING;
            _currentExcavator = nextExcavator;

            _excavator_controller[_currentExcavator].StartTeachPlay();
            Debug.Log($"Excavator {_currentExcavator + 1} started intervention.");
        }
    }
    private void TryStartNextIntervention()
    {
        if (_currentExcavator == -1 && _waitingQueue.Count > 0)
        {
            int next = _waitingQueue.Dequeue();
            excavator_state[next] = ExcavatorControlState.INTERVENING;
            _currentExcavator = next;

            //_excavator_controller[next].StartTeachPlay();
            Debug.Log($"Excavator {next + 1} started intervention.");
        }
    }
    public void EndIntervention(int id)
    {
        if (_currentExcavator == id)
        {
            //_excavator_controller[id].StopTeachPlay();
            excavator_state[id] = ExcavatorControlState.AUTO;
            _currentExcavator = -1;

            Debug.Log($"Excavator {id + 1} finished intervention.");
            StartNextExcavator();
        }
    }
    private void ShowQueueStatus()
    {
        // キューの状態を表示する（デバッグ用）
        string queueStatus = "Queue: ";
        foreach (int exc in _waitingQueue)
        {
            queueStatus += "Excavator " + (exc + 1) + " ";
        }
        Debug.Log(queueStatus);
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////
    private void CheckControlExcavatorNumber()
    {
        if (Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Keypad3) || Input.GetKeyDown("joystick button 5"))
        {
            if (_operation_mode == 1 || _operation_mode == 2 || _operation_mode == 3)
            {
                for (int i = 0; i < _number_excavator; i++)
                {
                    _excavator_controller[i].SetCommandZero();
                    input_command_ex[i].Clear();
                }
            }

            if (Input.GetKey(KeyCode.Keypad1))
                _number_control_excavator = 0;

            else if (Input.GetKey(KeyCode.Keypad2))
                _number_control_excavator = 1;

            else if (Input.GetKey(KeyCode.Keypad3))
                _number_control_excavator = 2;

            else if (Input.GetKeyDown("joystick button 5"))
            {
                _number_control_excavator++;
                if (_number_control_excavator == _number_excavator)
                    _number_control_excavator = 0;
            }

            ///--- Change Cemra and Canvas of Display1
            _camera_controller.ChangeControlExcavatorNumber(_number_control_excavator);

            Debug.Log(" Change Control Excavator" + (_number_control_excavator + 1));
        }
    }

    private void CheckCameraMode()
    {
        if (Input.GetKeyDown("joystick button 4"))
            _excavator_controller[_number_control_excavator].ChangeCameraMode();
        Debug.Log(" Change Camera Mode.");
    }

    private void CalculateJointControlCommandByGamepad()
    {
        input_command.Clear();

        ///--- Calculate Joint Control Command
        var delta = Time.deltaTime * 30F;
        ///--- Swing
        float joystickLeftX = Input.GetAxis("JoystickLeftX");
        if (joystickLeftX < -0.3F || 0.3F < joystickLeftX)
            input_command.delta_swing = delta * 1F * joystickLeftX;
        else
            input_command.delta_swing = 0F;

        ///--- Arm
        float joystickRightY = Input.GetAxis("JoystickRightY");
        if (joystickRightY < -0.3F || 0.3F < joystickRightY)
            input_command.delta_arm = delta * 1F * joystickRightY;
        else
            input_command.delta_arm = 0F;

        ///--- Boom
        float joystickLeftY = Input.GetAxis("JoystickLeftY");
        if (joystickLeftY < -0.3F || 0.3F < joystickLeftY)
            input_command.delta_boom = delta * 0.6F * joystickLeftY;
        else
            input_command.delta_boom = 0F;

        ///--- Bucket
        float joystickRightX = Input.GetAxis("JoystickRightX");
        if (joystickRightX < -0.3F || 0.3F < joystickRightX)
            input_command.delta_bucket = delta * 1.5F * joystickRightX;
        else
            input_command.delta_bucket = 0F;

        ///--- Tracks
        // if (input_events.trackRight != 0F || input_events.trackLeft != 0F)
        // {
        //     float rotationRight = -delta * 0.6F * input_events.trackRight;
        //     float rotationLeft = delta * 0.6F * input_events.trackLeft;
        //     float deltaRotation = rotationRight + rotationLeft;
        //     float input = input_events.trackRight + input_events.trackLeft;
        //     if (input > 1F) input = 1F;
        //     else if (input < -1F) input = -1F;
        //     float accel = 0F;
        //     if (input != 0F) accel = driveParams.initialAccel * Mathf.Sign(input) + (driveParams.maxAccel - driveParams.initialAccel) * input;
        //     excavator.Move(deltaRotation, accel, driveParams);
        // }
    }

    private void CalculatePositionControlCommandByGamepad()
    {
        input_command.Clear();
        var delta = Time.deltaTime * 30F;

        ///--- Position Y
        float joystickRightY = Input.GetAxis("JoystickRightY");
        if (joystickRightY < -0.3F || 0.3F < joystickRightY)
            input_command.delta_pos_x = -0.05F * joystickRightY;
        else
            input_command.delta_pos_x = 0F;

        ///--- Position Y
        float joystickLeftY = Input.GetAxis("JoystickLeftY");
        if (joystickLeftY < -0.3F || 0.3F < joystickLeftY)
            input_command.delta_pos_y = 0.05F * joystickLeftY;
        else
            input_command.delta_pos_y = 0F;

        ///--- Swing
        float joystickLeftX = Input.GetAxis("JoystickLeftX");
        if (joystickLeftX < -0.3F || 0.3F < joystickLeftX)
            input_command.delta_swing = delta * 1F * joystickLeftX;
        else
            input_command.delta_swing = 0F;

        ///--- Bucket
        float joystickRightX = Input.GetAxis("JoystickRightX");
        if (joystickRightX < -0.3F || 0.3F < joystickRightX)
            input_command.delta_bucket = delta * 1.5F * joystickRightX;
        else
            input_command.delta_bucket = 0F;

        ///--- Tracks
        // else 
        // {
        //     float joystickLeftY = Input.GetAxis("JoystickLeftY");
        //     if (joystickLeftY != 0)
        //     {
        //         input_events.trackLeft = -joystickLeftY;
        //     }

        //     float joystickRightY = Input.GetAxis("JoystickRightY");
        //     if (joystickRightY != 0)
        //     {
        //         input_events.trackRight = -joystickRightY;
        //     }
        // }
    }
}
