using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Text;     // for OutPutText
using System.IO;       // for OutPutText

using Const;


public class ExcavatorController : MonoBehaviour
{
    public float maxSpeed = 3F;
    public float creepSpeed = 1F;
    public float initialAccel = 1F;
    public float deltaAccel = 0.5F;
    public float maxAccel = 1F;
    public float deltaDirection = 1F;
    public int machine_number = 1;
    
    public bool useHook = false;
    public bool enableRearCameras = true;

    private DriveParams driveParams;

    private Excavator excavator;
    // bool travel = false;
    private int _control_mode = 0; //{ 0:Joint Control by Joy, 1:Position Control by Joy }

    ///---
    private int _number = 0;
    private ExcavatorState _excavator_state = new ExcavatorState();

    private InputCommand _input_command         = new InputCommand();
    private InputCommand _play_command          = new InputCommand();
    private InputCommand _sum_intervene_command = new InputCommand();

    private List<ExcavatorState> _list_play_data          = new List<ExcavatorState>();
    private List<ExcavatorState> _list_modified_play_data = new List<ExcavatorState>();

    private int _operation_mode = 0;
    private bool _flag_play_init  = false;
    private bool _flag_play_init_position  = false;
    private bool _flag_load_data  = false;
    private int _max_play_data = 0;
    private int _lap_play_data = 0;
    private int _count_play_data = 0;
    private int _state_tp = 0; ///--- { 0:stop, 1:teach or play, 2:pause }
    private int[] _play_data_point = { 4, 718, 1149, 1712 };
    private float _timer_sleep_max = 0.1F;
    private float _timer_laps  = 0F;
    private float _timer_sleep = 0F;
    private bool _flag_intervene = false;

    ///---
    private bool _camera_mode_operator = false;
    
    int scoreAmount = 1;
    

//////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////

    //---- Start is called before the first frame update
    void Start()
    {

        //Debug.Log( " Start Excavator Name "+transform.root.gameObject.name );
        string excavator_name = transform.root.gameObject.name;
        excavator_name        = excavator_name.Replace("Excavator","");
        _number = int.Parse( excavator_name ) - 1;
        excavator = new Excavator(transform.root.gameObject);


        float mass = gameObject.GetComponent<Rigidbody>().mass;
        driveParams = new DriveParams(mass, maxSpeed, creepSpeed, initialAccel, deltaAccel, maxAccel, deltaDirection);

        excavator.InitializeCamera( _number );
        excavator.ChangeCameraMode( _camera_mode_operator );

        excavator.useHook = useHook;

        excavator.swingRotate( 0F );
        excavator.boomRotate( 35F );
        excavator.armRotate( -50F );

        _input_command.Clear();
    }

