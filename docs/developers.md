# Using `mapperd` as a developer
`mapperd` is built around web standards, and uses both a REST api and a WebSocket server to provide the libmapper api.
The basic flow is as follows:
- Connect to the websocket endpoint at `/ws`
- Send an opcode 1 `INIT` to get a session token
- Use the REST api with that session token while keeping the websocket connection alive
- Close the websocket when done.

# API
## WebSocket
Path: `/ws`

All websocket messages are sent as JSON text and follow a common format:

```json
{
  "op": <number>,
  "data": any?
}
```
### Opcode 0: `INIT`

```json
{
  "op": 1,
  "data": null
}
```
Request a new session token. 

This must be the first opcode sent. The server will ignore any messages other than this one until it is sent.

### Opcode 1: `RESUME`

<reserved>

### Opcode 2: `SIGNAL_DATA`

```
{
  "op": 2,
  "data": {
    "signal_id": string,
    "value": number | number[]
  }
}
```
This message is sent by the client to set the value of a signal, or is sent from the server to inform the client of 
changes to a signal's value.

If sent from the server, `value` will be either a single number or an array, depending on the vector length of the signal.
Both forms are accepted from the client.

The client is automatically subscribed to updates from all "visible" signals, that is any signals owned by the sessions
attached to the websocket.

### Opcode 3: `CONNECTION_ID`

```json
{
  "op": 3,
  "data": string
}
```
Sent by the server in response to `INIT` or `RESUME`. Is ignored if sent from the client.

If sent in response to `INIT`, `data` will be a connection token that should be stored for future use.

If responding to `RESUME`, `data` will contain the same session token to confirm that the session has been attached to the
websocket.