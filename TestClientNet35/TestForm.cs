﻿using CommonLibrary;
using CommonLibrary.Tools;
using CommonLibrary.UI;
using CommonLibrary.Utilities;
using CommonLibrary.Web;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace TestNet32
{
    public partial class TestForm : Form
    {
        private bool _isMultiScreen;

        private MouseManager _mouseManager;
        private WindowManager _windowManager;

        private ScreenImageCapture _screenImageCapture;
        public TestForm()
        {
            InitializeComponent();

            _isMultiScreen = Screen.AllScreens.Length > 1;
        }


        private void TestForm_Load(object sender, EventArgs e)
        {

        }

        private void TestForm_Shown(object sender, EventArgs e)
        {

        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isMultiScreen)
            {
                if (_mouseManager != null)
                {
                    _mouseManager.StopWorkerThread();
                    _mouseManager = null;
                }

                if (_windowManager != null)
                {
                    _windowManager.StopWorkerThread();
                    _windowManager = null;
                }

                if (_screenImageCapture != null)
                {
                    _screenImageCapture.Stop();
                    _screenImageCapture = null;
                }
            }
        }

        private void ButtonMouseMovePrevent_Click(object sender, EventArgs e)
        {
            if (_isMultiScreen)
            {
                if (_mouseManager == null)
                {
                    int preventMoveScreenIndex = ScreenUtility.GetFirstScreenIndexAndExceptPrimaryScreenIndex();
                    IMouseFunctionStrategy functionStrategy = new MousePreventMoveScreenFunctionStrategy(preventMoveScreenIndex);
                    _mouseManager = new MouseManager(functionStrategy);
                    _mouseManager.StartWorkerThread();
                    _ButtonMouseMovePrevent.Text = "마우스 이동 제어 정지";
                }
                else
                {
                    _mouseManager.StopWorkerThread();
                    _mouseManager = null;
                    _ButtonMouseMovePrevent.Text = "마우스 이동 제어 시작";
                }
            }
        }

        private void ButtonMoveCursorPoint_Click(object sender, EventArgs e)
        {
            MouseManager.MoveCursorPoint(new Point(10, 10));
        }

        private void ButtonShowScreenIndex_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < Screen.AllScreens.Length; index++)
            {
                Screen screen = Screen.AllScreens[index];
                ScreenIndexDialog dlg = new ScreenIndexDialog(index, screen.Bounds, new ScreenIndexDialogColor()
                {
                    MainBackColor = Color.Green, MainForeColor = Color.White,
                    SubBackColor = Color.Black, SubForeColor = Color.White
                });

                dlg.Show();
            }
        }

        private void ButtonProcessWindowHandleFixedLocation_Click(object sender, EventArgs e)
        {
            if (_isMultiScreen)
            {
                if (_windowManager == null)
                {
                    ProcessesSetWindowPosFunctionStrategy strategy = new ProcessesSetWindowPosFunctionStrategy();
                    strategy.SetProcessAllWindowsHandlePos += ProcessWndHandlesAdjustLocationStrategy_SetProcessAllWindowsHandlePos;

                    _windowManager = new WindowManager(strategy);
                    _windowManager.StartWorkerThread();

                    _ButtonProcessWindowHandleFixedLocation.Text = "창 제어 정지";
                }
                else
                {
                    _windowManager.StopWorkerThread();
                    _windowManager = null;
                    _ButtonProcessWindowHandleFixedLocation.Text = "창 제어 시작";
                }
            }
        }

        private bool ProcessWndHandlesAdjustLocationStrategy_SetProcessAllWindowsHandlePos(object sender, ProcessAllSetWindowPosEventArgs e)
        {
            // 해당 이벤트핸들러에서 특정 프로세스의 모든창에 대하여 윈도우 창 제어를 할지 여부를 처리한다.
            if (e.Process.ProcessName.Equals(Explorer.ProcessName))
            {
                // 특정 프로세스 일때 특정 타이틀값에 해당하는 부분 윈도우 핸들만 처리 가능하다.
                if (e.Text.Equals("파일 탐색기"))
                {
                    return true;
                }
                return false;
            }
            else
            {
                // 일반적으로는 메인 윈도우 핸들만 창 제어를 처리한다.
                if (e.Process.MainWindowHandle == e.Handle)
                {
                    return true;
                }
                return false;
            }
        }

        private void _ButtonHttpToolkitTest_Click(object sender, EventArgs e)
        {
            Dictionary<string, string[]> parameter = new Dictionary<string, string[]>();
            parameter.Add("Test", new string[] { "Test1", "Test2" });
            parameter.Add("Hangeul", new string[] { Uri.EscapeUriString("한글") });

            /*
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("Test1", "Test1");
            parameter.Add("Test2", "Test2");
            parameter.Add("Hangeul", Uri.EscapeUriString("한글"));
            */

            HttpStatusCode statusCode = HttpStatusCode.OK;
            Exception exception = null;

            HttpToolkit httpToolkit = new HttpToolkit();
            httpToolkit.CreateHttpWebRequest += HttpToolkit_CreateHttpWebRequest;
            httpToolkit.CreateHttpWebResponse += HttpToolkit_CreateHttpWebResponse;
            string responseText = httpToolkit.GetResponseByPost(
                "http://localhost:8080/CSharpCommonLibrary/index.jsp",
                parameter,
                "UTF-8",
                "UTF-8",
                10000,
                out statusCode,
                out exception
            );

            _RichTextBoxHttpToolkitTest.Text = responseText;
        }

        private void HttpToolkit_CreateHttpWebRequest(object sender, CreateHttpWebRequestEventArgs e)
        {

        }

        private void HttpToolkit_CreateHttpWebResponse(object sender, CreateHttpWebResponseEventArgs e)
        {

        }

        private void ButtonStartAndStopScreenCapture_Click(object sender, EventArgs e)
        {
            if (_isMultiScreen)
            {
                if (_screenImageCapture == null)
                {
                    _screenImageCapture = new ScreenImageCapture(ScreenUtility.GetFirstScreenIndexAndExceptPrimaryScreenIndex());
                    _screenImageCapture.CreateScreenImageCapture += ScreenImageCapture_CreateScreenImageCapture;
                    _screenImageCapture.Start();

                    _ButtonStartAndStopScreenCapture.Text = "스크린 캡쳐 정지";
                }
                else
                {
                    _screenImageCapture.Stop();
                    _screenImageCapture.CreateScreenImageCapture -= ScreenImageCapture_CreateScreenImageCapture;
                    _screenImageCapture = null;

                    _ButtonStartAndStopScreenCapture.Text = "스크린 캡쳐 시작";
                }
            }
        }

        private void ScreenImageCapture_CreateScreenImageCapture(object sender, ScreenImageCaptureEventArgs e)
        {
            if (e.Exception == null)
            {
                if (_PictureBoxScreenCapture.Image != null)
                {
                    _PictureBoxScreenCapture.Image.Dispose();

                }
                _PictureBoxScreenCapture.SizeMode = PictureBoxSizeMode.StretchImage;
                _PictureBoxScreenCapture.Image = e.Bitmap;
            }
        }
    }
}
