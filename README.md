# Breathe Bratislava

## API

To setup API, you need to create `appsettings.json` file in `BP.Configuration` project. You can use `appsettings.example.json` as a template.

Example is made for PostgreSQL database that is prepared in `docker-compose` file.

## Frontend

To setup frontend, you need to create `.env` file in `BP.Frontend` project. You can use `.env.example` as a template.

Its recommended to use `pnpm` as a package manager. You can install it with `npm install -g pnpm`.

To install dependencies, run `pnpm install` in `./frontend` folder.

To run frontend, run `pnpm run dev` in `./frontend` folder.

## Discoveries

### SHMU

Endpoint for getting air quality data from SHMU API. (Reverse engineered)
https://www.shmu.sk/api/v1/airquality/

Known query parameters:

- station_meta - 1/0 - if 1, then return station metadata
- station - station id
- history - how many hours back to get data

Example:
- https://www.shmu.sk/api/v1/airquality/getdata?station_meta=1




Thanks stockio.com for lung icon:
https://www.stockio.com/free-icon/healthy-icons-lungs