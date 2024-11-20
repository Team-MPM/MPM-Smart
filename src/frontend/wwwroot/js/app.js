function getBluetoothDevices() {
    navigator.bluetooth.requestDevice({
        acceptAllDevices: true,
    }).then(device => {
        console.log('Name: ' + device.name);
        console.log('Id: ' + device.id);
        return device;
    }).then(device => {
        return device.gatt.connect()
    }).then(device => {
        return device.gatt.connect();
    }).then(server => {
        return server.getPrimaryService('battery_service');
    }).then(service => {
        return service.getCharacteristic('battery_level');
    }).then(characteristic => {
        return characteristic.readValue();
    }).then(value => {
        console.log(`Battery percentage is ${value.getUint8(0)}`);
    }).catch(error => {
        console.error(error);
    });
}
