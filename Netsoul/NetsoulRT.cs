using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetsoulLib.Common;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace NetsoulLib
{
    public class NetsoulRT : Netsoul
    {
        #region Fields
        private StreamSocket socket;
        private DataReader streamReader;
        private DataWriter streamWriter;
        #endregion

        #region Properties

        #endregion

        public NetsoulRT()
            : base()
        {
            this.socket = new StreamSocket();
        }

        public async override Task<bool> Init()
        {
            await this.socket.ConnectAsync(new HostName(this.Server), this.ServerPort.ToString(), SocketProtectionLevel.PlainSocket);
            this.streamReader = new DataReader(this.socket.InputStream);
            this.streamReader.InputStreamOptions = InputStreamOptions.Partial;
            this.streamWriter = new DataWriter(this.socket.OutputStream);

            return true;
        }

        public async override Task<string> ReadData()
        {
            uint x = await this.streamReader.LoadAsync(1024);
            var buffer = this.streamReader.ReadString(this.streamReader.UnconsumedBufferLength);

            return buffer;
        }

        public override string CalcMD5(string str)
        {
            HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm("MD5");
            CryptographicHash Hasher = md5.CreateHash();
            IBuffer bufferMessage = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            Hasher.Append(bufferMessage);
            IBuffer NewMD5Buffer = Hasher.GetValueAndReset();
            var newMD5 = CryptographicBuffer.EncodeToHexString(NewMD5Buffer);
            return newMD5.ToLower();
        }

        public override async Task<bool> SendData(string str)
        {
            try
            {
                this.streamWriter.WriteString(str);
                await this.streamWriter.StoreAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
