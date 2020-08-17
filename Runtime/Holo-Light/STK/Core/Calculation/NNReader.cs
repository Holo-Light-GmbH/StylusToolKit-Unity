using System.Collections.Generic;
using HoloLight.STK.Core.Tracker;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace HoloLight.STK.Core
{
    public class NNFHeaderPacket
    {
        public NNFHeaderPacket(byte[] headerInfoBytes)
        {
            this.Data = headerInfoBytes;

            byte[] binaryLengthByte = { headerInfoBytes[8], headerInfoBytes[7], headerInfoBytes[6], headerInfoBytes[5] };

            this.BinaryLength = BitConverter.ToInt32(binaryLengthByte, 0);

            byte[] dataPacketsLengthByte = { headerInfoBytes[13], headerInfoBytes[12], headerInfoBytes[11], headerInfoBytes[10] };

            this.DataPacketsLength = BitConverter.ToInt32(dataPacketsLengthByte, 0);
        }

        /// <summary>
        /// The Length of the ByteArray that actually represent the NeuralNetworkData in Binary Format
        /// </summary>
        public int BinaryLength { get; set; }
        /// <summary>
        /// The Length of Data Packets need to requested
        /// </summary>
        public int DataPacketsLength { get; set; }

        /// <summary>
        /// The Header Byte Array including the filling up bytes (e.g. ...88:88:88)
        /// </summary>
        public byte[] Data { get; set; }

        // in future, add a int that tells the length of data packets header...current is 9..  %TODO
    }

    public class NNFDataPacket
    {
        public NNFDataPacket(byte[] data)
        {
            byte[] packetNrByte = { data[8], data[7], data[6], data[5] };
            int packetNr = BitConverter.ToInt32(packetNrByte, 0);

            this.PacketNr = packetNr;

            byte[] actualData = new byte[NNReader.MAX_DATALENGTH - NNReader.DATAPACKET_HEADER_LENGTH];
            Buffer.BlockCopy(data, NNReader.DATAPACKET_HEADER_LENGTH, actualData, 0, actualData.Length);

            this.Data = actualData;
        }

        /// <summary>
        /// The Number of this Data Packet
        /// </summary>
        public int PacketNr { get; set; }

        /// <summary>
        /// The Byte Array without the Header
        /// </summary>
        public byte[] Data { get; set; }
    }

    public class NNReader
    {
        // ------------- CONSTANTS ------------- // 
        public const int MAX_DATALENGTH = 240;
        public const int DATAPACKET_HEADER_LENGTH = 9;
        private const int STARTING_ADDRESS = 327680; // Corresponding addresses 0x50000 = 327680, 0x60000 = 393216, 0X70000 = 458752

        private IConnection _connection;
        private NeuralNetworkData _neuralNetworkData;

        private int _startAddressInt;
        private List<NNFDataPacket> _dataPacketsList;
        private NNFHeaderPacket _nnfHeader;
        private List<int> _dataPacketNrs;

        public NNReader()
        {
        }

        public void Init(IConnection connection)
        {
            _connection = connection;
            InitVariables();
        }

        private void InitVariables()
        {
            if (_dataPacketsList == null)
            {
                _dataPacketsList = new List<NNFDataPacket>();
                _dataPacketNrs = new List<int>();
            }
            else
            {
                _dataPacketsList.Clear();
                _dataPacketNrs.Clear();
            }

            _startAddressInt = STARTING_ADDRESS;
            _nnfHeader = null;
        }

        public async Task<NeuralNetworkData> GetHMUData()
        {
            if (_neuralNetworkData == null)
            {
                _neuralNetworkData = new NeuralNetworkData();
            }

            await ReadNeuronalNetworkDataBinary();
            return _neuralNetworkData;
        }

        private void OnBinaryData(byte[] data)
        {
            if (_nnfHeader == null && data[0] == 0xAA && data[1] == 0xBB && data[2] == 0xCC && data[3] == 0xDD && data[4] == 0xEE)
            {
                _nnfHeader = new NNFHeaderPacket(data);
                // Debug.Log("Header Data Recieved: " + ByteArrayHelper.ByteToString(data));
            }

            if (_nnfHeader != null)
            {
                if (data.Length > 20)
                {
                    if (data[0] == 0xDA && data[1] == 0x7A && data[2] == 0xDA && data[3] == 0x7A && data[4] == 0x00)
                    {
                        NNFDataPacket dataPacket = new NNFDataPacket(data);

                        if (_dataPacketNrs.Contains(dataPacket.PacketNr))
                        {
                            // Debug.Log("WE ALREADY HAVE THIS DATA PACKET NR. " + dataPacket.PacketNr + " .\n" + ByteArrayHelper.ByteToString(data));
                            dataPacket = null;
                            return;
                        }
                        else
                        {
                            _dataPacketsList.Add(dataPacket);
                            _dataPacketNrs.Add(dataPacket.PacketNr);
                            // Debug.Log("NNF DATA PACKET NR. " + dataPacket.PacketNr + " packetNrList.Count " + _dataPacketNrs.Count + " (" + _dataPacketsList.Count + ")\n" + ByteArrayHelper.ByteToString(data));
                        }
                    }
                }
            }
        }

        private async Task ReadNeuronalNetworkDataBinary()
        {
            _connection.RegisterDataCallback(OnBinaryData);

            byte[] readHeader = { 0xFA, 0x99, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0 };
            ByteArrayHelper.OverwriteBytes(ref readHeader, ChangeAdressTo(_startAddressInt), 2, 4);
            _connection.SendData(readHeader);

            int stillTheSameValue = -1;

            while (_nnfHeader == null)
            {
                stillTheSameValue++;

                if (stillTheSameValue > 1)
                {
                    stillTheSameValue = 0;
                    _connection.SendData(readHeader);
                }

                await Task.Delay(50);
            }

            // Debug.Log("Header BinaryLength: " + _nnfHeader.BinaryLength);
            // Debug.Log("Header DataPacketsLength: " + _nnfHeader.DataPacketsLength);

            int requestDataLength = MAX_DATALENGTH;

            byte[] readCommand = { 0xFA, 0x99, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // read the rest

            ////////////////////// READING FULL ////////////////////////////////////////////////
            int startingAddress = _startAddressInt + MAX_DATALENGTH;
            requestDataLength = _nnfHeader.BinaryLength + _nnfHeader.DataPacketsLength * DATAPACKET_HEADER_LENGTH;
            if (requestDataLength % 240 != 0)
            {
                requestDataLength -= requestDataLength % 240;
                requestDataLength += 240;
            }
            ByteArrayHelper.OverwriteBytes(ref readCommand, ChangeAdressTo(startingAddress), 2, 4);
            ////////////////////// READING FULL ////////////////////////////////////////////////

            ByteArrayHelper.OverwriteBytes(ref readCommand, ByteConverter.ConvertIntToBytesLittleEndian(requestDataLength), 6, 4);

            _connection.SendData(readCommand);
            /*
            for (int i = 0; i < _nnfHeader.DataPacketsLength; i++)
            {
                RequestPacketByNr(i, readCommand);
            }
            */


            stillTheSameValue = -1;
            int lastLength = _dataPacketsList.Count;

            while (_dataPacketsList.Count < _nnfHeader.DataPacketsLength)
            {
                if (lastLength == _dataPacketsList.Count)
                {
                    stillTheSameValue++;
                }
                else
                {
                    stillTheSameValue = 0;
                }

                if (stillTheSameValue > 1)
                {
                    // if the length of the array is not changing 2 times in a row => request missing packets again

                    string packetListNrs = "";
                    ByteArrayHelper.OverwriteBytes(ref readCommand, ByteConverter.ConvertIntToBytesLittleEndian(MAX_DATALENGTH), 6, 4);
                    for (int i = 0; i < _nnfHeader.DataPacketsLength; i++)
                    {
                        bool alreadyGotIt = false;
                        for (int j = 0; j < _dataPacketNrs.Count; j++)
                        {
                            if (_dataPacketNrs[j] == i)
                            {
                                alreadyGotIt = true;
                                packetListNrs += i + ", ";
                                break;
                            }
                        }
                        if (alreadyGotIt) continue;

                        RequestPacketByNr(i, readCommand);
                    }
                    // Debug.Log("Packets Already Recieved : " + packetListNrs);
                    stillTheSameValue = 0;
                }

                lastLength = _dataPacketsList.Count;
                await Task.Delay(70);
            }
            
            // Debug.Log("_dataPacketsList.Count Finished: " + _dataPacketsList.Count);

            List<byte> finalNNFBinaryArray = new List<byte>();

            List<NNFDataPacket> newList = new List<NNFDataPacket>(_dataPacketsList);
            for (int i = 0; i < _dataPacketsList.Count; i++)
            {
                for (int j = 0; j < newList.Count; j++)
                {
                    if (newList[j].PacketNr == i)
                    {
                        finalNNFBinaryArray.AddRange(newList[j].Data);
                        break;
                    }
                }
            }

            byte[] nnfByte = finalNNFBinaryArray.ToArray();

            using (var rs = new MemoryStream(nnfByte))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    _neuralNetworkData = formatter.Deserialize(rs) as NeuralNetworkData;
                }
                catch (SerializationException e)
                {
                    Debug.Log("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    rs.Close();
                }
                _connection.UnRegisterDataCallback(OnBinaryData);
            }
        }

        public string GetInformation()
        {
            return "Packets recieved " + _dataPacketNrs.Count;
        }

        private void RequestPacketByNr(int packetNr, byte[] requestBytes)
        {
            int startingAddress = _startAddressInt + MAX_DATALENGTH + MAX_DATALENGTH * packetNr;
            ByteArrayHelper.OverwriteBytes(ref requestBytes, ChangeAdressTo(startingAddress), 2, 4);
            _connection.SendData(requestBytes);
        }

        private static byte[] ChangeAdressTo(int startingAdress)
        {
            int startAddressInInteger = startingAdress;// Corresponding addresses 0x50000 = 327680, 0x60000 = 393216, 0X70000 = 458752

            if (startAddressInInteger % MAX_DATALENGTH != 0)
            {
                startAddressInInteger -= startAddressInInteger % MAX_DATALENGTH;
                startAddressInInteger += MAX_DATALENGTH;
            }
            byte[] startAddressInBytes = ByteConverter.ConvertIntToBytesLittleEndian(startAddressInInteger);

            return startAddressInBytes;
        }

        internal class ByteArrayHelper
        {
            public static bool ByteArrayContains(byte[] target, byte[] searchArray)
            {
                return target.ToString().Contains(searchArray.ToString());
            }

            public static void OverwriteBytes(ref byte[] target, byte[] source, int start, int length)//overwrite length bytes from start in target with source
            {
                for (int i = 0; i < length; i++)
                {
                    target[i + start] = source[i];
                }
            }
            public static string ByteToString(byte[] data)
            {
                string testString = "";
                foreach (byte bbb in data)
                {
                    testString += bbb.ToString("X2") + ":";
                }

                return testString;
            }
        }
        internal class ByteConverter
        {
            public static byte[] ConvertIntToBytesLittleEndian(int source)
            {
                byte[] result = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    result[i] = (byte)(source >> (24 - 8 * i));
                }

                return result;
            }
            public static byte[] ConvertIntToBytesBigEndian(int source)
            {
                byte[] result = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    result[i] = (byte)(source >> (8 * i));
                }

                return result;
            }

            public static byte[] ConvertFloatToBytes(float source)
            {
                return BitConverter.GetBytes(source);
            }

            public static float ConvertBytestoFloat(byte[] value, int startIndex)
            {
                return BitConverter.ToSingle(value, startIndex);
            }
        }
    }
}