    void Update()
    {
        ///---
        /// 
        UpdateExcavatorState();
        Debug.Log(
            $"機体{machine_number}\n" +
            $"move = {stopper.movemode[(int)machine_number]}  " +
            $"blue = {stopper.bluemode[(int)machine_number]} " +
            $"box = {stopper.boxmode[(int)machine_number]} " +
            $"relocation = {stopper.relocationmode[(int)machine_number]}\n"
        );

        Debug.Log("boxManager.hidenumber: " + FindObjectOfType<BoxManager>().hidenumber + ", relocationnumber: " + FindObjectOfType<BoxManager>().relocationnumber);
        ///--- Normal Gamepad Control Mode
        if (_operation_mode == 0)
        {
            //Debug.Log($"delta_swing: {_input_command.delta_swing}, delta_arm: {_input_command.delta_arm}, delta_boom: {_input_command.delta_boom}, delta_bucket: {_input_command.delta_bucket}");
            ///---
            //Debug.Log("操作しようぜ");
            if (_control_mode == 0)
                JointControl();

            else if (_control_mode == 1)
                PositionControl();
        }

        ///--- Teach Mode
        else if (_operation_mode == 1)
        {
            ///--- Teeach Mode Only Can Use at Position Control Mode
            if (_control_mode == 1)
                PositionControl();
        }

        ///--- Play Mode
        else if (_operation_mode == 2)
        {
            if (!_flag_load_data)
                LoadPlayData();

            if (_flag_play_init)
                GoToInitialPosition();

            if (_flag_play_init_position)
            {
                if (_state_tp == 1)
                    PlayControl();
            }
            // //Debug.Log( " _count_play_data:= "+_count_play_data );
        }

        ///--- Play and Intervene Mode
        else if (_operation_mode == 3 || _operation_mode == 4 || _operation_mode == 5)
        {
            if (!_flag_load_data)
                LoadPlayData();


            if (_flag_play_init && _flag_play_init_position == false)
                GoToInitialPosition();


            if (_flag_play_init_position)
            {
                if (_state_tp == 1)
                {
                    // PlayInterveneControl();
                    PlayInterveneControlModifiedTrajectory();
                    //Debug.Log("position control");
                    //Debug.Log($"stopper.bluemode: {stopper.bluemode}");
                    //Debug.Log($"delta_pos_x: {_input_command.delta_pos_x}, delta_pos_y: {_input_command.delta_pos_y}");
                    //JointControl();
                }
                else if (_excavator_state.state_name == "Waiting")
                {
                    //Debug.Log("_excavator_state.state_name == Waiting");
                    _timer_sleep += Time.deltaTime;

                    if (_flag_intervene || _timer_sleep > _timer_sleep_max)
                    {
                        _state_tp = 1;
                        _count_play_data++;
                        _timer_sleep = 0F;
                    }
                }

                //==================boxmode=================================
                if (stopper.boxmode[(int)machine_number] == 6)
                {
                    if (0 < _count_play_data && _count_play_data < _play_data_point[0])
                    {
                        
                        int count = ScoreManager.Instance.GetMainScore(machine_number);
                        BoxManager boxManager = FindObjectOfType<BoxManager>();
                        bool isHide = (count + boxManager.hidenumber) % boxManager.hidenumber == 0;
                        bool isRelocation = (count + boxManager.relocationnumber) % boxManager.relocationnumber == 0;

                        if (count > 0 && isHide && isRelocation)
                        {
                            PausePlayData();
                            Debug.Log("機体" + machine_number + "：公倍数");
                            if (boxManager != null)
                            {
                                StartCoroutine(boxManager.MoveExcavator(machine_number));
                                stopper.boxmode[(int)machine_number] = 3;
                            }
                        }
                        else if (count > 0 && isHide)
                        {
                            PausePlayData();
                            Debug.Log("機体" + machine_number + "：非表示");
                            if (boxManager != null)
                            {
                                StartCoroutine(boxManager.HideDump(machine_number));
                                stopper.boxmode[(int)machine_number] = 3;
                            }
                        }
                        else if (count > 0 && isRelocation)
                        {
                            PausePlayData();
                            Debug.Log("機体" + machine_number + "：移動");
                            if (boxManager != null)
                            {
                                StartCoroutine(boxManager.MoveExcavator(machine_number));
                                stopper.boxmode[(int)machine_number] = 3;
                            }
                        }
                        else
                        {
                            Debug.Log("機体" + machine_number + "：スルー");
                            stopper.boxmode[(int)machine_number] = 0;
                        }
                    }
                }
                else if (stopper.boxmode[(int)machine_number] == 3)
                {
                    //PausePlayData();
                }
                else if (stopper.boxmode[(int)machine_number] == 4)
                {
                    StartTeachPlay();
                    FindObjectOfType<TotalExcavatorController>().EndIntervention(machine_number);
                    stopper.boxmode[(int)machine_number] = 0;
                }


                //==================movemode================================
                if (stopper.movemode[(int)machine_number] == 1)
                {
                    //Debug.Log($"Excavator({(int)machine_number}) pause");
                    PausePlayData();
                }
                else if (stopper.movemode[(int)machine_number] == 2)
                {
                    //Debug.Log("movemodeが0");
                    StartTeachPlay();
                    stopper.movemode[(int)machine_number] = 0;
                    //Debug.Log($"Excavator({(int)machine_number}) restart");
                }
                else if (stopper.movemode[(int)machine_number] == 10)
                {
                    //Debug.Log("movemodeが10");
                    PlayInterveneControlModifiedTrajectory();
                    _state_tp = 0;
                }

                //==================manmode================================
                if (stopper.manmode[(int)machine_number] == 1)
                {
                    PausePlayData();
                }
                else if (stopper.manmode[(int)machine_number] == 2)
                {
                    //Debug.Log("manmodeが0");
                    StartTeachPlay();
                    stopper.manmode [(int)machine_number] = 0;
                    //Debug.Log($"Excavator({(int)machine_number}) restart");
                }
                else if (stopper.manmode[(int)machine_number] == 10)
                {
                    //Debug.Log("manmodeが10");
                    PlayInterveneControlModifiedTrajectory();
                    _state_tp = 0;
                }

                //==================bluemode================================
                if (stopper.bluemode[(int)machine_number] == 2)
                {
                    PositionControl();
                    _state_tp = 0;
                }
                else if (stopper.bluemode[(int)machine_number] == 3)
                {
                    GoTomiddlePosition();
                }
                else if (stopper.bluemode[(int)machine_number] == 5)
                {
                    StartTeachPlay();
                    stopper.movemode[(int)machine_number] = 0;
                    stopper.bluemode[(int)machine_number] = 0;
                    //Debug.Log($"Excavator({(int)machine_number}) restart");
                }


                //========================================================
                _flag_intervene = false;

                // //Debug.Log( " "+transform.root.gameObject.name+" count:="+_count_play_data+" state:="+_excavator_state.state_name );
            }
            //Debug.Log( " _count_play_data:= "+_count_play_data );
        }
        //Debug.Log("movemode = " + stopper.movemode[(int)machine_number] );
        //Debug.Log("operation mode ="+ _operation_mode);
    }

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
    private void UpdateExcavatorState()
    {
        _excavator_state.swing  = excavator.swingAngle;
        _excavator_state.arm    = excavator.armAngle;
        _excavator_state.boom   = excavator.boomAngle;
        _excavator_state.bucket = excavator.bucketAngle;

        Vector3 position_effector = excavator.Kinematics();
        _excavator_state.pos_x = position_effector.x;
        _excavator_state.pos_y = position_effector.y;
        _excavator_state.pos_z = position_effector.z;

        if( _operation_mode == 2 || _operation_mode == 3 || _operation_mode == 4 || _operation_mode == 5)
        {
            if( _count_play_data < _play_data_point[0] )
                _excavator_state.state_name = "Turning1";
            else if( _count_play_data == _play_data_point[0] )
                _excavator_state.state_name = "Waiting";
            else if( _count_play_data < _play_data_point[1] )
                _excavator_state.state_name = "Drilling";
            else if( _count_play_data < _play_data_point[2] )
                _excavator_state.state_name = "Turning2";
            else if( _count_play_data < _play_data_point[3] )
                _excavator_state.state_name = "Releasing";
        }
    }

