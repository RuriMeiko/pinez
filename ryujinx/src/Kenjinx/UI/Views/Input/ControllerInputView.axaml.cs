using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.ViewModels.Input;
using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Input;
using Ryujinx.Input.Assigner;
using StickInputId = Ryujinx.Common.Configuration.Hid.Controller.StickInputId;

namespace Ryujinx.Ava.UI.Views.Input
{
    public partial class ControllerInputView : UserControl
    {
        private ButtonKeyAssigner _currentAssigner;

        public ControllerInputView()
        {
            InitializeComponent();

            foreach (ILogical visual in SettingButtons.GetLogicalDescendants())
            {
                if (visual is ToggleButton button and not CheckBox)
                {
                    button.IsCheckedChanged += Button_IsCheckedChanged;
                }

                if (visual is CheckBox check)
                {
                    check.IsCheckedChanged += CheckBox_IsCheckedChanged;
                }

                if (visual is Slider slider)
                {
                    slider.PropertyChanged += Slider_IsCheckedChanged;
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (_currentAssigner != null && _currentAssigner.ToggledButton != null && !_currentAssigner.ToggledButton.IsPointerOver)
            {
                _currentAssigner.Cancel();
            }
        }

        private float _changeSlider = -1.0f;

        private void Slider_IsCheckedChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is Slider check)
            {
                if ((bool)check.IsPointerOver && _changeSlider == -1.0f)
                {
                    _changeSlider = (float)check.Value;
                    
                }
                else if (!(bool)check.IsPointerOver)
                {
                    _changeSlider = -1.0f;
                }

                if (_changeSlider != -1.0f && _changeSlider != (float)check.Value)
                {

                     var viewModel = (DataContext as ControllerInputViewModel);
                     if (viewModel != null)
                     {
                         viewModel.ParentModel.IsModified = true;
                     }

                     _changeSlider = (float)check.Value;
                }
            }
        }

        private void CheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox check)
            {
                if ((bool)check.IsPointerOver)
                {
                    if (DataContext is ControllerInputViewModel viewModel)
                    {
                        viewModel.ParentModel.IsModified = true;
                    }

                    _currentAssigner?.Cancel();
                    _currentAssigner = null;
                }
            }
        }


        private void Button_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button ) 
            {
                if (button.IsChecked != null && (bool)button.IsChecked)
                {
                    if (_currentAssigner != null && button == _currentAssigner.ToggledButton)
                    {
                        return;
                    }

                    bool isStick = button.Tag != null && button.Tag.ToString() == "stick";

                    if (_currentAssigner == null)
                    {
                        _currentAssigner = new ButtonKeyAssigner(button);

                        this.Focus(NavigationMethod.Pointer);

                        PointerPressed += MouseClick;

                        var viewModel = (DataContext as ControllerInputViewModel);

                        IKeyboard keyboard = (IKeyboard)viewModel.ParentModel.AvaloniaKeyboardDriver.GetGamepad("0"); // Open Avalonia keyboard for cancel operations.
                        IButtonAssigner assigner = CreateButtonAssigner(isStick);

                        _currentAssigner.ButtonAssigned += (sender, e) =>
                        {
                            if (e.ButtonValue.HasValue)
                            {
                                var buttonValue = e.ButtonValue.Value;
                                viewModel.ParentModel.IsModified = true;

                                switch (button.Name)
                                {
                                    case "ButtonZl":
                                        viewModel.Config.ButtonZl = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonL":
                                        viewModel.Config.ButtonL = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonMinus":
                                        viewModel.Config.ButtonMinus = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "LeftStickButton":
                                        viewModel.Config.LeftStickButton = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "LeftJoystick":
                                        viewModel.Config.LeftJoystick = buttonValue.AsHidType<StickInputId>();
                                        break;
                                    case "DpadUp":
                                        viewModel.Config.DpadUp = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "DpadDown":
                                        viewModel.Config.DpadDown = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "DpadLeft":
                                        viewModel.Config.DpadLeft = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "DpadRight":
                                        viewModel.Config.DpadRight = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "LeftButtonSr":
                                        viewModel.Config.LeftButtonSr = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "LeftButtonSl":
                                        viewModel.Config.LeftButtonSl = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "RightButtonSr":
                                        viewModel.Config.RightButtonSr = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "RightButtonSl":
                                        viewModel.Config.RightButtonSl = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonZr":
                                        viewModel.Config.ButtonZr = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonR":
                                        viewModel.Config.ButtonR = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonPlus":
                                        viewModel.Config.ButtonPlus = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonA":
                                        viewModel.Config.ButtonA = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonB":
                                        viewModel.Config.ButtonB = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonX":
                                        viewModel.Config.ButtonX = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "ButtonY":
                                        viewModel.Config.ButtonY = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "RightStickButton":
                                        viewModel.Config.RightStickButton = buttonValue.AsHidType<GamepadInputId>();
                                        break;
                                    case "RightJoystick":
                                        viewModel.Config.RightJoystick = buttonValue.AsHidType<StickInputId>();
                                        break;
                                }
                            }
                        };

                        _currentAssigner.GetInputAndAssign(assigner, keyboard);
                    }
                    else
                    {
                        if (_currentAssigner != null )
                        {
                            _currentAssigner.Cancel();
                            _currentAssigner = null;
                            button.IsChecked = false;
                        }
                    }
                }
                else
                {
                    _currentAssigner?.Cancel();
                    _currentAssigner = null;
                }
            }
        }

        private void MouseClick(object sender, PointerPressedEventArgs e)
        {
            bool shouldUnbind = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;

            _currentAssigner?.Cancel(shouldUnbind);

            PointerPressed -= MouseClick;
        }

        private IButtonAssigner CreateButtonAssigner(bool forStick)
        {
            IButtonAssigner assigner;

            var controllerInputViewModel = DataContext as ControllerInputViewModel;

            assigner = new GamepadButtonAssigner(
                controllerInputViewModel?.ParentModel.SelectedGamepad,
                (controllerInputViewModel.ParentModel.Config as StandardControllerInputConfig).TriggerThreshold,
                forStick);

            return assigner;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _currentAssigner?.Cancel();
            _currentAssigner = null;
        }
    }
}
