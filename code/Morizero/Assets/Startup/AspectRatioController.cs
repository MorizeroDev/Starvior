using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Events;

/// <summary>
///强制设置Unity游戏窗口的长宽比。你可以调整窗口的大小，他会强制保持一定比例
///通过拦截窗口大小调整事件(WindowProc回调)并相应地修改它们来实现的
///也可以用像素为窗口设置最小/最大宽度和高度
///长宽比和最小/最大分辨率都与窗口区域有关，标题栏和边框不包括在内
///该脚本还将在应用程序处于全屏状态时强制设置长宽比。当你切换到全屏，
///应用程序将自动设置为当前显示器上可能的最大分辨率，而仍然保持固定比。如果显示器没有相同的宽高比，则会在左/右或上/下添加黑条
///确保你在PlayerSetting中设置了“Resizable Window”，否则无法调整大小
///如果取消不支持的长宽比在PlayerSetting中设置“Supported Aspect Rations”
///注意:因为使用了WinAPI，所以只能在Windows上工作。在Windows 10上测试过
/// </summary>
public class AspectRatioController : MonoBehaviour
{
    /// <summary>
    /// 每当窗口分辨率改变或用户切换全屏时，都会触发此事件
    ///  参数是新的宽度、高度和全屏状态(true表示全屏)
    /// </summary>
    public static bool seted = false;
    public bool avaliable = false;
    public ResolutionChangedEvent resolutionChangedEvent;
    [Serializable]
    public class ResolutionChangedEvent : UnityEvent<int, int, bool> { }

    // 如果为false，则阻止切换到全屏
    [SerializeField]
    private bool allowFullscreen = true;

    // 长宽比的宽度和高度
    [SerializeField]
    private float aspectRatioWidth = 16;
    [SerializeField]
    private float aspectRatioHeight = 9;

    // 最小值和最大值的窗口宽度/高度像素
    [SerializeField]
    private int minWidthPixel = 512;
    [SerializeField]
    private int minHeightPixel = 512;
    [SerializeField]
    private int maxWidthPixel = 2048;
    [SerializeField]
    private int maxHeightPixel = 2048;

    // 当前锁定长宽比。
    private float aspect;

    // 窗口的宽度和高度。不包括边框和窗口标题栏
    // 当调整窗口大小时，就会设置这些值
    private int setWidth = -1;
    private int setHeight = -1;

    // 最后一帧全屏状态。
    private bool wasFullscreenLastFrame;

    // 是否初始化了AspectRatioController
    // 一旦注册了WindowProc回调函数，就将其设置为true
    private bool started;

    // 显示器的宽度和高度。这是窗口当前打开的监视器
    private int pixelHeightOfCurrentScreen;
    private int pixelWidthOfCurrentScreen;

    //一旦用户请求终止applaction，则将其设置为true
    private bool quitStarted;

    // WinAPI相关定义
#region WINAPI

    // 当窗口调整时,WM_SIZING消息通过WindowProc回调发送到窗口
    private const int WM_SIZING = 0x214;

    // WM大小调整消息的参数
    private const int WMSZ_LEFT = 1;
    private const int WMSZ_RIGHT = 2;
    private const int WMSZ_TOP = 3;
    private const int WMSZ_BOTTOM = 6;

    // 获取指向WindowProc函数的指针
    private const int GWLP_WNDPROC = -4;

    // 委托设置为新的WindowProc回调函数
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private WndProcDelegate wndProcDelegate;