    private void JointControl()
    {
        ///--- Move Swing
        if (_input_command.delta_swing != 0F)
            excavator.swingRotate( _input_command.delta_swing );
        ///--- Move Arm
        if (_input_command.delta_arm != 0F)
            excavator.armRotate( _input_command.delta_arm );
        ///--- Move Boom
        if (_input_command.delta_boom != 0F)
            excavator.boomRotate( 0.6F * _input_command.delta_boom );
        ///--- Move Bucket
        if (_input_command.delta_bucket != 0F)
            excavator.bucketRotate( _input_command.delta_bucket );

        ///--- Tracks
        // if (inputEvents.trackRight != 0F || inputEvents.trackLeft != 0F)
        // {
        //     float rotationRight = -delta * 0.6F * inputEvents.trackRight;
        //     float rotationLeft = delta * 0.6F * inputEvents.trackLeft;
        //     float deltaRotation = rotationRight + rotationLeft;
        //     float input = inputEvents.trackRight + inputEvents.trackLeft;
        //     if (input > 1F) input = 1F;
        //     else if (input < -1F) input = -1F;
        //     float accel = 0F;
        //     if (input != 0F) accel = driveParams.initialAccel * Mathf.Sign(input) + (driveParams.maxAccel - driveParams.initialAccel) * input;
        //     excavator.Move(deltaRotation, accel, driveParams);
        // }
    }

    private void PositionControl()
    {
                //Debug.Log("PositionControl");
        ///--- Move Arm and Boom
        StartCoroutine( excavator.PositionControlArm( _input_command.delta_pos_x, _input_command.delta_pos_y ) );
        ///--- Move Swing
        if (_input_command.delta_swing != 0F)
            excavator.swingRotate( _input_command.delta_swing );
        ///--- Move Bucket
        if (_input_command.delta_bucket != 0F)
            excavator.bucketRotate( _input_command.delta_bucket );

        ///--- Tracks
        // if (inputEvents.trackRight != 0F || inputEvents.trackLeft != 0F)
        // {
        //     float rotationRight = -delta * 0.6F * inputEvents.trackRight;
        //     float rotationLeft = delta * 0.6F * inputEvents.trackLeft;
        //     float deltaRotation = rotationRight + rotationLeft;
        //     float input = inputEvents.trackRight + inputEvents.trackLeft;
        //     if (input > 1F) input = 1F;
        //     else if (input < -1F) input = -1F;
        //     float accel = 0F;
        //     if (input != 0F) accel = driveParams.initialAccel * Mathf.Sign(input) + (driveParams.maxAccel - driveParams.initialAccel) * input;
        //     excavator.Move(deltaRotation, accel, driveParams);
        // }
    }

    private void PlayControl()
    {
        ///--- Update Excavator State
        GetExcavatorState();

        ///--- Calculate Delta Value
        _play_command.delta_pos_x = _list_play_data[_count_play_data].pos_x - _excavator_state.pos_x;
        _play_command.delta_pos_y = _list_play_data[_count_play_data].pos_y - _excavator_state.pos_y;
        _play_command.delta_pos_z = _list_play_data[_count_play_data].pos_z - _excavator_state.pos_z;

        _play_command.delta_swing  = _list_play_data[_count_play_data].swing - _excavator_state.swing;
        _play_command.delta_arm    = _list_play_data[_count_play_data].arm - _excavator_state.arm;
        _play_command.delta_boom   = _list_play_data[_count_play_data].boom - _excavator_state.boom;
        _play_command.delta_bucket = _list_play_data[_count_play_data].bucket - _excavator_state.bucket;
        _play_command.delta_trackRight = _list_play_data[_count_play_data].trackRight - _excavator_state.trackRight;
        _play_command.delta_trackLeft  = _list_play_data[_count_play_data].trackLeft - _excavator_state.trackLeft;

        // //Debug.Log (" _list_play_data[_count_play_data].pos_x:="+_list_play_data[_count_play_data].pos_x+" _excavator_state.pos_x:="+_excavator_state.pos_x );
        // //Debug.Log (" _list_play_data[_count_play_data].swing:="+_list_play_data[_count_play_data].swing+" _excavator_state.swing:="+_excavator_state.swing );
        // //Debug.Log (" _list_play_data[_count_play_data].bucket:="+_list_play_data[_count_play_data].bucket+" _excavator_state.bucket:="+_excavator_state.bucket );

        int temp = CheckReachPlayDataPosition( _play_command );
        if( temp == 4 )
            _count_play_data++;

        else if( _count_play_data == _play_data_point[0] )
        {
            _timer_sleep += Time.deltaTime;


            if( _timer_sleep < _timer_sleep_max )
                _play_command.Clear();
            else
            {
                _timer_sleep = 0F;
                _count_play_data++;
            }
        }

        else
            _count_play_data++;

        ///--- Move Arm and Boom
        StartCoroutine( excavator.PositionControlArm( _play_command.delta_pos_x, _play_command.delta_pos_y ) );
        ///--- Move Swing
        if ( Mathf.Abs(_play_command.delta_swing) > 0.001F )
            excavator.swingRotate( _play_command.delta_swing );
        ///--- Move Bucket
        if ( Mathf.Abs(_play_command.delta_bucket) > 0.001F)
            excavator.bucketRotate( _play_command.delta_bucket );

        ///--- Tracks
        // if (inputEvents.trackRight != 0F || inputEvents.trackLeft != 0F)
        // {
        //     float rotationRight = -delta * 0.6F * inputEvents.trackRight;
        //     float rotationLeft = delta * 0.6F * inputEvents.trackLeft;
        //     float deltaRotation = rotationRight + rotationLeft;
        //     float input = inputEvents.trackRight + inputEvents.trackLeft;
        //     if (input > 1F) input = 1F;
        //     else if (input < -1F) input = -1F;
        //     float accel = 0F;
        //     if (input != 0F) accel = driveParams.initialAccel * Mathf.Sign(input) + (driveParams.maxAccel - driveParams.initialAccel) * input;
        //     excavator.Move(deltaRotation, accel, driveParams);
        // }

        ///--- Update Lap Time
        _timer_laps += Time.deltaTime;

        ///--- When the Playing Data Reaches the End, Return to the Beginning
        if( _count_play_data >= _max_play_data )
        {
            //Debug.Log( " "+transform.root.gameObject.name+" lap:="+(_lap_play_data+1)+" time:="+_timer_laps );
            _timer_laps = 0F;
            _lap_play_data++;
            _count_play_data = 0;//_play_data_point[0];
        }
    }

