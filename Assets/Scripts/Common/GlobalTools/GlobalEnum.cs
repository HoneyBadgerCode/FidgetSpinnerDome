/// <summary>
/// 项目所有枚举
/// </summary>
public class GlobalEnum
{
    /// <summary>
    /// 旋转物体的状态
    /// </summary>
    public enum SpinnerState
    {
        stop,//静止 
        onDrag,//被拖拽
        onRotation,//旋转中
    }
    
    /// <summary>
    /// 物体旋转方向
    /// </summary>
    public enum SpinnerRotationDir
    {
        Left,
        Right,
    }
}
