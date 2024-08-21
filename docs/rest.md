# Resources
Most resources have a unique alphanumeric identifier. Allocate at least 30 characters of space to store the ID.
## Devices
### POST `/devices`
Creates a new libmapper device. Polling is handled by `mapperd` and clients can assume the device is ready to use as soon as the responce is recieved. 

Example request body:
```json
{
    name: "My Device"
}
```

Example response:
```json
{
    successful: true,
    device_id: "n3ji4k438$nk3l4l"
}
```

## Signals
All signal routes are scoped to a specific device, so you will have had to have created a device before using any of the following routes.

### POST `/devices/[device]/signals`

Creates a new signal attached to the specified device identifier. You must provide a name and data type for the signal, along with some optional metadata.

Creating a signal automatically subscribes to it's value, and the session websocket will recieve updates to the value.

Example request body:
```json
{
    name: "Brightness"
    direction: 1, // valid values are 1 for incoming, 2 for outgoing,
    type: 0, // 0 for double, 1 for int32
    
    // optional params
    vector_length: 1,
    min: 0,
    max: 100,
    units: "Candelas"
}
```

Example response:
```json
{
    successful: true,
    signal_id: "klmgles$3c1ldea"
}

```

### DELETE `/devices/[device]/signals/[signal]`

Deletes the specified signal. All maps to it will be destroyed and all websockets will be unsubscribed from it. 

No request body is required. On success, returns HTTP 204 No Content.

