// Remote Example1 - reciever
// for CHIRIMEN with nodejs
import {requestGPIOAccess} from "./node_modules/node-web-gpio/dist/index.js";
const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));
import nodeWebSocketLib from "websocket"; // https://www.npmjs.com/package/websocket
import {RelayServer} from "./RelayServer.js"
import { pen_use} from './mpu6050.js';
import { LEDon } from './neopixel-i2c.js'
import {alartLED} from './alartLED.js'

var channel;
var gpioPort0;
var Ispenuse = false;

async function connect(){
    // GPIOポート0の初期化
    var gpioAccess = await requestGPIOAccess();
    var mbGpioPorts = gpioAccess.ports;
    gpioPort0 = mbGpioPorts.get(26);
    await gpioPort0.export("out"); //port0 out

    // webSocketリレーの初期化
    var relay = RelayServer("chirimentest", "chirimenSocket" , nodeWebSocketLib, "https://chirimen.org");
    channel = await relay.subscribe("tottori_f");
    console.log("web socketリレーサービスに接続しました");
    channel.onmessage = controlLED;
}

function controlLED(messge){
        console.log(messge.data);
        if (messge.data[0] == "pen" && messge.data[1] == 0 && messge.data[2] == "LEDon"){
            LEDon();
            console.log("ON");
            channel.send("LEDをオンにしました");
        } else if (messge.data[0] == "pen" && messge.data[1] == 0 && messge.data[2] == "LEDon"){
            console.log("OFF");
            channel.send("LEDをオフにしました");
        }
}

connect();
main();

async function main(){
    var useCount = 0;
    while(true){
        let penUse = await pen_use();
        console.log(penUse);

        if (useCount <=0 && penUse){
            useCount = 0;
            let penstatus = ["pen",0,true];
            var sendData = new data(penstatus)
            channel.send(sendData);
        }
        if (useCount >=0 && !(penUse)){
            useCount =0
        }

        if (penUse){useCount++;}
        else {useCount--;}

        if (useCount <= -6){
            alartLED();
            // stopを送る
            let penStatus = ["pen",0,false];
            var sendData = new data(penStatus)
            channel.send(sendData);
        }

        if (Ispenuse != penUse){
            Ispenuse = penUse;
        }
    }
}
