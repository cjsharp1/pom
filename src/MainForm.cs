using System.Media;
using System.Runtime.InteropServices;

namespace Pom;

public class MainForm : Form
{
    private readonly TimerDisplay _timerDisplay;
    private readonly PomButton _startPauseButton;
    private readonly PomButton _resetButton;
    private readonly PomButton _workButton;
    private readonly PomButton _shortBreakButton;
    private readonly PomButton _longBreakButton;
    private readonly ToggleSwitch _darkModeToggle;
    private readonly SliderControl _opacitySlider;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly PomSettings _settings;
    private readonly NotifyIcon _trayIcon;

    private int _remainingSeconds;
    private bool _isRunning;
    private string _currentMode = "Work";
    private bool _isDarkMode;

    private static readonly Dictionary<string, int> Durations = new()
    {
        ["Work"] = 25 * 60,
        ["Short Break"] = 5 * 60,
        ["Long Break"] = 15 * 60
    };

    // Each mode carries its own accent so the window's state is readable at a glance.
    private static readonly Dictionary<string, Color> ModeAccents = new()
    {
        ["Work"] = Theme.Accent,
        ["Short Break"] = Color.FromArgb(58, 150, 221),
        ["Long Break"] = Color.FromArgb(124, 106, 214)
    };

    private Color CurrentAccent => ModeAccents[_currentMode];

    public MainForm()
    {
        _settings = PomSettings.Load();
        _isDarkMode = _settings.DarkMode;

        Text = "Pom";
        Icon = IconGenerator.CreateTomatoIcon();
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        TopMost = true;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(300, 310);
        Opacity = _settings.Opacity;

        _darkModeToggle = new ToggleSwitch
        {
            Checked = _isDarkMode
        };
        _darkModeToggle.CheckedChanged += OnDarkModeToggled;

        _opacitySlider = new SliderControl
        {
            Minimum = 10,
            Maximum = 100,
            Value = (int)(_settings.Opacity * 100),
            Size = new Size(150, 26)
        };
        _opacitySlider.ValueChanged += OnOpacityChanged;

        var settingsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 38
        };
        int settingsWidth = _darkModeToggle.Width + 12 + _opacitySlider.Width;
        int settingsX = (300 - settingsWidth) / 2;
        _darkModeToggle.Location = new Point(settingsX, 10);
        _opacitySlider.Location = new Point(settingsX + _darkModeToggle.Width + 12, 10);
        settingsPanel.Controls.Add(_opacitySlider);
        settingsPanel.Controls.Add(_darkModeToggle);

        _timerDisplay = new TimerDisplay
        {
            Dock = DockStyle.Top,
            Height = 170
        };

        _startPauseButton = new PomButton
        {
            Text = "Start",
            Width = 104,
            Height = 36,
            FontSize = 11f,
            ButtonStyle = PomButtonStyle.Primary,
            Accent = Theme.Go
        };
        _startPauseButton.Click += OnStartPauseClick;

        _resetButton = new PomButton
        {
            Text = "Reset",
            Width = 90,
            Height = 36,
            FontSize = 11f,
            ButtonStyle = PomButtonStyle.Secondary
        };
        _resetButton.Click += OnResetClick;

