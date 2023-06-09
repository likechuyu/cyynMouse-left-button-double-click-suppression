using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        // 定义鼠标钩子
        private static IntPtr mouseHookId = IntPtr.Zero;

        // 设置双击事件的最大间隔（单位为毫秒）
        public static int ClickTimeLimit = 40;

        // 在鼠标按下时记录时间戳和鼠标位置
        private static int prevClickX = 0;
        private static int prevClickY = 0;
        private static int prevClickTimestamp = 0;

        // Windows API 函数
        private delegate IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static MouseHookProc mouseHookCallback = MouseHookCallback;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        // 钩子处理函数
        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam || MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam))
            {
                MSLLHOOKSTRUCT_EX hookData = (MSLLHOOKSTRUCT_EX)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT_EX));

                switch ((MouseMessages)wParam)
                {
                    case MouseMessages.WM_LBUTTONDOWN:
                        int timeDiff = Environment.TickCount - prevClickTimestamp;
                        if (timeDiff < ClickTimeLimit && hookData.pt.x == prevClickX && hookData.pt.y == prevClickY)
                        {
                            return new IntPtr(1); // 屏蔽短时间双击事件
                        }
                        prevClickX = hookData.pt.x;
                        prevClickY = hookData.pt.y;
                        prevClickTimestamp = Environment.TickCount;
                        break;
                }
            }
            return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
        }

        // 键盘钩子消息
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
        }

        // 结构体定义
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT_EX
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 取消鼠标钩子
            UnhookWindowsHookEx(mouseHookId);
        }

        // 获取当前模块句柄
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClickTimeLimit = int.Parse(Ltime.Text);
            // 判断钩子是否已经注册
            if (mouseHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(mouseHookId);
            }
            // 注册鼠标钩子
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                mouseHookId = SetWindowsHookEx(14, mouseHookCallback, GetModuleHandle(curModule.ModuleName), 0);
            }
            MessageBox.Show("启动成功", "提示");

        }










        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(mouseHookId);
            MessageBox.Show("已关闭", "提示");

        }
    }
}
