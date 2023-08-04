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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using nickmaltbie.ScreenManager;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Netcode.Transports.UTP.UnityTransport;

namespace nickmaltbie.OpenKCC.netcode.Common
{
    public class NetworkControls : MonoBehaviour
    {
        public const string ServerCommonName = "OpenKCCServer";

        public Button hostButton;
        public Button clientButton;
        public Button serverButton;
        public Button playOffline;
        public TMP_InputField serverAddress;
        public TMP_InputField serverPort;
        public TMP_Text debugMessage;

        public string onlineScreen;
        public string offlineScreen;

        private bool previousConnected;
        private Dictionary<string, CancellationTokenSource> dnsCancellationToken = new Dictionary<string, CancellationTokenSource>();
        private CancellationTokenSource previousCancellationToken;
        private IAsyncResult dnsLookupAction;

        public enum NMActionType
        {
            Host,
            Client,
            Server,
            Offline
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
            playOffline.onClick.AddListener(() => StartNetworkManager(NMActionType.Offline));

            var networkTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;

            serverAddress.onValueChanged.AddListener(address =>
            {
                if (IPAddress.TryParse(address, out _))
                {
                    ConnectionAddressData connectionData = networkTransport.ConnectionData;
                    connectionData.Address = address;
                    networkTransport.ConnectionData = connectionData;
                    debugMessage.text = $"Valid Server IP Address!";

                    if (dnsLookupAction != null && !dnsLookupAction.IsCompleted)
                    {
                        previousCancellationToken?.Cancel();
                    }

                    previousCancellationToken = null;
                    dnsLookupAction = null;
                }
                else
                {
                    debugMessage.text = $"Attempting to resolve hostname {address}";
                    if (dnsLookupAction != null && !dnsLookupAction.IsCompleted)
                    {
                        previousCancellationToken?.Cancel();
                    }

                    previousCancellationToken = dnsCancellationToken[address] = new CancellationTokenSource();
                    dnsLookupAction = Dns.BeginGetHostEntry(
                        address,
                        (IAsyncResult result) =>
                        {
                            if (!dnsCancellationToken.TryGetValue(address, out CancellationTokenSource token))
                            {
                                return;
                            }

                            bool cancellationRequested = token.IsCancellationRequested;
                            dnsCancellationToken.Remove(address);
                            token.Dispose();
                            if (cancellationRequested)
                            {
                                return;
                            }

                            IPHostEntry ie = Dns.EndGetHostEntry(result);
                            if (ie.AddressList.Length > 0)
                            {
                                debugMessage.text = $"Valid address {address} resolved to {ie.AddressList[0]}!";
                            }
                            else
                            {
                                debugMessage.text = $"Could not resolve hostname {address}";
                            }
                        },
                        $"Resolving host entry {address}");
                }
            });

            serverPort.onValueChanged.AddListener(port =>
            {
                if (ushort.TryParse(port, out ushort parsed))
                {
                    ConnectionAddressData connectionData = networkTransport.ConnectionData;
                    connectionData.Port = parsed;
                    networkTransport.ConnectionData = connectionData;
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

                }
            }

            previousConnected = connectedState;
        }

        public void StartNetworkManager(NMActionType action)
        {
            if (GetConnectedState())
            {
                debugMessage.text = $"Already setup as a client or server, please disconnect before trying again";
                return;
            }

            if (action == NMActionType.Offline)
            {
                // Override the hosting transport with an offline version
                NetworkTransport offlineTransport = NetworkManager.Singleton.gameObject.GetComponent<OfflineNetworkTransport>() ?? NetworkManager.Singleton.gameObject.AddComponent<OfflineNetworkTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = offlineTransport;
                NetworkManager.Singleton.StartHost();
                return;
            }
            else
            {
                UnityTransport networkTransport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>() ?? NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
                ConnectionAddressData connectionData = networkTransport.ConnectionData;
                connectionData.Address = serverAddress.text;
                connectionData.Port = ushort.Parse(serverPort.text);
                networkTransport.ConnectionData = connectionData;
            }

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
