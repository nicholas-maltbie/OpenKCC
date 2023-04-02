// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using Unity.Netcode;

namespace nickmaltbie.OpenKCC.netcode.Common
{
    public class OfflineNetworkTransport : NetworkTransport
    {
        public override ulong ServerClientId => 0;

        public override void DisconnectLocalClient()
        {

        }

        public override void DisconnectRemoteClient(ulong clientId)
        {

        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            return 0;
        }

        public override void Initialize(NetworkManager networkManager = null)
        {
            // do nothing... for now
        }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            clientId = default;
            payload = default;
            receiveTime = default;
            return NetworkEvent.Nothing;
        }

        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
        {
            // Do nothing...
        }

        public override void Shutdown()
        {
            // Do nothing...
        }

        public override bool StartClient()
        {
            return true;
        }

        public override bool StartServer()
        {
            return true;
        }
    }
}