    private void PlayInterveneControl()
    {
        ///--- Update Excavator State
        GetExcavatorState();

        ///--- Calculate Delta Value
        _play_command.delta_pos_x = _list_play_data[_count_play_data].pos_x - _excavator_state.pos_x;
        _play_command.delta_pos_y = _list_play_data[_count_play_data].pos_y - _excavator_state.pos_y;
        _play_command.delta_swing  = _list_play_data[_count_play_data].swing - _excavator_state.swing;
        _play_command.delta_bucket = _list_play_data[_count_play_data].bucket - _excavator_state.bucket;

        // //Debug.Log (" _list_play_data[_count_play_data].pos_x:="+_list_play_data[_count_play_data].pos_x+" _excavator_state.pos_x:="+_excavator_state.pos_x );
        // //Debug.Log ("_list_play_data[_count_play_data].swing:="+_list_play_data[_count_play_data].swing+" _excavator_state.swing:="+_excavator_state.swing );
        // //Debug.Log ("_list_play_data[_count_play_data].bucket:="+_list_play_data[_count_play_data].bucket+" _excavator_state.bucket:="+_excavator_state.bucket );

        if( _count_play_data == _play_data_point[0] )
        {
            PausePlayData();
            _play_command.Clear();
        }

        else if( _play_data_point[0] < _count_play_data && _count_play_data < _play_data_point[1] )
        {
            ///--- Caluculate Intervene Input Value
            _sum_intervene_command.delta_pos_x  += 0.2F*_input_command.delta_pos_x;
            _sum_intervene_command.delta_pos_y  += 0.2F*_input_command.delta_pos_y;
            _sum_intervene_command.delta_swing  += 0.2F*_input_command.delta_swing;
            _sum_intervene_command.delta_bucket += 0.4F*_input_command.delta_bucket;
            if( _sum_intervene_command.delta_pos_x > 0.5F )
                _sum_intervene_command.delta_pos_x = 0.5F;
            else if( _sum_intervene_command.delta_pos_x < -0.5F )
                _sum_intervene_command.delta_pos_x = -0.5F;
            if( _sum_intervene_command.delta_pos_y > 0.5F )
                _sum_intervene_command.delta_pos_y = 0.5F;
            else if( _sum_intervene_command.delta_pos_y < -0.5F )
                _sum_intervene_command.delta_pos_y = -0.5F;

            _play_command.delta_pos_x  += _sum_intervene_command.delta_pos_x;
            _play_command.delta_pos_y  += _sum_intervene_command.delta_pos_y;
            _play_command.delta_swing  += _sum_intervene_command.delta_swing;
            _play_command.delta_bucket += _sum_intervene_command.delta_bucket;

            // //Debug.Log ( " _play_command.delta_pos_x:="+_play_command.delta_pos_x+" _sum_intervene_command.delta_pos_x:="+_sum_intervene_command.delta_pos_x );
            // //Debug.Log ( " _play_command.delta_pos_y:="+_play_command.delta_pos_y+" _sum_intervene_command.delta_pos_y:="+_sum_intervene_command.delta_pos_y );
            // //Debug.Log ( " _play_command.delta_swing:="+_play_command.delta_swing+" _sum_intervene_command.delta_swing:="+_sum_intervene_command.delta_swing );
            // //Debug.Log ( " bucket _play_command:="+_play_command.delta_bucket+" _sum_intervene_command:="+_sum_intervene_command.delta_bucket+" now:="+_excavator_state.bucket );

            _count_play_data++;
        }

        else
        {
            int temp = CheckReachPlayDataPosition( _play_command );
            if( temp == 4 )
                _count_play_data++;
        }

        ///--- Move Arm and Boom
        StartCoroutine( excavator.PositionControlArm( _play_command.delta_pos_x, _play_command.delta_pos_y ) );
        ///--- Move Swing
        if ( Mathf.Abs(_play_command.delta_swing) > 0.001F )
            excavator.swingRotate( _play_command.delta_swing );
        ///--- Move Bucket
        if ( Mathf.Abs(_play_command.delta_bucket) > 0.001F)
            excavator.bucketRotate( _play_command.delta_bucket );

        ///--- Update Lap Time
        _timer_laps += Time.deltaTime;

        ///--- When the Playing Data Reaches the End, Return to the Beginning
        if( _count_play_data >= _max_play_data )
        {
            //Debug.Log( " "+transform.root.gameObject.name+" lap:="+(_lap_play_data+1)+" time:="+_timer_laps );
            _timer_laps = 0F;
            _lap_play_data++;
            _count_play_data = 0;
            
            _sum_intervene_command.Clear();
        }
    }


