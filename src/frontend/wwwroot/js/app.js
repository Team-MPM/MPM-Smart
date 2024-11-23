function getBluetoothDevices() {
    navigator.bluetooth.requestDevice({
        filters: [{ services: ["00000001-810e-4a5b-8d75-3e5b444bc3cf"] }]
    }).then(device => {
        console.log('Name: ' + device.name);
        console.log('Id: ' + device.id);
        return device;
    }).then(device => {
        return device.gatt.connect()
    }).then(device => {
        return device.gatt.connect();
    }).then(server => {
        return server.getPrimaryService('00000001-810e-4a5b-8d75-3e5b444bc3cf');
    }).then(service => {
        return service.getCharacteristic('00000002-810e-4a5b-8d75-3e5b444bc3cf');
    }).then(characteristic => {
        return characteristic.readValue();
    }).then(value => {
        console.log(`Data is ${value.getUint8(0)}`);
    }).catch(error => {
        console.error(error);
    });
}