        var controlPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46
        };
        int controlsWidth = _startPauseButton.Width + 8 + _resetButton.Width;
        int controlsX = (300 - controlsWidth) / 2;
        _startPauseButton.Location = new Point(controlsX, 5);
        _resetButton.Location = new Point(controlsX + _startPauseButton.Width + 8, 5);
        controlPanel.Controls.Add(_startPauseButton);
        controlPanel.Controls.Add(_resetButton);

        _workButton = new PomButton { Text = "Work", Width = 75, Height = 28, ButtonStyle = PomButtonStyle.Pill, Accent = ModeAccents["Work"] };
        _workButton.Click += (_, _) => SetMode("Work");

        _shortBreakButton = new PomButton { Text = "Short Break", Width = 90, Height = 28, ButtonStyle = PomButtonStyle.Pill, Accent = ModeAccents["Short Break"] };
        _shortBreakButton.Click += (_, _) => SetMode("Short Break");

        _longBreakButton = new PomButton { Text = "Long Break", Width = 90, Height = 28, ButtonStyle = PomButtonStyle.Pill, Accent = ModeAccents["Long Break"] };
        _longBreakButton.Click += (_, _) => SetMode("Long Break");

        var modePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40
        };
        int modeWidth = _workButton.Width + 6 + _shortBreakButton.Width + 6 + _longBreakButton.Width;
        int modeX = (300 - modeWidth) / 2;
        _workButton.Location = new Point(modeX, 3);
        _shortBreakButton.Location = new Point(modeX + _workButton.Width + 6, 3);
        _longBreakButton.Location = new Point(modeX + _workButton.Width + 6 + _shortBreakButton.Width + 6, 3);
        modePanel.Controls.Add(_workButton);
        modePanel.Controls.Add(_shortBreakButton);
        modePanel.Controls.Add(_longBreakButton);

        Controls.Add(modePanel);
        Controls.Add(controlPanel);
        Controls.Add(_timerDisplay);
        Controls.Add(settingsPanel);

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += OnTimerTick;

        _trayIcon = new NotifyIcon
        {
            Icon = Icon,
            Text = "Pom",
            Visible = false
        };
        _trayIcon.DoubleClick += OnTrayIconDoubleClick;

        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Show", null, (_, _) => RestoreFromTray());
        trayMenu.Items.Add("Exit", null, (_, _) => { _trayIcon.Visible = false; Application.Exit(); });
        _trayIcon.ContextMenuStrip = trayMenu;

        _remainingSeconds = Durations["Work"];
        HighlightActiveMode();
        ApplyTheme();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            _trayIcon.Visible = true;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        base.OnFormClosing(e);
    }

    private void OnTrayIconDoubleClick(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void RestoreFromTray()
    {
        _trayIcon.Visible = false;
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void SetMode(string mode)
    {
        _currentMode = mode;
        _remainingSeconds = Durations[mode];
        _isRunning = false;
        _timer.Stop();
        _startPauseButton.Text = "Start";
        _startPauseButton.Accent = Theme.Go;
        UpdateTimerDisplay();
        HighlightActiveMode();
    }

    private void OnStartPauseClick(object? sender, EventArgs e)
    {
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
            _startPauseButton.Text = "Start";
            _startPauseButton.Accent = Theme.Go;
        }
        else
        {
            if (_remainingSeconds <= 0)
            {
                _remainingSeconds = Durations[_currentMode];
                UpdateTimerDisplay();
            }
            _timer.Start();
            _isRunning = true;
            _startPauseButton.Text = "Pause";
            _startPauseButton.Accent = CurrentAccent;
        }
    }

    private void OnResetClick(object? sender, EventArgs e)
    {
        _timer.Stop();
        _isRunning = false;
        _remainingSeconds = Durations[_currentMode];
        _startPauseButton.Text = "Start";
        _startPauseButton.Accent = Theme.Go;
        UpdateTimerDisplay();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _remainingSeconds--;

        if (_remainingSeconds <= 0)
        {
            _remainingSeconds = 0;
            _timer.Stop();
            _isRunning = false;
            _startPauseButton.Text = "Start";
            _startPauseButton.Accent = Theme.Go;
            UpdateTimerDisplay();
            OnTimerComplete();
            return;
        }

        UpdateTimerDisplay();
    }

    private void OnTimerComplete()
    {
        Opacity = 1.0;
        _opacitySlider.Value = 100;

        if (_settings.PlaySound)
            SystemSounds.Exclamation.Play();

        if (_settings.FlashWindow)
            FlashWindowEx(Handle);

        if (_settings.ShowToast)
            ShowToastNotification();
    }

    private void ShowToastNotification()
    {
        var notification = new NotifyIcon
        {
            Visible = true,
            Icon = SystemIcons.Information,
            BalloonTipTitle = "Pom",
            BalloonTipText = $"{_currentMode} complete!"
        };
        notification.ShowBalloonTip(3000);
        Task.Delay(4000).ContinueWith(_ =>
        {
            notification.Visible = false;
            notification.Dispose();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void UpdateTimerDisplay()
    {
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        _timerDisplay.SetTime($"{minutes:D2}:{seconds:D2}");

        int total = Durations[_currentMode];
        _timerDisplay.Progress = total > 0 ? (double)_remainingSeconds / total : 0;
        _timerDisplay.Caption = _currentMode;
        _timerDisplay.Accent = CurrentAccent;
    }

    private void HighlightActiveMode()
    {
        _workButton.Selected = _currentMode == "Work";
        _shortBreakButton.Selected = _currentMode == "Short Break";
        _longBreakButton.Selected = _currentMode == "Long Break";

        _timerDisplay.Accent = CurrentAccent;
        _timerDisplay.Caption = _currentMode;
        _timerDisplay.Invalidate();
    }

    private void OnDarkModeToggled(object? sender, EventArgs e)
    {
        _isDarkMode = _darkModeToggle.Checked;
        _settings.DarkMode = _isDarkMode;
        _settings.Save();
        ApplyTheme();
    }

    private void OnOpacityChanged(object? sender, EventArgs e)
    {
        Opacity = _opacitySlider.Value / 100.0;
        _settings.Opacity = Opacity;
        _settings.Save();
    }

    private void ApplyTheme()
    {
        var backColor = Theme.Background(_isDarkMode);
        var foreColor = Theme.Text(_isDarkMode);

        BackColor = backColor;
        ForeColor = foreColor;

        ApplyTitleBarTheme();

        foreach (Control control in GetAllControls(this))
        {
            switch (control)
            {
                case PomButton btn:
                    btn.DarkBackground = _isDarkMode;
                    btn.BackColor = backColor;
                    btn.Invalidate();
                    break;
                case TimerDisplay td:
                    td.IsDark = _isDarkMode;
                    td.BackColor = backColor;
                    td.Accent = CurrentAccent;
                    td.Invalidate();
                    break;
                case SliderControl sc:
                    sc.IsDark = _isDarkMode;
                    sc.BackColor = backColor;
                    sc.Invalidate();
                    break;
                case Label lbl:
                    lbl.BackColor = backColor;
                    lbl.ForeColor = foreColor;
                    break;
                default:
                    control.BackColor = backColor;
                    break;
            }
        }

        Invalidate(true);
    }

    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwcpRound = 2;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    /// <summary>Matches the Windows title bar to the app theme.</summary>
    private void ApplyTitleBarTheme()
    {
        if (!IsHandleCreated) return;

        try
        {
            int useDark = _isDarkMode ? 1 : 0;
            DwmSetWindowAttribute(Handle, DwmwaUseImmersiveDarkMode, ref useDark, sizeof(int));

            // Soften the window outline to match the rounded UI.
            int corner = DwmwcpRound;
            DwmSetWindowAttribute(Handle, DwmwaWindowCornerPreference, ref corner, sizeof(int));
        }
        catch
        {
            // Unsupported on older Windows builds; the app still works.
        }
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
        // The first Tab press enables focus rings for the rest of the session.
        if ((keyData & Keys.KeyCode) == Keys.Tab)
        {
            PomButton.KeyboardNavigationUsed = true;
            foreach (var c in GetAllControls(this))
                c.Invalidate();
        }
        return base.ProcessDialogKey(keyData);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTitleBarTheme();
    }

    private static IEnumerable<Control> GetAllControls(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            yield return control;
            foreach (var child in GetAllControls(control))
                yield return child;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

    private static void FlashWindowEx(IntPtr handle)
    {
        FlashWindow(handle, true);
    }
}
