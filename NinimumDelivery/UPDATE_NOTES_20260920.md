# Ninimum Delivery update

## 1. Xaritani ochish
- Delivery detail now receives the customer's saved `users.location_latitude` and `users.location_longitude` from the existing Ninimum backend.
- The coordinates are returned only after the courier owns/accepts the delivery.
- `Xaritani ochish` opens Google Maps driving directions to the customer. Google Maps uses the courier/device current location as the starting point.
- If the customer has no saved coordinates, the app falls back to the customer's address text.
- No database migration is required because the existing `users` table already has the coordinate columns.

## 2. Auto login
- After a successful courier login, the token, login state and courier profile are saved in Preferences.
- On the next app start, the Delivery app opens the main shell automatically instead of showing LoginPage.
- Temporary network problems during startup no longer clear the saved login.
- Only explicit Logout clears the saved delivery session.
