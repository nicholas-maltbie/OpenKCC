// Copyright (C) 2022 Nicholas Maltbie
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

using System.Net;
using Netcode.Transports.WebSocket;
using nickmaltbie.ScreenManager;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.NetworkStateMachineUnity.ExampleAnim
{
    public class NetworkControls : MonoBehaviour
    {
        public Button hostButton;
        public Button clientButton;
        public Button serverButton;
        public TMP_InputField serverAddress;
        public TMP_InputField serverPort;
        public TMP_Text debugMessage;

        public string onlineScreen;
        public string offlineScreen;

        private bool previousConnected;

        public enum NMActionType
        {
            Host,
            Client,
            Server
        };

        public bool GetConnectedState()
        {
            return NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        }

        public void Awake()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                hostButton.gameObject.SetActive(false);
                serverButton.gameObject.SetActive(false);
            }

            hostButton.onClick.AddListener(() => StartNetworkManager(NMActionType.Host));
            clientButton.onClick.AddListener(() => StartNetworkManager(NMActionType.Client));
            serverButton.onClick.AddListener(() => StartNetworkManager(NMActionType.Server));

            var networkTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as WebSocketTransport;

            serverAddress.onValueChanged.AddListener(address =>
            {
                if (IPAddress.TryParse(address, out _))
                {
                    networkTransport.ConnectAddress = address;
                    debugMessage.text = $"Valid Server IP Address!";
                }
                else
                {
                    debugMessage.text = $"Invalid IP Address string: {address}";
                }
            });
            serverPort.onValueChanged.AddListener(port =>
            {
                if (ushort.TryParse(port, out ushort parsed))
                {
                    networkTransport.Port = parsed;
                    debugMessage.text = $"Valid Server Port!";
                }
                else
                {
                    debugMessage.text = $"Invalid port string: {port}";
                }
            });

            NetworkManager.Singleton.OnClientDisconnectCallback += id =>
            {
                if (id == NetworkManager.Singleton.LocalClientId || id == 0)
                {
                    UIManager.Instance.SetScreen(offlineScreen);
                }
            };
        }

        public void Update()
        {
            bool connectedState = GetConnectedState();
            if (connectedState != previousConnected)
            {
                if (connectedState)
                {
                    UIManager.Instance.SetScreen(onlineScreen);
                }
                else
                {
                    NetworkManager.Singleton.GetComponent<NetworkTransport>().Initialize(NetworkManager.Singleton);
                }
            }

            previousConnected = connectedState;
        }

        public bool ValidIPAddressAndPort()
        {
            bool validPort = ushort.TryParse(serverPort.text, out ushort _);
            bool validAddress = IPAddress.TryParse(serverAddress.text, out _);
            return validPort && validAddress;
        }

        public void StartNetworkManager(NMActionType action)
        {
            if (!ValidIPAddressAndPort())
            {
                debugMessage.text = $"Won't start server, invalid address \"{serverAddress.text}\" or port \"{serverPort.text}\"";
                return;
            }

            if (GetConnectedState())
            {
                debugMessage.text = $"Already setup as a client or server, please disconnect before trying again";
                return;
            }

            var networkTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as WebSocketTransport;
            networkTransport.ConnectAddress = serverAddress.text;
            networkTransport.Port = ushort.Parse(serverPort.text);

            switch (action)
            {
                case NMActionType.Host:
                    NetworkManager.Singleton.StartHost();
                    break;
                case NMActionType.Client:
                    NetworkManager.Singleton.StartClient();
                    break;
                case NMActionType.Server:
                    NetworkManager.Singleton.StartServer();
                    break;
            }
        }
    }
}
