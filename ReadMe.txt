The server needs to manage a list of users and cars. Users can rent cars.

User Data:

Name (string)
Password (string)
The user data is stored in a users.txt file. Each user's information is on a single line in the format: name|password. When the server starts, it should read the data from this file. (If reading from the file fails, the server should initialize with constant data.)

The car data is stored in a list.txt file, with each car's information on a single line in the format: license_plate;type;brand;mileage. When the server starts, it should read the content of this file. (If reading from the file fails, the server should initialize the list with constant data.)

The server should support the following functions:

LISTA: List all cars, available without login.
LOGIN|<name>|<pass>: Logs in the specified user.
LOGOUT: Logs out the currently logged-in client.
KOLCSONZES|<license_plate>: Rents the car with the specified license plate. The server must check that the car is not currently rented, and this function is only available after login.
VISSZAVISZ|<license_plate>|<km>: Indicates that the car with the specified license plate is being returned. This function is only available after login. The new mileage must be recorded and cannot be smaller than the previous value.
EXIT: Ends communication and the client disconnects.
Both the client and server code must be implemented. The messages should be transmitted through streams. The server should be able to handle communication with multiple clients simultaneously.

The server's IP address and port should be specified in an app.conf file.