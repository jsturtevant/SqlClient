// Generated by `wit-bindgen` 0.32.0. DO NOT EDIT!
// <auto-generated />
#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ImportsWorld.wit.imports.wasi.sockets.v0_2_1
{
    public static class InstanceNetworkInterop {

        internal static class InstanceNetworkWasmInterop
        {
            [DllImport("wasi:sockets/instance-network@0.2.1", EntryPoint = "instance-network"), WasmImportLinkage]
            internal static extern int wasmImportInstanceNetwork();

        }

        public  static unsafe global::ImportsWorld.wit.imports.wasi.sockets.v0_2_1.INetwork.Network InstanceNetwork()
        {
            var result =  InstanceNetworkWasmInterop.wasmImportInstanceNetwork();
            var resource = new global::ImportsWorld.wit.imports.wasi.sockets.v0_2_1.INetwork.Network(new global::ImportsWorld.wit.imports.wasi.sockets.v0_2_1.INetwork.Network.THandle(result));
            return resource;

            //TODO: free alloc handle (interopString) if exists
        }

    }
}