    private void PlayInterveneControlModifiedTrajectory()
    {
        ///--- Update Excavator State
        GetExcavatorState();

        //Debug.Log("movemode = " + stopper.movemode[(int)machine_number]);
        //Debug.Log("bluemode = " + stopper.bluemode[(int)machine_number]);

        ///--- Calculate Delta Value
        _play_command.delta_pos_x = _list_modified_play_data[_count_play_data].pos_x - _excavator_state.pos_x;
        _play_command.delta_pos_y = _list_modified_play_data[_count_play_data].pos_y - _excavator_state.pos_y;
        _play_command.delta_swing  = _list_modified_play_data[_count_play_data].swing - _excavator_state.swing;
        _play_command.delta_bucket = _list_modified_play_data[_count_play_data].bucket - _excavator_state.bucket;

        if (stopper.movemode[(int)machine_number] == 1)
        {

        }
        else if (stopper.movemode[(int)machine_number] == 2)
        {

        }
        else if (stopper.movemode[(int)machine_number] == 10)
        {
            
        }
        else
        {
            if (_count_play_data == 0)
            {
                //Debug.Log("startteachplay");
                ///---
                int temp = CheckReachPlayDataPosition(_play_command);
                if (temp == 4)
                    _count_play_data++;
                //Debug.Log("STEP0");
                // //Debug.Log( " X error:= "+Mathf.Abs(_play_command.delta_pos_x)+" target:= "+_list_modified_play_data[_count_play_data].pos_x+" current:= "+_excavator_state.pos_x );
                // //Debug.Log( " Y error:= "+Mathf.Abs(_play_command.delta_pos_y)+" target:= "+_list_modified_play_data[_count_play_data].pos_y+" current:= "+_excavator_state.pos_y );
                // //Debug.Log( " Mathf.Abs(_play_command.delta_pos_x):= "+Mathf.Abs(_play_command.delta_pos_x)+" Mathf.Abs(_play_command.delta_pos_y):= "+Mathf.Abs(_play_command.delta_pos_y) );
                // //Debug.Log( " _play_command.delta_swing:= "+_play_command.delta_swing+" _play_command.delta_bucket:= "+_play_command.delta_bucket );
            }

            ///--- STEP 1: Turning 1
            else if (_count_play_data < _play_data_point[0])
            {
                // //Debug.Log( " laps:= "+_lap_play_data+" state:= "+_excavator_state.state_name );

                int temp = CheckReachPlayDataPosition(_play_command);
                if (temp == 4)
                    _count_play_data++;
                //Debug.Log("STEP1");
            }

            ///--- STEP 2: Waiting Intervene Action
            else if (_count_play_data == _play_data_point[0])
            {
                PausePlayData();
                _play_command.Clear();
                //Debug.Log("STEP2");
            }

            ///--- STEP 3: Drilling
            else if (_count_play_data < _play_data_point[1])
            {
                // //Debug.Log( " laps:= "+_lap_play_data+" state:= "+_excavator_state.state_name );

                ///--- Caluculate Intervene Input Value
                _sum_intervene_command.delta_pos_x += 0.2F * _input_command.delta_pos_x;
                _sum_intervene_command.delta_pos_y += 0.2F * _input_command.delta_pos_y;
                _sum_intervene_command.delta_swing += 0.2F * _input_command.delta_swing;
                _sum_intervene_command.delta_bucket += 0.4F * _input_command.delta_bucket;
                if (_sum_intervene_command.delta_pos_x > 0.5F)
                    _sum_intervene_command.delta_pos_x = 0.5F;
                else if (_sum_intervene_command.delta_pos_x < -0.5F)
                    _sum_intervene_command.delta_pos_x = -0.5F;
                if (_sum_intervene_command.delta_pos_y > 0.5F)
                    _sum_intervene_command.delta_pos_y = 0.5F;
                else if (_sum_intervene_command.delta_pos_y < -0.5F)
                    _sum_intervene_command.delta_pos_y = -0.5F;

                _play_command.delta_pos_x += _sum_intervene_command.delta_pos_x;
                _play_command.delta_pos_y += _sum_intervene_command.delta_pos_y;
                _play_command.delta_swing += _sum_intervene_command.delta_swing;
                _play_command.delta_bucket += _sum_intervene_command.delta_bucket;

                // //Debug.Log ( " _play_command.delta_pos_x:="+_play_command.delta_pos_x+" _sum_intervene_command.delta_pos_x:="+_sum_intervene_command.delta_pos_x );
                // //Debug.Log ( " _play_command.delta_pos_y:="+_play_command.delta_pos_y+" _sum_intervene_command.delta_pos_y:="+_sum_intervene_command.delta_pos_y );
                // //Debug.Log ( " _play_command.delta_swing:="+_play_command.delta_swing+" _sum_intervene_command.delta_swing:="+_sum_intervene_command.delta_swing );
                // //Debug.Log ( " bucket _play_command:="+_play_command.delta_bucket+" _sum_intervene_command:="+_sum_intervene_command.delta_bucket+" now:="+_excavator_state.bucket );

                _count_play_data++;
                //Debug.Log("STEP3");
            }

            ///--- STEP 4: Turning 2
            else if (_play_data_point[1] <= _count_play_data && _count_play_data < _play_data_point[2])
            {
                if (stopper.boxmode[(int)machine_number] == 1)
                {
                    stopper.boxmode[(int)machine_number] = 2;
                }
                else
                {
                    ///--- Adjust Swing Angle to Fit the intervention
                    if (Mathf.Sign(_list_play_data[_play_data_point[2]].swing - _excavator_state.swing) != Mathf.Sign(_play_command.delta_swing))
                        _play_command.delta_swing = 0F;
                    // //Debug.Log( " sign last:="+Mathf.Sign( _list_play_data[_play_data_point[2]].swing - _excavator_state.swing )+" now:="+Mathf.Sign( _play_command.delta_swing ));

                    ///---
                    int temp = CheckReachPlayDataPosition(_play_command);
                    if (temp == 4)
                        _count_play_data++;
                    //Debug.Log("STEP4");
                }
            }

            ///--- STEP 5: Releasing
            else if (_count_play_data < _play_data_point[3])
            {
                // //Debug.Log( " laps:= "+_lap_play_data+" state:= "+_excavator_state.state_name );

                ///---
                int temp = CheckReachPlayDataPosition(_play_command);
                if (temp == 4)
                {
                    _count_play_data++;
                    //Debug.Log(temp);
                }
                //Debug.Log("STEP5");

            }

            ///--- Move Arm and Boom
            StartCoroutine(excavator.PositionControlArm(_play_command.delta_pos_x, _play_command.delta_pos_y));
            ///--- Move Swing
            if (Mathf.Abs(_play_command.delta_swing) > 0.001F)
                excavator.swingRotate(_play_command.delta_swing);
            ///--- Move Bucket
            if (Mathf.Abs(_play_command.delta_bucket) > 0.001F)
                excavator.bucketRotate(_play_command.delta_bucket);

            ///--- Save Modified Play Data
            if (_excavator_state.state_name == "Drilling")
            {
                _list_modified_play_data[_count_play_data - 1].pos_x = _excavator_state.pos_x;
                _list_modified_play_data[_count_play_data - 1].pos_y = _excavator_state.pos_y;
                _list_modified_play_data[_count_play_data - 1].swing = _excavator_state.swing;
                // _list_modified_play_data[ _count_play_data-1 ].bucket = _excavator_state.bucket;
            }

            ///--- Update Lap Time
            _timer_laps += Time.deltaTime;
            //Debug.Log(_count_play_data);
            ///--- When the Playing Data Reaches the End, Return to the Beginning
            if (_count_play_data >= _play_data_point[3])
            {
                //Debug.Log( " "+transform.root.gameObject.name+" lap:="+(_lap_play_data+1)+" time:="+_timer_laps );
                _timer_laps = 0F;
                _lap_play_data++;
                _count_play_data = 0;

                _sum_intervene_command.Clear();
                ModifiedPlayDataTrajectory();
                //Debug.Log("STEP6");
            }
        }
    }