    // 检索调用线程的线程标识符
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    // 检索指定窗口所属类的名称
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    // 通过将句柄传递给每个窗口，依次传递给应用程序定义的回调函数，枚举与线程关联的所有非子窗口
    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    // 将消息信息传递给指定的窗口过程
    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    // 检索指定窗口的边框的尺寸
    // 尺寸是在屏幕坐标中给出的，它是相对于屏幕左上角的
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);

    //检索窗口客户区域的坐标。客户端坐标指定左上角
    //以及客户区的右下角。因为客户机坐标是相对于左上角的
    //在窗口的客户区域的角落，左上角的坐标是(0,0)
    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

    // 更改指定窗口的属性。该函数还将指定偏移量的32位(长)值设置到额外的窗口内存中
    [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    //更改指定窗口的属性。该函数还在额外的窗口内存中指定的偏移量处设置一个值
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    //用于查找窗口句柄的Unity窗口类的名称
    private const string UNITY_WND_CLASSNAME = "UnityWndClass";

    // Unity窗口的窗口句柄
    private IntPtr unityHWnd;

    // 指向旧WindowProc回调函数的指针
    private IntPtr oldWndProcPtr;

    // 指向我们自己的窗口回调函数的指针
    private IntPtr newWndProcPtr;

    /// <summary>
    /// WinAPI矩形定义。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

#endregion

    void Start()
    {
        if (!avaliable) return;
        // 不要在Unity编辑器中注册WindowProc回调函数，它会指向Unity编辑器窗口，而不是Game视图
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        //注册回调，然后应用程序想要退出
        Application.wantsToQuit += ApplicationWantsToQuit;
 
        // 找到主Unity窗口的窗口句柄
        EnumThreadWindows(GetCurrentThreadId(), (hWnd, lParam) =>
        {
            var classText = new StringBuilder(UNITY_WND_CLASSNAME.Length + 1);
            GetClassName(hWnd, classText, classText.Capacity);
 
            if (classText.ToString() == UNITY_WND_CLASSNAME)
            {
                unityHWnd = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);
 
        // 将长宽比应用于当前分辨率
        SetAspectRatio(aspectRatioWidth, aspectRatioHeight, true);
 
        // 保存当前的全屏状态
        wasFullscreenLastFrame = Screen.fullScreen;
 
        // Register (replace) WindowProc callback。每当一个窗口事件被触发时，这个函数都会被调用
        //例如调整大小或移动窗口
        //保存旧的WindowProc回调函数，因为必须从新回调函数中调用它
        wndProcDelegate = wndProc;
        newWndProcPtr = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
        oldWndProcPtr = SetWindowLong(unityHWnd, GWLP_WNDPROC, newWndProcPtr);
 
        // 初始化完成
        started = true;
        int fullscreen = PlayerPrefs.GetInt("Settings.IsFullScreen", 1);
        Screen.fullScreen = (fullscreen == 0);
        DontDestroyOnLoad(gameObject);
#endif

    }

    private void Awake()
    {
        if (!seted)
        {
            seted = true;
            avaliable = true;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    ///将目标长宽比设置为给定的长宽比。
    /// </summary>
    /// <param name="newAspectWidth">宽高比的新宽度</param>
    /// <param name="newAspectHeight">纵横比的新高度</param>
    /// <param name="apply">true，当前窗口分辨率将立即调整以匹配新的纵横比 false，则只在下次手动调整窗口大小时执行此操作</param>
    public void SetAspectRatio(float newAspectWidth, float newAspectHeight, bool apply)
    {
        //计算新的纵横比
        aspectRatioWidth = newAspectWidth;
        aspectRatioHeight = newAspectHeight;
        aspect = aspectRatioWidth / aspectRatioHeight;

        // 调整分辨率以匹配长宽比(触发WindowProc回调)
        if (apply)
        {
            Screen.SetResolution(Screen.width, Mathf.RoundToInt(Screen.width / aspect), Screen.fullScreen);
        }
    }

    /// <summary>
    /// WindowProc回调。应用程序定义的函数，用来处理发送到窗口的消息 
    /// </summary>
    /// <param name="msg">用于标识事件的消息</param>
    /// <param name="wParam">额外的信息信息。该参数的内容取决于uMsg参数的值 </param>
    /// <param name="lParam">其他消息的信息。该参数的内容取决于uMsg参数的值 </param>
    /// <returns></returns>
    IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        // 检查消息类型
        // resize事件
        if (msg == WM_SIZING)
        {
            // 获取窗口大小结构体
            RECT rc = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

            // 计算窗口边框的宽度和高度
            RECT windowRect = new RECT();
            GetWindowRect(unityHWnd, ref windowRect);

            RECT clientRect = new RECT();
            GetClientRect(unityHWnd, ref clientRect);

            int borderWidth = windowRect.Right - windowRect.Left - (clientRect.Right - clientRect.Left);
            int borderHeight = windowRect.Bottom - windowRect.Top - (clientRect.Bottom - clientRect.Top);

            // 在应用宽高比之前删除边框(包括窗口标题栏)
            rc.Right -= borderWidth;
            rc.Bottom -= borderHeight;

            // 限制窗口大小
            int newWidth = Mathf.Clamp(rc.Right - rc.Left, minWidthPixel, maxWidthPixel);
            int newHeight = Mathf.Clamp(rc.Bottom - rc.Top, minHeightPixel, maxHeightPixel);

            // 根据纵横比和方向调整大小
            switch (wParam.ToInt32())
            {
                case WMSZ_LEFT:
                    rc.Left = rc.Right - newWidth;
                    rc.Bottom = rc.Top + Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_RIGHT:
                    rc.Right = rc.Left + newWidth;
                    rc.Bottom = rc.Top + Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_TOP:
                    rc.Top = rc.Bottom - newHeight;
                    rc.Right = rc.Left + Mathf.RoundToInt(newHeight * aspect);
                    break;
                case WMSZ_BOTTOM:
                    rc.Bottom = rc.Top + newHeight;
                    rc.Right = rc.Left + Mathf.RoundToInt(newHeight * aspect);
                    break;
                case WMSZ_RIGHT + WMSZ_BOTTOM:
                    rc.Right = rc.Left + newWidth;
                    rc.Bottom = rc.Top + Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_RIGHT + WMSZ_TOP:
                    rc.Right = rc.Left + newWidth;
                    rc.Top = rc.Bottom - Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_LEFT + WMSZ_BOTTOM:
                    rc.Left = rc.Right - newWidth;
                    rc.Bottom = rc.Top + Mathf.RoundToInt(newWidth / aspect);
                    break;
                case WMSZ_LEFT + WMSZ_TOP:
                    rc.Left = rc.Right - newWidth;
                    rc.Top = rc.Bottom - Mathf.RoundToInt(newWidth / aspect);
                    break;
            }

            // 保存实际分辨率,不包括边界
            setWidth = rc.Right - rc.Left;
            setHeight = rc.Bottom - rc.Top;

            // 添加边界
            rc.Right += borderWidth;
            rc.Bottom += borderHeight;

            // 触发分辨率更改事件
            resolutionChangedEvent.Invoke(setWidth, setHeight, Screen.fullScreen);

            // 回写更改的窗口参数
            Marshal.StructureToPtr(rc, lParam, true);
        }

        // 调用原始的WindowProc函数
        return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
    }

    void Update()
    {
        if (!avaliable) return;
        if (Input.GetKeyUp(KeyCode.F11) || ((Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt)) && Input.GetKeyUp(KeyCode.Return)))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        // 如果不允许全屏，则阻止切换到全屏
        if (!allowFullscreen && Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }

        if (Screen.fullScreen && !wasFullscreenLastFrame)
        {
            //切换到全屏检测,设置为最大屏幕分辨率，同时保持长宽比
            int height;
            int width;

            //根据当前长宽比和显示器的比例进行比较，上下或左右添加黑边
            bool blackBarsLeftRight = aspect < (float)pixelWidthOfCurrentScreen / pixelHeightOfCurrentScreen;

            if (blackBarsLeftRight)
            {
                height = pixelHeightOfCurrentScreen;
                width = Mathf.RoundToInt(pixelHeightOfCurrentScreen * aspect);
            }
            else
            {
                width = pixelWidthOfCurrentScreen;
                height = Mathf.RoundToInt(pixelWidthOfCurrentScreen / aspect);
            }

            Screen.SetResolution(width, height, true);
            resolutionChangedEvent.Invoke(width, height, true);
        }
        else if (!Screen.fullScreen && wasFullscreenLastFrame)
        {
            // 从全屏切换到检测到的窗口。设置上一个窗口的分辨率。
            Screen.SetResolution(setWidth, setHeight, false);
            resolutionChangedEvent.Invoke(setWidth, setHeight, false);
        }
        else if (!Screen.fullScreen && setWidth != -1 && setHeight != -1 && (Screen.width != setWidth || Screen.height != setHeight))
        {
            //根据高度设置宽度，因为Aero Snap不会触发WM_SIZING。
            setHeight = Screen.height;
            setWidth = Mathf.RoundToInt(Screen.height * aspect);

            Screen.SetResolution(setWidth, setHeight, Screen.fullScreen);
            resolutionChangedEvent.Invoke(setWidth, setHeight, Screen.fullScreen);
        }
        else if (!Screen.fullScreen)
        {
            // 保存当前屏幕的分辨率
            // 下次切换到全屏时，此分辨率将被设置为窗口分辨率
            // 只有高度，如果需要，宽度将根据高度和长宽比设置，以确保长宽比保持在全屏模式
            pixelHeightOfCurrentScreen = Screen.currentResolution.height;
            pixelWidthOfCurrentScreen = Screen.currentResolution.width;
        }

        //保存下一帧的全屏状态
        wasFullscreenLastFrame = Screen.fullScreen;

        // 当游戏窗口调整大小时，在编辑器中触发分辨率改变事件。
#if UNITY_EDITOR
        if (Screen.width != setWidth || Screen.height != setHeight)
        {
            setWidth = Screen.width;
            setHeight = Screen.height;
            resolutionChangedEvent.Invoke(setWidth, setHeight, Screen.fullScreen);
        }
#endif
    }

    /// <summary>
    /// 调用SetWindowLong32或SetWindowLongPtr64，取决于可执行文件是32位还是64位。
    /// 这样，我们就可以同时构建32位和64位的可执行文件而不会遇到问题。
    /// </summary>
    /// <param name="hWnd">The window handle.</param>
    /// <param name="nIndex">要设置的值的从零开始的偏移量</param>
    /// <param name="dwNewLong">The replacement value.</param>
    /// <returns>返回值是指定偏移量的前一个值。否则零.</returns>
    private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        //32位系统
        if (IntPtr.Size == 4)
        {
            return SetWindowLong32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    /// <summary>
    /// 退出时调用。 返回false将中止并使应用程序保持活动。True会让它退出。
    /// </summary>
    /// <returns></returns>
    private bool ApplicationWantsToQuit()
    {
        //仅允许在应用程序初始化后退出。
        if (!started)
            return false;

        //延迟退出，clear up
        if (!quitStarted)
        {
            StartCoroutine("DelayedQuit");
        }

        return true;
    }

    /// <summary>
    /// 恢复旧的WindowProc回调，然后退出。
    /// </summary>
    IEnumerator DelayedQuit()
    {

        // 重新设置旧的WindowProc回调,如果检测到WM_CLOSE,这将在新的回调本身中完成, 64位没问题，32位可能会造成闪退

        SetWindowLong(unityHWnd, GWLP_WNDPROC, oldWndProcPtr);

        yield return new WaitForEndOfFrame();

        quitStarted = true;
        Application.Quit();
    }
}
