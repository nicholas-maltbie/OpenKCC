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

using nickmaltbie.ScreenManager;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.netcode.Common
{
    public class DisconnectAction : MonoBehaviour
    {
        public Button disconnectButton;
        public GameObject offlineScreen;

        private enum NetworkState
        {
            Host,
            Server,
            Client,
            None
        }

        private NetworkState previousState;

        public void Update()
        {
            NetworkState state = NetworkState.None;
            if (NetworkManager.Singleton.IsHost)
            {
                disconnectButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Stop Host";
                state = NetworkState.Host;
            }
            else if (NetworkManager.Singleton.IsServer)
            {
                disconnectButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Stop Server";
                state = NetworkState.Server;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                disconnectButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Disconnect";
                state = NetworkState.Client;
            }

            disconnectButton.gameObject.SetActive(state != NetworkState.None);

            if (state != previousState)
            {
                disconnectButton.onClick.RemoveAllListeners();
                disconnectButton.onClick.AddListener(() =>
                {
                    NetworkManager.Singleton.Shutdown();
                    UIManager.Instance.SetScreen(offlineScreen.name);
                });
            }

            previousState = state;
        }
    }
}
