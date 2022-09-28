using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 转轮物体
///    职能：1.根据输入速度和方向自转  2.自转速度会随着时间时间递减为0进入结算
/// </summary>
public class Spinner_2D : MonoBehaviour
{
    /// <summary>
    /// 鼠标按下位置记录
    /// </summary>
    private Vector3 m_MouPos_Down = Vector3.zero;

    /// <summary>
    /// 鼠标按下之后的时间
    /// </summary>
    private float m_MouseDownTimer = 0.0f;


    /// <summary>
    /// 规定多长时间为拖拽
    /// </summary>
    private float m_DownLatencyTime = 0.35f;

    /// <summary>
    /// 物体旋转速度
    /// </summary>
    private float m_RotationSpeed = 6f;

    /// <summary>
    /// 旋转阻力
    /// </summary>
    public float m_RotationResistance = 0.0f;

    /// <summary>
    /// 滑动偏移量
    /// </summary>
    public float m_SlideOffset = 0.0f;

    /// <summary>
    /// 物体旋转方向
    /// </summary>
    private GlobalEnum.SpinnerRotationDir m_Dir = GlobalEnum.SpinnerRotationDir.Left;

    private Camera m_MainCamera => Camera.main;

    //物体的状态
    private GlobalEnum.SpinnerState m_State = GlobalEnum.SpinnerState.stop;

    public GlobalEnum.SpinnerState State
    {
        get { return m_State; }
        private set { m_State = value; }
    }

    /// <summary>
    /// 帧间隔时间
    /// </summary>
    private float IFS = 0.02f;

    /// <summary>
    /// 用来判断物体旋转过一圈了
    /// </summary>
    private float m_Laps = 0.0f;

    /// <summary>
    /// 记录旋转圈数
    /// </summary>
    private int m_RotatingLaps = 0;

    /// <summary>
    /// 一分钟等于60秒
    /// </summary>
    private int m_Second = 60;

    /// <summary>
    /// 一圈360度
    /// </summary>
    private int m_Angle = 360;

    /// <summary>
    /// 旋转物体
    /// </summary>
    public Transform SpinnerTransform;

    /// <summary>
    /// 鼠标按下 是否可以拖拽
    /// </summary>
    private bool m_IsCanDrag = false;

    private float dis;

    //运动曲线
    public AnimationCurve Curve;
    public float StopTime = 10;
    private float m_StopTimer = 0.0f;

    //触摸屏幕状态
    private Touch m_Touch;

    private void Awake()
    {
        //不允许多点触摸
        Input.multiTouchEnabled = false;
    }

    void Start()
    {
        EventManager.OnChangeSlideOffset.AddEventHandler(OnChangeSlideOffset);
    }

    private void OnDestroy()
    {
        EventManager.OnChangeSlideOffset.RemoveEventHandler(OnChangeSlideOffset);
    }

    private void OnChangeSlideOffset(string obj)
    {
        float.TryParse(obj, out m_SlideOffset);
    }

    private void OnMouseDown()
    {
        m_MouseDownTimer = 0.0f;
        m_StopTimer = 0.0f;
#if UNITY_EDITOR
        m_MouPos_Down = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
#elif UNITY_ANDROID
        m_MouPos_Down = m_MainCamera.ScreenToWorldPoint(m_Touch.position);
#endif
        // if (State != GlobalEnum.SpinnerState.onRotation)
        OnChangeState(GlobalEnum.SpinnerState.stop);
        m_IsCanDrag = true;
        Debug.Log("OnMouseDown");
    }

