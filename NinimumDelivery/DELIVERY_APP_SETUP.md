# Ninimum Delivery – first version

This package is based on the latest Ninimum mobile/backend sources supplied on 2026-09-20.
The existing customer mobile APIs and Payme code were intentionally left unchanged.

## Delivery app flow

1. Courier signs in with a delivery-worker account.
2. Home shows:
   - available paid orders,
   - courier's active deliveries,
   - today's completed count.
3. Courier opens an available delivery and taps **Accept delivery**.
   - claiming is atomic: two couriers cannot claim the same delivery.
   - customer phone is hidden until the courier accepts the delivery.
4. Courier can call the customer or open the address in Maps.
5. Courier taps **Start delivery** -> order becomes `ON_THE_WAY`.
6. Courier taps **Delivered** -> delivery and customer order become `DELIVERED`.
7. Completed/failed deliveries remain in Delivery History.
8. Profile has online/offline, language (Uzbek/Russian/English), and logout.

## Backend additions

New API root:

`/ninimum/api/v1/delivery-app`

Courier endpoints:

- `POST /login`
- `GET /me`
- `GET /dashboard`
- `GET /available`
- `GET /active`
- `GET /history`
- `POST /detail`
- `PUT /claim`
- `PUT /status`
- `PUT /online`

Temporary admin/Swagger endpoints (until the Ninimum admin web project is built):

- `POST /delivery-app/admin/createWorker`
- `GET /delivery-app/admin/workers`

The backend adds a separate JWT authority: `DELIVERY`. Delivery tokens cannot use normal customer APIs or admin APIs.

The update also removes the old server logging that printed user passwords and full Bearer tokens.

## Database – run this FIRST

Before deploying the updated JAR, run:

```bash
sudo mariadb ninimum < /path/to/20260920_delivery_app.sql
```

The SQL creates only new delivery-app tables:

- `delivery_app_workers`
- `delivery_jobs`
- `delivery_job_tracking`

It does not modify the existing `orders`, `payments`, or Payme tables.
It also imports currently paid, undelivered orders into `delivery_jobs`.

The backend synchronizes newly paid orders into `delivery_jobs` whenever the delivery app loads the dashboard/available list. This was intentionally implemented outside Payme so the existing payment flow is not changed.

## Backend deployment

Build as usual:

```bash
mvn clean package
```

Upload `target/ninimum-backend-1.0.0.jar` to:

`/home/ubuntu/backend_ninimum/`

Then use the deployment helper already configured on the server:

```bash
sudo deploy-ninimum
```

## Create the first courier account

Until the admin web page is built, use Swagger with an ADMIN token and call:

`POST /ninimum/api/v1/delivery-app/admin/createWorker`

Example body:

```json
{
  "fullName": "Test Courier",
  "phoneNumber": "998901234567",
  "password": "choose-a-password",
  "vehicleType": "Car",
  "vehicleNumber": "01 A 123 BC"
}
```

The password is BCrypt-encoded by the backend before storage.

## Delivery mobile project

Project: `NinimumDelivery`

- App id: `com.ninimum.delivery`
- Backend URL: `http://95.182.118.233/ninimum/api/v1/`
- Default language: Uzbek
- Includes Uzbek, Russian, English resources
- Android clear-text HTTP is enabled because Ninimum currently uses the server IP over HTTP.
- Maps are opened in the device Maps/browser using the delivery address; no separate Google Maps SDK key is required for this first version.

Run/build:

```bash
dotnet restore
dotnet build -f net10.0-android
```

Release APK can be configured with a separate delivery-app signing key later. Do not reuse the Ninimum customer app package id.

## Next project

The admin web project is intentionally not implemented yet. The next phase can use the new backend admin endpoints and add product creation/editing, image upload, stock, tariff prices, categories, orders, and delivery-worker management.


## Courier login ID update
The delivery app now signs in with a Ninimum-issued courier ID and password, not a phone number.

For an existing database, run `20260920_delivery_worker_id_login.sql` once. Existing workers receive IDs like `DEL0001`; you can change them afterward.

Example admin create-worker request:
```json
{
  "workerId": "DEL001",
  "fullName": "Test Courier",
  "phoneNumber": null,
  "password": "123456",
  "vehicleType": "Car",
  "vehicleNumber": "01 A 123 BC"
}
```

For local MAUI testing, AppConstants currently points to `http://192.168.219.105:8083/ninimum/api/v1/`.
