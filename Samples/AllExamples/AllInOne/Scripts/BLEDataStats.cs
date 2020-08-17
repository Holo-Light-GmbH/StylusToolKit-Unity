using HoloLight.STK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Examples.AllExamples
{
    public class BLEPaket
    {
        public long timeStamp;
        public string dataString;
        public long passedMs;
        public Vector3 Position;
        public Vector3 Rotation;
        public float distancePosition = 0;
        public long deltaMS = 0;
    }

    /// <summary>
    /// For debugging the Stylus 
    /// </summary>
    public class BLEDataStats : MonoBehaviour
    {
        [SerializeField]
        private HoloStylusManager _manager;

        private List<BLEPaket> _blePakets = new List<BLEPaket>();
        private Stopwatch _stopWatch;

        private bool newValue = false;
        [SerializeField]
        private TextMeshPro _textOutput;

        private void Start()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            _manager.Connector.RegisterDataCallback(OnStylusData);
        }

        private static string ByteToString(byte[] data)
        {
            string byteString = "";

            foreach (byte b in data)
            {
                byteString += b.ToString("X2") + "-";
            }

            return byteString;
        }

        private void OnStylusData(byte[] data)
        {
            StylusData stylusData;
            if (data[0] == 0xFE && _manager.DataParser.IsInitialized)
            {
                stylusData = _manager.DataParser.ParseFrame(data);

                if (data[13] == 0x00 && data[14] == 0x00 && data[15] == 0x00 && data[16] == 0x00 && data[17] == 0x00 && data[18] == 0x00)
                {
                    // Battery
                    NewStylusDataRecieved("BATTERY " + ByteToString(data), stylusData);
                    return;
                }
            }
            else
            {
                stylusData = new StylusData();
                stylusData.Position = Vector3.zero;
                if (data.Length == 0)
                {

                    NewStylusDataRecieved("EMPTY STREAM " + ByteToString(data), stylusData);
                    return;
                }
            }

            NewStylusDataRecieved(ByteToString(data), stylusData);
        }

        private void Update()
        {
            if (newValue)
            {
                newValue = false;

                float timeCaptured = (_blePakets[_blePakets.Count - 1].passedMs - _blePakets[0].passedMs) / 1000.0f;
                float frequency = _blePakets.Count / timeCaptured;
                _textOutput.text =  "Time captured: " + timeCaptured + "s\n"+
                                    "Frequency: " + frequency.ToString("F1") + "Hz\n" +
                                    "Interval: "+ _blePakets[_blePakets.Count - 1].deltaMS + "ms";
            }
        }

        public void NewStylusDataRecieved(string dataString, StylusData stylusData)
        {
            DateTime dateTime = DateTime.UtcNow.ToLocalTime();

            BLEPaket blePaket = new BLEPaket();

            blePaket.timeStamp = dateTime.Ticks;
            blePaket.dataString = dataString;
            blePaket.passedMs = _stopWatch.ElapsedMilliseconds;
            blePaket.Position = stylusData.Position;
            blePaket.Rotation = stylusData.Rotation;

            if (_blePakets.Count >= 1)
            {
                blePaket.deltaMS = blePaket.passedMs - _blePakets[_blePakets.Count - 1].passedMs;
                blePaket.distancePosition = Vector3.Distance(_blePakets[_blePakets.Count - 1].Position, blePaket.Position);
            }

            _blePakets.Add(blePaket);

            newValue = true;
        }

        public void Save()
        {
            if (_blePakets.Count <= 0)
            {
                UnityEngine.Debug.Log("Not enough BLE Packets to save data (" + _blePakets.Count + ")");
                return;
            }

            StringBuilder sb = new StringBuilder();
            DateTime dateTime = DateTime.UtcNow.ToLocalTime();
            string fullPath = Application.persistentDataPath + "\\" + "BLEData_" + dateTime.ToString("yy_MM_dd_HH_mm_ss_f") + ".csv";

            _stopWatch.Reset();
            _stopWatch.Restart();

            float timeCaptured = (_blePakets[_blePakets.Count - 1].passedMs - _blePakets[0].passedMs) / 1000.0f;
            float frequency = _blePakets.Count / timeCaptured;

            sb.AppendLine("Packets Count:;" + _blePakets.Count + ";Time measured(s):;" + timeCaptured + "s;Frequency[Hz]:;" + frequency.ToString("F1"));
            sb.AppendLine("PaketNr;Timestamp;Stopwatch Timestamp;Data[];Position;Rotation;PositionDistance;Interval [ms];Time passed [s]");

            for (int i = 0; i < _blePakets.Count; i++)
            {
                BLEPaket blePaket = _blePakets[i];
                string timeStamp = blePaket.timeStamp.ToString();
                string position = blePaket.Position.ToString("F3");

                sb.AppendLine(i + 1 + ";" + timeStamp + ";" + blePaket.passedMs + ";" + blePaket.dataString + ";" + position + ";" + blePaket.Rotation.ToString("F2") + ";" + blePaket.distancePosition + ";" + blePaket.deltaMS + ";" + ((float)(blePaket.passedMs/1000)).ToString("F2"));
            }

            System.IO.File.WriteAllText(fullPath, sb.ToString());

            _blePakets.Clear();
        }
    }
}