    private void LoadPlayData()
    {
        ///--- Load Play Excavator Data
        string file_path = @"Assets/Excavator/TrajectoryData/"+transform.root.gameObject.name+"_trajectory_data.csv";
        StreamReader sr = new StreamReader( file_path );
        var row_data = sr.ReadLine(); //一行読み込み

        int count_row = 0;
        while (row_data != null)
        {
            //③カンマで区切り、文字配列とする
            var columns_data = row_data.Split(',');

            ExcavatorState _excavator_play_data = new ExcavatorState();
            ExcavatorState _excavator_play_data_modifiled = new ExcavatorState();
            if( count_row != 0 )
            {
                _excavator_play_data.pos_x = float.Parse(columns_data[0]);
                _excavator_play_data.pos_y = float.Parse(columns_data[1]);
                _excavator_play_data.pos_z = float.Parse(columns_data[2]);

                _excavator_play_data.swing  = float.Parse(columns_data[3]);
                _excavator_play_data.arm    = float.Parse(columns_data[4]);
                _excavator_play_data.boom   = float.Parse(columns_data[5]);
                _excavator_play_data.bucket = float.Parse(columns_data[6]);
                _excavator_play_data.trackRight = float.Parse(columns_data[7]);
                _excavator_play_data.trackLeft  = float.Parse(columns_data[8]);

                _excavator_play_data_modifiled.pos_x = float.Parse(columns_data[0]);
                _excavator_play_data_modifiled.pos_y = float.Parse(columns_data[1]);
                _excavator_play_data_modifiled.pos_z = float.Parse(columns_data[2]);

                _excavator_play_data_modifiled.swing  = float.Parse(columns_data[3]);
                _excavator_play_data_modifiled.arm    = float.Parse(columns_data[4]);
                _excavator_play_data_modifiled.boom   = float.Parse(columns_data[5]);
                _excavator_play_data_modifiled.bucket = float.Parse(columns_data[6]);
                _excavator_play_data_modifiled.trackRight = float.Parse(columns_data[7]);
                _excavator_play_data_modifiled.trackLeft  = float.Parse(columns_data[8]);

                _list_play_data.Add( _excavator_play_data );
                _list_modified_play_data.Add( _excavator_play_data_modifiled );
            }
            
            row_data = sr.ReadLine(); //一行読み込み
            count_row++;
        }

        _max_play_data = count_row - 1;

        _flag_load_data = true;
        //Debug.Log( " "+transform.root.gameObject.name+" Complete load Excavator play data." );
    }

