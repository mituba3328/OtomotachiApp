#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
#include <Adafruit_NeoPixel.h>

//---------------------------------------------------------
// Constants
//---------------------------------------------------------
#define OTOMO_SERVICE_UUID "73eb73e5-eec5-4bd2-8a53-2f059253ac96"
#define OTOMO_SERVICE_UUID "73eb73e5-eec5-4bd2-8a53-2f059253ac96"
#define OTOMO_ALERT_CHARACTERISIC_UUID "e877a7d3-c1eb-4ada-92cf-d15dfcb01cd9"
#define OTOMO_USING_CHARACTERISIC_UUID "c5479cc0-3854-487d-aefd-47c981617c55"
// #define OTOMO_ENABLE_CHARACTERISIC_UUID "2f801b84-9b8a-44b2-bd03-a208aede2320"

#define BLE_DEVICE_NAME "OTOMOPEN"

#define ALERTLED_PIN 27
#define CHEERLED_PIN 17//17
// 21 sda 22 scl
#define NUMPIXELS 5

Adafruit_NeoPixel pixels(NUMPIXELS, CHEERLED_PIN, NEO_GRB + NEO_KHZ800); 

#include<Wire.h>
#include <cstdlib> 
const int MPU_addr=0x68;  // I2C address of the MPU-6050

//---------------------------------------------------------
BLEServer *pServer = NULL;
BLECharacteristic *pCheerLedCharacteristic = NULL;
BLECharacteristic *pAlertLedCharacteristic = NULL;
BLECharacteristic *pIsUsingCharacteristic = NULL;
// BLECharacteristic *pIsEnableCharacteristic = NULL;
bool deviceConnected;
bool oldDeviceConnected;

bool CheerLEDOn = false;
bool AlertLEDOn = false;

void OnCheerLED(void * pvParameters)
{
  Serial.println(CheerLEDOn);
  pixels.begin();
  pixels.clear(); //NeoPixelの出力をリセット
  for (size_t i = 0; i < 5; i++){
    pixels.setPixelColor(i, pixels.Color(255,255,255)); //LEDの色を設定
    pixels.show();   //LEDに色を反映
    delay(500);
  }
  delay(3000);
  for (size_t i = 0; i < 5; i++){
    pixels.setPixelColor(i, pixels.Color(0,0,0)); //LEDの色を設定
    pixels.show();   //LEDに色を反映
    delay(500);
  }
  pixels.clear();
}

// Callbacks
class ServerCallbacks: public BLEServerCallbacks {
  void onConnect(BLEServer *pServer) {
    deviceConnected = true;
    Serial.println("onConnect");
  };
  void onDisconnect(BLEServer *pServer) {
    deviceConnected = false;
    Serial.println("onDisconnect");
  }
};

class CheerLEDCharacteristicCallbacks:public BLECharacteristicCallbacks {
  void onWrite(BLECharacteristic *pCharacteristic) {
      Serial.println("onWrite");
      std::string rxValue = pCharacteristic->getValue();
      if( rxValue.length() > 0 ){
        CheerLEDOn = rxValue[0]!=0;
        Serial.print("Received Value: ");
        for(int i=0; i<rxValue.length(); i++ ){
          Serial.print(rxValue[i],HEX);
        }
        Serial.println();
    };
    if (CheerLEDOn){
      xTaskCreatePinnedToCore(OnCheerLED, "OnCheerLED", 4096, NULL, 1, NULL, 0); //Core 0でタスク開始
    }
    
  };
};
class AlertLEDCharacteristicCallbacks: public BLECharacteristicCallbacks {
  void onWrite(BLECharacteristic *pCharacteristic) {
    Serial.println("onWrite");
  }
};

class IsUsingCharacteristicCallbacks: public BLECharacteristicCallbacks {};

// class IsEnableCharacteristicCallbacks: public BLECharacteristicCallbacks {
//   void onWrite(BLECharacteristic *pCharacteristic) {
//     Serial.println("onWrite");
//   }
// };

// func
void InitBLE(){
  BLEDevice::init(BLE_DEVICE_NAME);
    deviceConnected = false;
    oldDeviceConnected = false;
    // Server
    pServer = BLEDevice::createServer();
    pServer->setCallbacks(new ServerCallbacks());
    // Service
    BLEService *pService = pServer->createService(OTOMO_SERVICE_UUID);
    // Characteristic
    
    // CheerLED
    pCheerLedCharacteristic = pService->createCharacteristic(
        OTOMO_CHEERLED_CHARACTERISIC_UUID,
        BLECharacteristic::PROPERTY_WRITE
    );
    pCheerLedCharacteristic->setCallbacks(new CheerLEDCharacteristicCallbacks());
    pCheerLedCharacteristic->addDescriptor(new BLE2902());

    // AlertLED
    pAlertLedCharacteristic = pService->createCharacteristic(
        OTOMO_ALERT_CHARACTERISIC_UUID,
        BLECharacteristic::PROPERTY_NOTIFY
    );
    pAlertLedCharacteristic->setCallbacks(new AlertLEDCharacteristicCallbacks());
    pAlertLedCharacteristic->addDescriptor(new BLE2902());

    // IsUsing
    pIsUsingCharacteristic = pService->createCharacteristic(
        OTOMO_USING_CHARACTERISIC_UUID,
        BLECharacteristic::PROPERTY_NOTIFY
    );
    pIsUsingCharacteristic->setCallbacks(new IsUsingCharacteristicCallbacks());
    pIsUsingCharacteristic->addDescriptor(new BLE2902());

    // IsEnable
    // pIsEnableCharacteristic = pService->createCharacteristic(
    //     OTOMO_ENABLE_CHARACTERISIC_UUID,
    //     BLECharacteristic::PROPERTY_WRITE  |
    //     BLECharacteristic::PROPERTY_NOTIFY
    // );
    // pIsEnableCharacteristic->setCallbacks(new IsEnableCharacteristicCallbacks());
    // pIsEnableCharacteristic->addDescriptor(new BLE2902());
    
    pService->start();
    // Advertising
    BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(OTOMO_SERVICE_UUID);
    pAdvertising->setScanResponse(false);
    pAdvertising->setMinPreferred(0x0);
    BLEDevice::startAdvertising();
    Serial.println("startAdvertising");
}

