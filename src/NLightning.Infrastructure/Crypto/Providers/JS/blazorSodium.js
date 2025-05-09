import _sodium from 'libsodium-wrappers-sumo';
export const sodium = _sodium;

export async function init() {
    await sodium.ready;

    const usingWasm = typeof (sodium.libsodium.asm) === 'object'
        && typeof (sodium.libsodium.asm.__proto__) === 'undefined';

    console.log(`Sodium init: { version: ${sodium.sodium_version_string()}, wasm: ${usingWasm} }`);
}

export function getSodiumConstant(key) {
    return sodium[key];
}

export function printSodium() {
    console.log(sodium);
}