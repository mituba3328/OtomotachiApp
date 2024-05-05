import {requestI2CAccess} from "./node_modules/node-web-i2c/index.js";
import MPU6050 from "@chirimen/mpu6050";
const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));

export async function pen_use() {
    // i2cのインスタンス作成
    var i2cAccess = await requestI2CAccess();
    // i2cのポートを指定(1)
    var port = i2cAccess.ports.get(1);
    var mpu6050 = new MPU6050(port, 0x68);
    await mpu6050.init();

    // 前回のデータを保存する
    var old_g = [0,0,0];
    var old_r = [0,0,0];
    var sum_dg = 0;
    var sum_dr = 0;
    for(let i=1; i<10;i++){
        const data = await mpu6050.readAll();
        const g = [data.gx, data.gy, data.gz];
        const r = [data.rx, data.ry, data.rz];
        let dg = [0,0,0];
        let dr = [0,0,0];
        g.forEach((g,index) => {
            dg[index] = Math.abs(g - old_g[index]);
        });
        r.forEach((r,index) => {
            dr[index] = Math.abs(r - old_r[index]);
        });

        sum_dr += dr[0] + dr[1] + dr[2];
        sum_dg += dg[0] + dg[1] + dg[2];
        old_g = g;
        old_r = r;
        await sleep(100);
    }
    if ( sum_dr > 750 ){
    return true;}
    else{return false;}
}