void BLEOtomo_Process()
{
  // disconnecting
  if(!deviceConnected && oldDeviceConnected){
    delay(500); // give the bluetooth stack the chance to get things ready
    pServer->startAdvertising();
    Serial.println("restartAdvertising");
    oldDeviceConnected = deviceConnected;
  }
  // connecting
  if(deviceConnected && !oldDeviceConnected){
    oldDeviceConnected = deviceConnected;
  }
}

bool Mpu6050_Process()
{
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
  Wire.endTransmission(false);
  Wire.requestFrom(MPU_addr,14,true);  // request a total of 14 registers

  int Old_Ac[] = {
      Wire.read()<<8|Wire.read(),  // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)    
      Wire.read()<<8|Wire.read(),  // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
      Wire.read()<<8|Wire.read()  // 0x3F (ACCEL_ZOUT_H) & 0x40 (ACCEL_ZOUT_L)
    };
  int Old_Gy[] = {
      Wire.read()<<8|Wire.read(),  // 0x43 (GYRO_XOUT_H) & 0x44 (GYRO_XOUT_L)
      Wire.read()<<8|Wire.read(),  // 0x45 (GYRO_YOUT_H) & 0x46 (GYRO_YOUT_L)
      Wire.read()<<8|Wire.read()  // 0x47 (GYRO_ZOUT_H) & 0x48 (GYRO_ZOUT_L)
    };

  int Sum_Ac = 0;
  int Sum_Gy = 0;

  for (size_t i = 0; i < 10; i++)
  {
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr,14,true);  // request a total of 14 registers

    const int Ac[]={
      Wire.read()<<8|Wire.read(),  // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)    
      Wire.read()<<8|Wire.read(),  // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
      Wire.read()<<8|Wire.read()  // 0x3F (ACCEL_ZOUT_H) & 0x40 (ACCEL_ZOUT_L)
    };
    const int Gy[]={
      Wire.read()<<8|Wire.read(),  // 0x43 (GYRO_XOUT_H) & 0x44 (GYRO_XOUT_L)
      Wire.read()<<8|Wire.read(),  // 0x45 (GYRO_YOUT_H) & 0x46 (GYRO_YOUT_L)
      Wire.read()<<8|Wire.read()  // 0x47 (GYRO_ZOUT_H) & 0x48 (GYRO_ZOUT_L)
    };

    for (size_t i = 0; i < 3; i++){
      Sum_Ac += std::abs(Ac[i]-Old_Ac[i]);
    }
    for (size_t i = 0; i < 3; i++){
      Sum_Gy += std::abs(Gy[i]-Old_Gy[i]);
    }

    for (size_t i = 0; i < 3; i++) {Old_Ac[i] = Ac[i];}
    for (size_t i = 0; i < 3; i++) {Old_Ac[i] = Ac[i];}
    // Serial.println(Sum_Ac);
    // Serial.println(Sum_Gy);
    delay(100);

  }
  if (Sum_Ac > 100000){return true;}
  else{return false;}
}

// void LEDSubProcess(void * pvParameters)
// {
//   while (true){
//     if(CheerLEDOn){OnCheerLED();}
//   }
// }

void setup() {
  pinMode(ALERTLED_PIN, OUTPUT);
  xTaskCreatePinnedToCore(OnCheerLED, "OnCheerLED", 4096, NULL, 1, NULL, 0);
  Wire.begin();
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B);  // PWR_MGMT_1 register
  Wire.write(0);     // set to zero (wakes up the MPU-6050)
  Wire.endTransmission(true);

  Serial.begin(115200);
  Serial.println("Service Starting....");
  InitBLE();
}

//---------------------------------------------------------
int UseCount=0;

void loop() {
  BLEOtomo_Process();
  if( deviceConnected ){
    bool PenUsing=Mpu6050_Process();
    if (UseCount<=0 && PenUsing){
      UseCount=0;
      
      // Notify
      Serial.println("Send:Using");
      int SendData = 1;
      pIsUsingCharacteristic->setValue(SendData);
      pIsUsingCharacteristic->notify();
    }
    if (UseCount >=0 && !(PenUsing)){
        UseCount =0;
    }
    
    if (PenUsing){UseCount++;}
    else {UseCount--;}

    if (UseCount <= -6){
      // Notify
      Serial.println("Send:Not Using");
      int SendData = 0;
      pIsUsingCharacteristic->setValue(SendData);
      pIsUsingCharacteristic->notify();

      for (size_t i = 0; i < 10; i++) {
        digitalWrite(ALERTLED_PIN,HIGH);
        if(Mpu6050_Process()){
          digitalWrite(ALERTLED_PIN,LOW);
          break;
        }
        Serial.println("Send:Not Using");
          int SendData = 0;
          pIsUsingCharacteristic->setValue(SendData);
          pIsUsingCharacteristic->notify();
        digitalWrite(ALERTLED_PIN,LOW);
        if(Mpu6050_Process()){
          digitalWrite(ALERTLED_PIN,LOW);
          Serial.println("Send:Not Using");
          break;
        }
      }
    }
  }
}