    private void ModifiedPlayDataTrajectory()
    {
        ///--- Turning 1
        for( int i=1; i<_play_data_point[0]; i++ )
        {
            _list_modified_play_data[i].swing = i * ( _list_modified_play_data[_play_data_point[1]-1].swing - _list_modified_play_data[0].swing ) / ( _play_data_point[0] - 1 ) + _list_modified_play_data[0].swing;
        }

        ///--- Drilling
        for( int i=_play_data_point[0]; i<_play_data_point[1]; i++ )
        {
            _list_modified_play_data[i].swing = _list_modified_play_data[_play_data_point[1]-1].swing;
        }

        ///--- Turning 2
        for( int i=_play_data_point[1]; i<_play_data_point[2]; i++ )
        {
            _list_modified_play_data[i].pos_x  = ( i - _play_data_point[1] )  * ( _list_modified_play_data[_play_data_point[2]].pos_x - _list_modified_play_data[_play_data_point[1]-1].pos_x ) / ( _play_data_point[2] - _play_data_point[1] - 1 ) + _list_modified_play_data[_play_data_point[1]-1].pos_x;
            _list_modified_play_data[i].pos_y  = ( i - _play_data_point[1] )  * ( _list_modified_play_data[_play_data_point[2]].pos_y - _list_modified_play_data[_play_data_point[1]-1].pos_y ) / ( _play_data_point[2] - _play_data_point[1] - 1 ) + _list_modified_play_data[_play_data_point[1]-1].pos_y;
            _list_modified_play_data[i].swing  = ( i - _play_data_point[1] )  * ( _list_modified_play_data[_play_data_point[2]].swing - _list_modified_play_data[_play_data_point[1]-1].swing ) / ( _play_data_point[2] - _play_data_point[1] - 1 ) + _list_modified_play_data[_play_data_point[1]-1].swing;
            _list_modified_play_data[i].bucket = ( i - _play_data_point[1] )  * ( _list_modified_play_data[_play_data_point[2]].bucket - _list_modified_play_data[_play_data_point[1]-1].bucket ) / ( _play_data_point[2] - _play_data_point[1] - 1 ) + _list_modified_play_data[_play_data_point[1]-1].bucket;
        }

    }

    private int CheckReachPlayDataPosition( InputCommand input_command  )
    {
        int number_reach = 0;
        if( Mathf.Abs( input_command.delta_pos_x) < 0.015F )
            number_reach++;
        if( Mathf.Abs( input_command.delta_pos_y) < 0.015F )
            number_reach++;
        if( Mathf.Abs( input_command.delta_swing) < 1.0F )
            number_reach++;
        if( Mathf.Abs( input_command.delta_bucket) < 1.0F )
            number_reach++;

        return number_reach;
    }

    private void GoToInitialPosition()
    {
        ///--- Update Excavator State
        GetExcavatorState();

        ///--- Calculate Delta Value
        _play_command.delta_pos_x = _list_play_data[0].pos_x - _excavator_state.pos_x;
        _play_command.delta_pos_y = _list_play_data[0].pos_y - _excavator_state.pos_y;
        _play_command.delta_swing  = _list_play_data[0].swing - _excavator_state.swing;
        _play_command.delta_bucket = _list_play_data[0].bucket - _excavator_state.bucket;

        ///--- Move Arm and Boom
        StartCoroutine( excavator.PositionControlArm( _play_command.delta_pos_x, _play_command.delta_pos_y ) );
        ///--- Move Swing
        if ( Mathf.Abs(_play_command.delta_swing) > 0.001F )
            excavator.swingRotate( _play_command.delta_swing );
        ///--- Move Bucket
        if ( Mathf.Abs(_play_command.delta_bucket) > 0.001F)
            excavator.bucketRotate( _play_command.delta_bucket );


        int temp = CheckReachPlayDataPosition( _play_command );
        if( temp == 4 )
        {
            _flag_play_init = false;
            _flag_play_init_position = true;
            //Debug.Log( " "+transform.root.gameObject.name+" is initial position." );
            stopper.movemode[(int)machine_number] = 2;
            stopper.bluemode[(int)machine_number] = 0;
        }
    }