    private void OnMouseUp()
    {
        if (State == GlobalEnum.SpinnerState.stop)
        {
#if UNITY_EDITOR
            var mousPos_Up = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
#elif UNITY_ANDROID
            var mousPos_Up = m_MainCamera.ScreenToWorldPoint(m_Touch.position);
#endif
            dis = Vector3.Distance(m_MouPos_Down, mousPos_Up);
            m_Dir = m_MouPos_Down.x <= mousPos_Up.x
                ? GlobalEnum.SpinnerRotationDir.Right
                : GlobalEnum.SpinnerRotationDir.Left;
            m_RotationSpeed = (dis / m_MouseDownTimer) / m_SlideOffset;
            m_RotationResistance = m_RotationSpeed;
            StopTime = Mathf.Lerp(3, 15, m_RotationSpeed / 90.0f);
            Debug.Log($"StopTime == {StopTime}; m_RotationSpeed == {m_RotationSpeed}");
            OnChangeState(GlobalEnum.SpinnerState.onRotation);
        }

        m_IsCanDrag = false;
        Debug.Log("OnMouseUp");
    }

    private void OnMouseDrag()
    {
        if (m_MouseDownTimer >= m_DownLatencyTime)
        {
            OnChangeState(GlobalEnum.SpinnerState.onDrag);
            if (m_IsCanDrag)
            {
                //拖拽时 父级为null 不改变上一帧look的位置
                SpinnerTransform.SetParent(null);
                m_IsCanDrag = false;
            }

            Transform transform1;
            (transform1 = transform).rotation =
                Quaternion.LookRotation(m_MainCamera.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
            transform1.eulerAngles = new Vector3(0, 0, -transform1.eulerAngles.z);
            //设置父级 可以跟父级一起look
            if (SpinnerTransform.parent == null || SpinnerTransform.parent != transform1)
                SpinnerTransform.SetParent(transform1);
        }

        Debug.Log("OnMouseDrag");
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }

        if (Input.GetMouseButton(0))
        {
            OnMouseDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
#elif UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            m_Touch = Input.GetTouch(0);
            switch (m_Touch.phase)
            {
                case TouchPhase.Began:
                    OnMouseDown();
                    break;
                case TouchPhase.Moved:
                    OnMouseDrag();
                    break;
                case TouchPhase.Ended:
                    OnMouseUp();
                    break;
            }
        }
#endif

        OnRotationAnim();
        m_MouseDownTimer += Time.deltaTime;
    }

    /// <summary>
    /// 播放选装动画
    /// </summary>
    private void OnRotationAnim()
    {
        if (State == GlobalEnum.SpinnerState.onRotation)
        {
            switch (m_Dir)
            {
                case GlobalEnum.SpinnerRotationDir.Left:
                    SpinnerTransform.eulerAngles += new Vector3(0, 0, -m_RotationSpeed);
                    break;
                case GlobalEnum.SpinnerRotationDir.Right:
                    SpinnerTransform.eulerAngles += new Vector3(0, 0, m_RotationSpeed);
                    break;
            }

            //根据曲线取值 控制速度变慢
            if (m_StopTimer < StopTime)
            {
                var r = 1 - Curve.Evaluate(m_StopTimer / StopTime);
                m_RotationSpeed = Mathf.Lerp(m_RotationResistance, 0, r);
                m_StopTimer += Time.deltaTime;
            }

            //切管状态
            if (m_RotationSpeed <= 0.0f)
            {
                EventManager.OnShowSpinnerSpeed.BroadCastEvent(0);
                OnChangeState(GlobalEnum.SpinnerState.stop);
            }
            else
            {
                //计算每分钟多少圈  公式：60秒 除以  （ 360度 除以 当前帧的角度  乘以 一帧的时间 ）
                int minAngle = (int) (m_Second / (m_Angle / m_RotationSpeed * Time.deltaTime));
                EventManager.OnShowSpinnerSpeed.BroadCastEvent(minAngle);

                //圈数
                m_Laps += m_RotationSpeed / m_Angle;
                if (m_Laps >= 1)
                {
                    m_Laps = 0;
                    m_RotatingLaps++;
                    EventManager.OnShowSpinnerSpins.BroadCastEvent(m_RotatingLaps);
                }
            }
        }
    }


    /// <summary>
    /// 更改状态
    /// </summary>
    /// <param name="_state"></param>
    public void OnChangeState(GlobalEnum.SpinnerState _state)
    {
        State = _state;
    }
}