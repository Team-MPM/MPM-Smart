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

## Profile 

### info

```http
GET /api/user/info
```
- Permissions: `Permissions.Profile.ViewProfile`
  - Returns: `object {string Username, string Language, bool UseDarkMode, List<string> Permissions, List<string> Roles, Dictionary<string, List<string>> RolePermissions}`

### UserName

```http
GET /api/user/username
```
- Permissions: `Permissions.Profile.ViewProfile`
- Returns: `object {string username}`

```http
POST /api/user/username
```
- Permissions: `Permissions.User.ChangeUsername`
- Content: `object {string username}`
- Returns: StatusCode

### Password

```http
POST /api/user/password
```
- Permissions: `Permissions.User.ChangePassword`
- Content: `object {string oldPassword, string newPassword}`
- Returns: StatusCode

### Language

```http
GET /api/user/language
```
- Permissions: `Permissions.Profile.ViewProfile`
- Returns: `object {string language}`

```http
POST /api/user/language
```
- Permissions: `Permissions.User.EditProfile`
- Content: `object {string language}`
- Returns: StatusCode

### Permissions

```http
GET /api/user/permissions
```
- Permissions: `Permissions.Profile.ViewProfile`
- Returns: `object {List<string> UserPermissions , Dictionary<string, array string> RolePermieeions}`

### UserManagement

```http
GET /api/user/users
```
- Permissions: `Permissions.User.Viewusers`
- Returns: `List<object> {string username, string Language, bool useDarkMode, bool isAdmin}`

```http
POST /api/user/users
```
- Permissions: `Permissions.User.AddUser`
- Content: `object {string username, string password}`
- Returns: StatusCode
- 
```http
DELETE /api/user/users
```
- Permissions: `Permissions.User.RemoveUser`
- Content: `object {string username}`
- Returns: StatusCode

## Roles

```http
GET /api/roles/
```
- Permissions: `Permissions.Role.ViewRoles`
- Returns: `List<object> {int id, string name, List<string> Permissions}`

```http
POST /api/roles/{name}
```
- RouteParams: `string name`
- Permissions: `Permissions.Role.ManageRole`
- Returns: StatusCode
-
```http
DELETE /api/roles/{name}
```
- RouteParams: `string name`
- Permissions: `Permissions.Role.ManageRole`
- Returns: StatusCode


