using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetsoulLib.Common;

namespace NetsoulLib.Desktop
{
    public class NetsoulDesktop : Netsoul
    {
        private Socket socket;
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static string response;

        public NetsoulDesktop()
            : base()
        {
        }

        public async override Task<bool> Init()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await Task.Run(() =>
            {
                this.socket.BeginConnect(this.Server, this.ServerPort, r =>
                {
                    Socket client = (Socket)r.AsyncState;
                    client.EndConnect(r);
                    connectDone.Set();
                }, this.socket);
                connectDone.WaitOne();
            });

            return true;
        }

        #region Send
        public override async Task<bool> SendData(string str)
        {
            try
            {
                await Task.Run(() =>
                {
                    Send(this.socket, str);
                    sendDone.Set();
                });
                sendDone.WaitOne();
                sendDone.Reset();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Receive

        public async override Task<string> ReadData()
        {
            try
            {
                await Task.Run(() =>
                {
                    Receive(this.socket);
                });
                receiveDone.WaitOne();
                receiveDone.Reset();
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    response = state.sb.ToString();
                    receiveDone.Set();

                    //// Get the rest of the data.
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    //    new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
        #endregion

        public override string CalcMD5(string str)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();

            //HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm("MD5");
            //CryptographicHash Hasher = md5.CreateHash();
            //IBuffer bufferMessage = CryptographicBuffer.ConvertStringToBinary(info.RandomMD5
            //    + "-" + info.ClientIp + "/" + info.ClientPort.ToString()
            //    + this.Password, BinaryStringEncoding.Utf8);
            //Hasher.Append(bufferMessage);
            //IBuffer NewMD5Buffer = Hasher.GetValueAndReset();
            //var newMD5 = CryptographicBuffer.EncodeToHexString(NewMD5Buffer);
            //return newMD5.ToLower();

            //return "";
        }
    }
}
