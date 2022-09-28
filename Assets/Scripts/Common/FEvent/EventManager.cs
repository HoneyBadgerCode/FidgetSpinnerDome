using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    /// <summary>
    /// 显示物体旋转速度 一分钟转多少圈
    /// </summary>
    public static FEvent<int> OnShowSpinnerSpeed = new FEvent<int>();
    
    /// <summary>
    /// 已经转了多少圈
    /// </summary>
    public static FEvent<int> OnShowSpinnerSpins = new FEvent<int>(); 
    
    /// <summary>
    /// 更改速度
    /// </summary>
    public static FEvent<string> OnChangeSlideOffset = new FEvent<string>(); 
    
} 