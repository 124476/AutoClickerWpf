using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using MessageBox = System.Windows.MessageBox;

namespace Autocliker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int WH_KEYBOARD_LL = 13;
        private LowLevelKeyboardProcDelegate m_callback;
        private IntPtr m_hHook;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr LowLevelKeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var khs = (KeyboardHookStruct)
                          Marshal.PtrToStructure(lParam,
                          typeof(KeyboardHookStruct));
                if (Convert.ToInt32("" + wParam) == 256)
                {
                    if ((int)khs.VirtualKeyCode == (int)Key.E + 21)  //E
                        StopFun();
                    if ((int)khs.VirtualKeyCode == (int)Key.Q + 21)  //Q
                        StartFun();
                    if ((int)khs.VirtualKeyCode == (int)Key.R + 21)  //R
                        PickFun();
                }
            }
            return CallNextHookEx(m_hHook, nCode, wParam, lParam);
        }

        private void PickFun()
        {
            // обновление информации происходит каждые 10 мс
            Point defPnt = new Point();
            // заполняем defPnt информацией о координатах мышки
            GetCursorPos(ref defPnt);
            MessageBox.Show(defPnt.ToString());
            // выводим информацию в окно
            YValue.Text = defPnt.Y.ToString();
            XValue.Text = defPnt.X.ToString();
        }

        async private void StartFun()
        {
            isStart = true;
            int X = 65535 / (int)ScreenWidth * Convert.ToInt32(XValue.Text);
            int Y = 65535 / (int)ScreenHeight * Convert.ToInt32(YValue.Text);

            while (isStart)
            {
                mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, X, Y, 0, 0);
                //Выполнение первого клика левой клавишей мыши
                mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                await Task.Delay(60000 * Convert.ToInt32(XValue.Text) + 1000 * Convert.ToInt32(YValue.Text));
            }
        }

        private void StopFun()
        {
            isStart = false;
            XValue.Text = "0";
            YValue.Text = "0";
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            public readonly int VirtualKeyCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr ExtraInfo;
        }
        private delegate IntPtr LowLevelKeyboardProcDelegate(
            int nCode, IntPtr wParam, IntPtr lParam);

        public void SetHook()
        {
            m_callback = LowLevelKeyboardHookProc;
            m_hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                m_callback,
                GetModuleHandle(IntPtr.Zero), 0);
        }
        public void Unhook()
        {
            UnhookWindowsHookEx(m_hHook);
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        static protected long totalPixels = 0;
        static protected int diffX;
        static protected int diffY;
        bool isStart = false;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        //Нормированные абсолютные координаты
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //Нажатие на левую кнопку мыши
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //Поднятие левой кнопки мыши
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        //Нажатие на правой кнопку мыши
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //Поднятие правой кнопки мыши
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //перемещение указателя мыши
        private const int MOUSEEVENTF_MOVE = 0x0001;

        int value;
        public MainWindow()
        {
            InitializeComponent();
            SetHook();
            this.Focus();
        }

        private void UpBtn_Click(object sender, RoutedEventArgs e)
        {
            value++;
            TextValue.Text = value.ToString();
        }

        private void DownBtn_Click(object sender, RoutedEventArgs e)
        {
            if (value > 0)
                value--;
            TextValue.Text = value.ToString();
        }

        double ScreenWidth = SystemParameters.VirtualScreenWidth;
        double ScreenHeight = SystemParameters.VirtualScreenHeight;
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartFun();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopFun();
        }

        private void TextValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char number = Char.Parse(e.Text);

            if (!char.IsDigit(number))
                e.Handled = true;
        }
        
        private void PickBtn_Click(object sender, RoutedEventArgs e)
        {
            PickFun();
        }
    }
}
