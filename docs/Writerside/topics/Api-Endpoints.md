# Api Endpoints

## Format:

### Endpoint:
```http
METHOD /PATH/{id}?page={number}
```
- Permissions: `Permissions.Content.ViewPages`
- RouteParams: id : int
- QueryParams: number: int
- Header: "Token" : string Token
- Content: `object {string [ParameterName]}`
- Returns: StatusCode


### Definitions
- **Permissions:** The permission the user needs to access this resource. `[Root].[Section].[Type]`
- **Method:** Get, Post, Delete, Put, ...
- **Path:** Path on the server under which you can find the resource
- **RouteParams:** `/PATH/[param]`, for example `{id}`: `/PATH/5`
- **QueryParams:** `/PATH?Param=value`, for example `/PATH?page=5`
- **Header:** Parameters inside the header of the request, these contain a key and a value: `Token: "[Token]"`
- **Content:** Params to pass in the body of the request, for example: `object {string SystemName}` -> `{"systemName" : "[value]"}`
- **Returns:** The return value inside the content of the response, for example `object {string SystemName}`. StatusCode means no content.


## Controller

### Systemname
```http
GET /api/settings/systemname
```
- Permissions: `User.Settings.ViewSettings`
- Returns: `object {string SystemName}`

```http
POST /api/settings/systemname
```
- Permissions: `Permissions.Settings.ChangeHostName`
- Content: `object {string SystemName}`
- Returns: StatusCode
- 
### Systemtime

```http
GET /api/settings/systemtime
```
- Permissions: `User.Settings.ViewSettings`
- Returns: `object {string TimeZoneCode}`

```http
POST /api/settings/systemtime
```
- Permissions: `Permissions.Settings.ChangeSystemTime`
- Content: `object {string TimeZoneCode}`
- Returns: StatusCode

### TimeBetweenUpdates

```http
GET /api/settings/timebetweenupdates
```
- Permissions: `User.Settings.ViewSettings`
- Returns: `object {string TimeZoneCode}`

```http
POST /api/settings/timebetweenupdates
```
- Permissions: `Permissions.Settings.ChangeTimeBetweenUpdates`
- Content: `object {int TimeBetweenUpdatesSeconds}`
- Returns: StatusCode