    private void GoTomiddlePosition()
    {
        ///--- Update Excavator State
        GetExcavatorState();

        ///--- Calculate Delta Value
        _play_command.delta_pos_x = _list_play_data[718].pos_x - _excavator_state.pos_x;
        _play_command.delta_pos_y = _list_play_data[718].pos_y - _excavator_state.pos_y;
        _play_command.delta_swing  = _list_play_data[718].swing - _excavator_state.swing;
        _play_command.delta_bucket = _list_play_data[718].bucket - _excavator_state.bucket;

        _count_play_data = 718;
        ///--- Move Arm and Boom
        StartCoroutine( excavator.PositionControlArm( _play_command.delta_pos_x, _play_command.delta_pos_y ) );
        ///--- Move Swing
        if ( Mathf.Abs(_play_command.delta_swing) > 0.001F )
            excavator.swingRotate( _play_command.delta_swing );
        ///--- Move Bucket
        if ( Mathf.Abs(_play_command.delta_bucket) > 0.001F)
            excavator.bucketRotate( _play_command.delta_bucket );


        int temp = CheckReachPlayDataPosition( _play_command );
        if( temp == 4 )
        {
            _flag_play_init = false;
            _flag_play_init_position = true;
            //Debug.Log( " "+transform.root.gameObject.name+" is middle position." );
            //stopper.movemode[(int)machine_number] = 2;
            stopper.bluemode[(int)machine_number] = 5;
        }
    }

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetControlMode( int input_control_mode )
    {
        _control_mode = input_control_mode;
        // //Debug.Log( " set _control_mode:="+control_mode );

        _state_tp = 0;
        _count_play_data = 0;
        _lap_play_data = 0;
    }

    public int GetControlMode()
    {
        return _control_mode;
    }

    public void SetOperationMode( int input_operation_mode )
    {
        _operation_mode = input_operation_mode;
        // //Debug.Log( " set _operation_mode:="+_operation_mode );
        _state_tp = 0;
        _count_play_data = 0;
        _lap_play_data = 0;

        _flag_play_init = false;
        _flag_play_init_position = false;

        _input_command.Clear();
        _play_command.Clear();
    }

    public int GetOperationMode()
    {
        return _operation_mode;
    }

    public void SetInputCommand( InputCommand input_command )
    {
        _input_command = input_command;
    }

    public void SetCommandZero()
    {
        _state_tp = 0;
        _count_play_data = 0;
        _lap_play_data = 0;

        _input_command.Clear();
        _play_command.Clear();
    }

    public ExcavatorState GetExcavatorState()
    {
        return _excavator_state;
    }

    public void StartTeachPlay()
    {
        //Debug.Log("StartTeachPlay");
        if( _operation_mode == 1 )
            _state_tp = 1;

        else if( _operation_mode == 2 || _operation_mode == 3 || _operation_mode == 4 || _operation_mode == 5)
        {
            if( _flag_play_init_position == true )
            {
                _state_tp = 1;
            }
            else
                Debug.Log( " "+transform.root.gameObject.name+" is not initial position, we cannot start play. ");
        }

    }

    public void StopTeachPlay()
    {
       _state_tp = 0;
    }

    public void PausePlayData()
    {
        Debug.Log( " "+transform.root.gameObject.name+" Pause Play Data." );
       _state_tp = 2;
    }

    public void StopPlayData()
    {
        Debug.Log( " "+transform.root.gameObject.name+" Stop Play Data." );
       _state_tp = 0;
       _count_play_data = 0;
       _lap_play_data = 0;
       _flag_play_init = false;
       _flag_play_init_position = false;
    }

    public void SetInitialPosition()
    {
       _flag_play_init = true;
    }

    public int GetTeachPlayState()
    {
        return _state_tp;
    }

    public int GetCountPlayData()
    {
        return _count_play_data;
    }

    public void SetPlayDataPoint( int number_point, int data_point )
    {
        _play_data_point[number_point] = data_point;
    }

    public void SetTimerSleepMax( float set_timer_sleep_max )
    {
        _timer_sleep_max = set_timer_sleep_max;
    }

    public float GetTimerSleep()
    {
        return _timer_sleep;
    }

    public float GetTimerSleepMax()
    {
        return _timer_sleep_max;
    }

    public void SetFlagIntervene()
    {
       _flag_intervene = true;
    }

    public string GetExcavatorStateName()
    {
        return _excavator_state.state_name;
    }

    public void DebugInfo()
    {
        // Debug.Log( " "+transform.root.gameObject.name+" pos_x:="+_excavator_state.pos_x+" _input_pos_x:="+_input_command.delta_pos_x );
        // Debug.Log( " "+transform.root.gameObject.name+" pos_y:="+_excavator_state.pos_y+" _input_pos_y:="+_input_command.delta_pos_y );
        // Debug.Log( " "+transform.root.gameObject.name+" swing:="+_excavator_state.swing+" _input_swing:="+_input_command.delta_swing );
        // Debug.Log( " "+transform.root.gameObject.name+" bucket:="+_excavator_state.bucket+" _input_bucket:="+_input_command.delta_bucket );
    }
    //////////////////////////////////////////////////////////////////////////////////////
    public void ChangeCameraMode()
    {
        if( _camera_mode_operator )
            _camera_mode_operator = false;
        else
            _camera_mode_operator = true;

        excavator.ChangeCameraMode( _camera_mode_operator );
    }
    
    public void SetCameraOn()
    {
        excavator.SetCameraOn();
    }

    public void SetCameraOff()
    {
        excavator.SetCameraOff();
    }
}
