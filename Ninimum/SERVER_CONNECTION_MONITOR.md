# Ninimum server connection monitor

The connection monitor is global for every `BasePage`, including Login.

## Stable connection behavior

The monitor now separates **backend reachability** from normal API/business errors.

- Any HTTP response from `/actuator/health` proves that the backend HTTP process is reachable.
- One timed-out heartbeat never shows `ConnectionStatusView`.
- Three consecutive background probe failures are required before the banner appears.
- A recent successful real API response has higher priority than a failed background heartbeat, preventing banner flicker while pages such as `MyProfilePage` load several requests.
- If a normal API request has a transport failure, the monitor confirms it with two heartbeat probes before showing a global connection error.
- Multiple simultaneous API failures share one confirmation task.
- When the backend becomes reachable again, one successful heartbeat or API response hides the banner immediately.
- GET requests that were genuinely blocked by an outage resume automatically after recovery.

## UI rules

- Connected + page is loading: `LoadingView`
- No internet: red `ConnectionStatusView`
- Backend unreachable: orange `ConnectionStatusView`
- Reconnecting: orange `ConnectionStatusView` with spinner
- `LoadingView` stays hidden while the connection banner is visible

## Recommended test

1. Run backend and open Main/Profile. The banner must stay hidden.
2. Navigate repeatedly between Main and MyProfilePage. The banner must not blink.
3. Stop backend. After confirmed failures, the orange banner appears.
4. Start backend. The banner disappears automatically and pending GET data resumes.
5. Disable internet. The red no-internet banner appears.

## Disconnected screen interaction behavior

When `ConnectionStatusView` is visible, `BasePage` now places a lightweight interaction shield above the page content (below the existing connection banner).

- Existing/last-loaded content remains visible so the page does not jump to a blank screen.
- The content is softly de-emphasized while disconnected.
- Taps, buttons, `RefreshView` pull-down gestures, `SwipeView`, list-item navigation, and manual infinite-scroll gestures cannot reach the page underneath.
- `LoadingView` remains hidden while the connection banner is visible, as before.
- The shield is bound to `ConnectionMonitorService.IsBannerVisible`, so it disappears automatically as soon as the server connection is restored.
- `ConnectionStatusView` itself is unchanged and remains tappable for manual retry.

This behavior is centralized in `BasePage`; individual pages do not need separate offline-disable logic.
