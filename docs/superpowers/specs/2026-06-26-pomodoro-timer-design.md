# Pomodoro Timer — Design Spec

## Overview

A minimal Windows desktop pomodoro timer. Small always-on-top WinForms window showing a countdown, with buttons to select work/break modes.

## Technology

- .NET 10, WinForms
- Single self-contained Windows exe
- Project lives in `C:\Dev\Personal\Pom`

## Window

- Fixed size (~280×180 pixels)
- Always-on-top (`TopMost = true`)
- Standard title bar — minimizable, closable (no maximize)
- Not resizable

### Layout (top to bottom)

1. **Mode label** — shows current mode name (e.g. "Work", "Short Break", "Long Break")
2. **Countdown label** — large centered text, e.g. "25:00"
3. **Control buttons row** — Start/Pause, Reset
4. **Mode buttons row** — Work, Short Break, Long Break

## Behavior

### Modes & Durations

| Mode        | Duration |
|-------------|----------|
| Work        | 25:00    |
| Short Break | 05:00    |
| Long Break  | 15:00    |

### Interactions

- Clicking a mode button sets that mode, resets the countdown to the mode's duration, and stops the timer if running.
- **Start** begins the countdown; button label changes to **Pause**.
- **Pause** stops the countdown; button label reverts to **Start**.
- **Reset** stops the timer and resets to the current mode's full duration.
- When countdown reaches 0:00, notifications fire based on settings.

### Notifications (on timer complete)

Configurable via `settings.json`:

| Setting       | Type | Default | Behavior                          |
|---------------|------|---------|-----------------------------------|
| `playSound`   | bool | true    | Plays system beep/ding            |
| `showToast`   | bool | false   | Shows a Windows toast notification|
| `flashWindow` | bool | false   | Flashes the taskbar button        |

## Configuration

- File: `settings.json` in the same directory as the exe
- Created with defaults on first run if missing
- Read at startup; no in-app settings UI (edit the file directly)

## Project Structure

```
Pom/
├── Pom.csproj
├── Program.cs          # Entry point
├── MainForm.cs         # Window, UI, timer logic
├── Settings.cs         # Settings model + load/save
└── docs/
    └── superpowers/
        └── specs/
            └── 2026-06-26-pomodoro-timer-design.md
```

## Out of Scope

- Automatic work→break cycling / session counting
- In-app settings UI
- Cross-platform support
- Theming / dark mode
