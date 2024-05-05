import { requestI2CAccess } from "./node_modules/node-web-i2c/index.js";
import NPIX from "@chirimen/neopixel-i2c";
const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));

const neoPixels = 5; // LED個数
// main();
export async function LEDon() {
    const i2cAccess = await requestI2CAccess();
    const port = i2cAccess.ports.get(1);
    const npix = new NPIX(port, 0x41);
    await npix.init(neoPixels);
    await setledwhite(npix);
    await npix.setGlobal(0, 0, 0);
}

// パターンはRRGGBB の並びで
const LEDwhite = [
    [0xffffff, 0x000000, 0x000000, 0x000000, 0x000000],
    [0xffffff, 0xffffff, 0x000000, 0x000000, 0x000000],
    [0xffffff, 0xffffff, 0xffffff, 0x000000, 0x000000],
    [0xffffff, 0xffffff, 0xffffff, 0xffffff, 0x000000],
    [0xffffff, 0xffffff, 0xffffff, 0xffffff, 0xffffff],
    [0x000000, 0xffffff, 0xffffff, 0xffffff, 0xffffff],
    [0x000000, 0x000000, 0xffffff, 0xffffff, 0xffffff],
    [0x000000, 0x000000, 0x000000, 0xffffff, 0xffffff],
    [0x000000, 0x000000, 0x000000, 0x000000, 0xffffff],
    [0x000000, 0x000000, 0x000000, 0x000000, 0x000000]
];

async function setledwhite(npix) {
    for(let i=0;i<10;i++){
        await setPattern(npix, LEDwhite[i]);
        if(i == 4){ await sleep(5000);}
        else{await sleep(200);}
    }
}
async function setPattern(npix, pattern) {
    // パターン設定
    const grbArray = [];
    for (const color of pattern) {
        const r = color >> 16 & 0xff;
        const g = color >> 8 & 0xff;
        const b = color & 0xff;
        grbArray.push(g);
        grbArray.push(r);
        grbArray.push(b);
    }
    await npix.setPixels(grbArray);
}
