Weather Simulator

First service is simulator of weather sensors. Service generates events from indoor and outdoor sensors. Each sensor send data: temperature, humidity and CO2 content. The service implements a streaming mode that returns events from sensors.
I made GRPC method that returns the current parameters of a specific sensor. For example, now you need to directly find out what the latest data was on the sensor.

Second sevice is client, which is processing the events from simulator service.
Serivce has subscription to the data from simulator. I define in configuraion file the sensor, what data are required. Service is able to react if the configuration file is changed while the application is running.
The service interacts with the emulator service through a full duplex grpc stream.
He knows how to re-raise the stream, if suddenly there is a break in communication. For example, if the emulator service is stopped, then it will try to connect with it, until the victorious one.
The service saves information on sensors in memory.
The service has an HTTP handler to get the saved data for a particular sensor
The service has an HTTP handler that calls the GRPC handler to get the most recent data.
I integrated my own Rate Limiter to this service to limit the number of HTTP requests.