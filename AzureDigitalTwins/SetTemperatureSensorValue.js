/// <reference path="handletemperature2.js" />
function process(telemetry, executionContext) {

    // Get sensor metadata
    var sensor = getSensorMetadata(telemetry.SensorId);

    // Retrieve the sensor value
   //  var parseReading = JSON.parse(telemetry.Message);

    // Set the sensor reading as the current value for the sensor.
    setSensorValue(telemetry.SensorId, sensor.DataType, telemetry.Message);
}