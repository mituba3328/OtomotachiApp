// Remote Example4 - controller
import nodeWebSocketLib from "websocket";
import {RelayServer} from "./RelayServer.js";
import * as fs from 'fs' // 読み込む
var relay = RelayServer("achex", "chirimenSocket" , nodeWebSocketLib, "https://chirimen.org");

var channel;
var pen0;
var pen1;
var penOff;
main();

async function main(){
	// webSocketリレーの初期化
	var relay = RelayServer("chirimentest", "chirimenSocket" );
	channel = await relay.subscribe("tottori_f");
    console.log("Relay Server Connected!!");
	channel.onmessage = getMessage;

    
}

function getMessage(msg){ // メッセージを受信したときに起動する関数
    switch (msg.data[0]) {
        case "pen":
            if(msg.data[1] == 0){
                pen0 = msg.data[2];
            }else if(msg.data[1] == 1){
                pen1 = msg.data[2];
            }
            break;
        case "glass":
            fs.writeFile('../../sendMsgSleep.tmp','sleep',() =>{});
            break;
        case "ita":
            console.log(msg.data[1] / 1000);
            fs.writeFile('../../sendMsgWork.tmp',String(Math.round(msg.data[1] / 1000)),() =>{});
            break;
    }
}

function onLed(){ // LED を点灯する
    let data0 = ["pen",0,"LEDon"]
    channel.send(data0);
    let data1 = ["pen",1,"LEDon"]
    channel.send(data1);
}

