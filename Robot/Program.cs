using Gadgeteer.Modules.GHIElectronics;
using GHI.Glide;
using GHI.Glide.Display;
using Microsoft.SPOT;
using System;
using GT = Gadgeteer;
using UIButton = GHI.Glide.UI.Button;
using UITextBlock = GHI.Glide.UI.TextBlock;

namespace Robot
{
    public partial class Program
    {
        public enum Action
        {
            Forward,
            Backward,
            Left,
            Right,
            Stop
        }

        private const double SPEED = 1.0;
        private readonly TimeSpan _ticks = new TimeSpan(3000000);

        #region Display

        private const ushort MinTintAmount = 0;
        private const ushort MaxTintAmount = 200;
        private Window _window;
        private UITextBlock _logTextBlock;
        private UIButton[] _buttons;
        private UIButton _forwardButton;
        private UIButton _backwardButton;
        private UIButton _leftButton;
        private UIButton _rightButton;
        private UIButton _stopButton;

        #endregion

        private void ProgramStarted()
        {
            InitializeDisplay();

            #region Buttons

            var timer = new GT.Timer(200);
            timer.Tick += t =>
            {
                button1.ToggleLED();
                button2.ToggleLED();
            };

            button1.Mode = Button.LedMode.OnWhilePressed;
            button1.ButtonReleased += (b, s) =>
            {
                leftLed.BlinkRepeatedly(GT.Color.Blue, _ticks, GT.Color.Red, _ticks);
                rightLed.BlinkRepeatedly(GT.Color.Red, _ticks, GT.Color.Blue, _ticks);
                timer.Start();
            };

            button2.Mode = Button.LedMode.OnWhilePressed;
            button2.ButtonReleased += (b, s) =>
            {
                leftLed.TurnOff();
                rightLed.TurnOff();
                timer.Stop();
                button1.TurnLedOff();
                button2.TurnLedOff();
            };

            #endregion

            usbHost.MouseConnected += usbHost_MouseConnected;

            leftLed.FadeRepeatedly(GT.Color.Cyan);
            rightLed.FadeRepeatedly(GT.Color.Cyan);

            Log("Ready to go!");
        }

        private void usbHost_MouseConnected(USBHost sender, GHI.Usb.Host.Mouse mouse)
        {
            mouse.ButtonChanged += (s, e) =>
            {
                switch (e.Which)
                {
                    case GHI.Usb.Buttons.Left:
                        Move(Action.Left);
                        break;
                    case GHI.Usb.Buttons.Middle:
                        Move(Action.Stop);
                        break;
                    case GHI.Usb.Buttons.Right:
                        Move(Action.Right);
                        break;
                    case GHI.Usb.Buttons.Extended1: // back button
                        Move(Action.Backward);
                        break;
                    case GHI.Usb.Buttons.Extended2: // forward button
                        Move(Action.Forward);
                        break;
                };
            };

            Log("Mouse Connected!");
        }

        private void Move(Action action)
        {
            switch (action)
            {
                case Action.Forward:
                    Log("Moving forward...");
                    SetButtonTintAmount(_forwardButton);
                    leftLed.TurnGreen();
                    rightLed.TurnGreen();
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, -SPEED);
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, -SPEED);
                    break;
                case Action.Backward:
                    Log("Moving backward...");
                    SetButtonTintAmount(_backwardButton);
                    leftLed.TurnWhite();
                    rightLed.TurnWhite();
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, SPEED);
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, SPEED);
                    break;
                case Action.Left:
                    Log("Turning left...");
                    SetButtonTintAmount(_leftButton);
                    leftLed.BlinkRepeatedly(GT.Color.Yellow, _ticks, GT.Color.Black, _ticks);
                    rightLed.TurnGreen();
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, -SPEED);
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, SPEED);
                    break;
                case Action.Right:
                    Log("Turning right...");
                    SetButtonTintAmount(_rightButton);
                    leftLed.TurnGreen();
                    rightLed.BlinkRepeatedly(GT.Color.Yellow, _ticks, GT.Color.Black, _ticks);
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, SPEED);
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, -SPEED);
                    break;
                case Action.Stop:
                    Log("Stopped.");
                    SetButtonTintAmount(_stopButton);
                    leftLed.TurnRed();
                    rightLed.TurnRed();
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor1, 0.1);
                    motorDriverL298.SetSpeed(MotorDriverL298.Motor.Motor2, 0.1);
                    motorDriverL298.StopAll();
                    break;
            };
        }

        public void Log(string message)
        {
            Debug.Print(message);
            _logTextBlock.Text = message;
            UpdateDisplayObject(_logTextBlock);
        }

        #region Display

        private void InitializeDisplay()
        {
            _window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Window));

            GlideTouch.Initialize();

            _buttons = new UIButton[5];

            _forwardButton = (UIButton)_window.GetChildByName("forwardButton");
            _forwardButton.TapEvent += s => Move(Action.Forward);
            _buttons[0] = _forwardButton;

            _backwardButton = (UIButton)_window.GetChildByName("backwardButton");
            _backwardButton.TapEvent += s => Move(Action.Backward);
            _buttons[1] = _backwardButton;

            _leftButton = (UIButton)_window.GetChildByName("leftButton");
            _leftButton.TapEvent += s => Move(Action.Left);
            _buttons[2] = _leftButton;

            _rightButton = (UIButton)_window.GetChildByName("rightButton");
            _rightButton.TapEvent += s => Move(Action.Right);
            _buttons[3] = _rightButton;

            _stopButton = (UIButton)_window.GetChildByName("stopButton");
            _stopButton.TapEvent += s => Move(Action.Stop);
            _buttons[4] = _stopButton;

            _logTextBlock = (UITextBlock)_window.GetChildByName("logTextBlock");

            Glide.FitToScreen = true;

            Glide.MainWindow = _window;
        }

        private void SetButtonTintAmount(UIButton selectedButton)
        {
            foreach (var button in _buttons)
            {
                button.TintAmount = button.Equals(selectedButton) ? MaxTintAmount : MinTintAmount;
                UpdateDisplayObject(button);
            }
        }

        private void UpdateDisplayObject(DisplayObject displayObject)
        {
            _window.FillRect(displayObject.Rect);
            displayObject.Invalidate();
        }

        #endregion
    }
}
