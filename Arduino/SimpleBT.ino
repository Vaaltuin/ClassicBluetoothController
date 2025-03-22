/*
    W Breytenbach
    2025/03/22
    Simple program to do some Classic Bluetooth communication functions.

    It communicates through the serial port to a HC-05 Bluetooth module.
    It will start to periodically send a string consisting of the Msg and the string representation of cnt.

    It checks for reeived data, and upon success it sets Msg to what was received.

    It uses a simple message structure where the first byte has bit 8 set to one, and bits 7 downto 0 specifying 
    the number of ASCII characters to receive. This way the message byte is easy to recongnize.  
*/

  int cnt = 0;
  String Msg = "Pete";
  
// the setup function runs once when you press reset or power the board
void setup() {
  // initialize digital pin LED_BUILTIN as an output.
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.begin(38400);
  Serial.setTimeout(10);
  Serial.println("Ready");
}

// the loop function runs over and over again forever
void loop() {

  if (Serial.available() > 0){
    byte data = 0;

    // ignore all but the length byte
    while (data < 0x80){
       data = Serial.read();
    }

    digitalWrite(LED_BUILTIN, HIGH);
    // for now a simple read string does the trick to get the message
    Msg = Serial.readString();
    Msg.trim();

    cnt = 0;
  }
  else {
    // Send a message to the phone
    String Number = String(cnt++);
    String reply = String(Msg + Number);
    int len = reply.length();
    Serial.write(len + 0x80);
    Serial.print(reply);

    delay(2000);  // don't do it too often.
  }
}
