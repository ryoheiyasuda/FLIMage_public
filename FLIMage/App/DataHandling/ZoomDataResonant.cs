using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FLIMage;
using System.Reflection;
using System.Net.NetworkInformation;
using SharpCompress.Common;

namespace FLIMage
{
    internal class ZoomDataResonant
    {
        public ScanParameters State { get; set; }


        public ZoomDataResonant(ScanParameters Scan)
        {
            State = Scan;
            LoadJsonForZoomDelayPair();
        }

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private SortedDictionary<double, DelayData> DataDict = new SortedDictionary<double, DelayData>();
        private static double Qd(double z) =>
    Math.Round(z * 10d, MidpointRounding.AwayFromZero) / 10d;

        public class DelayData
        {
            public double zoom { get; set; }
            public double resonantScanDelay_us { get; set; }
            public double resonantEOMDelay_us { get; set; }
            public double LineClockDelay_us { get; set; }
            public double ScanDelay { get; set; }
            public double EOMDelay { get; set; }


            public DelayData Copy()
            {
                return (DelayData)this.MemberwiseClone();
            }
        }

        // ----- Static mapping (fields on Acquisition, properties on DelayData) -----
        private static readonly BindingFlags AcqFieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic; // include NonPublic if needed

        // Map all DelayData double props (except "zoom") -> Acquisition double fields with same name
        private static readonly (PropertyInfo DelayProp, FieldInfo AcqField)[] DelayPropToAcqField =
            typeof(DelayData).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(double) && p.Name != nameof(DelayData.zoom))
                .Select(p => new { p, f = typeof(ScanParameters.Acquisition).GetField(p.Name, AcqFieldFlags) })
                .Where(x => x.f != null && x.f.FieldType == typeof(double))
                .Select(x => (x.p, x.f))
                .ToArray();

        // Handle zoom separately (optional but explicit)
        private static readonly FieldInfo AcqZoomField =
            typeof(ScanParameters.Acquisition).GetField(nameof(DelayData.zoom), AcqFieldFlags);

        // ----- Copy functions -----

        // DelayData -> State.Acq (sets zoom too)
        public void CopyDelayToState(double zoom, DelayData delay)
        {
            //if (AcqZoomField != null) AcqZoomField.SetValue(State.Acq, zoom);

            for (int i = 0; i < DelayPropToAcqField.Length; i++)
            {
                var map = DelayPropToAcqField[i];
                var val = (double)map.DelayProp.GetValue(delay, null);
                map.AcqField.SetValue(State.Acq, val);
            }
        }

        // State.Acq -> DelayData (reads zoom too)
        public DelayData CopyStateToDelay()
        {
            var d = new DelayData();

            if (AcqZoomField != null)
                d.zoom = (double)AcqZoomField.GetValue(State.Acq);

            for (int i = 0; i < DelayPropToAcqField.Length; i++)
            {
                var map = DelayPropToAcqField[i];
                var val = (double)map.AcqField.GetValue(State.Acq);
                map.DelayProp.SetValue(d, val, null);
            }

            return d;
        }
        public void AddData(Double zoom, DelayData delay)
        {
            DataDict[Qd(zoom)] = delay?.Copy();
        }

        public String GetJsonFilePath()
        {
            var directory_name = State.Files.initFolderPath + Path.DirectorySeparatorChar + "ResonantSetting";
            var filename = "Resonant_Zoom_Data.json";
            var filepath = Path.Combine(directory_name, filename);
            if (!Directory.Exists(directory_name))
            {
                Directory.CreateDirectory(directory_name);
                File.SetAttributes(directory_name, FileAttributes.Normal);
            }
            return filepath;
        }

        private static readonly PropertyInfo[] DelayDoubleProps =
                typeof(DelayData).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(double))
                    .ToArray();

        private static DelayData LerpDelay(DelayData a, DelayData b, double t)
        {
            var r = new DelayData();
            foreach (var p in DelayDoubleProps)
            {
                double v0 = (double)p.GetValue(a);
                double v1 = (double)p.GetValue(b);
                p.SetValue(r, v0 + t * (v1 - v0));
            }
            return r;
        }

        public void GetDelayData()
        {
            var zoom = State.Acq.zoom;

            if (DataDict.Count == 0)
                return;

            // Exact match fast path
            if (DataDict.TryGetValue(zoom, out var exact))
            {
                CopyDelayToState(zoom, exact);
                return;
            }

            // We'll find the bounding keys without creating an array
            double? lowerKey = null;
            double? upperKey = null;

            foreach (var kvp in DataDict)
            {
                if (kvp.Key < zoom)
                {
                    lowerKey = kvp.Key;
                    continue;
                }
                upperKey = kvp.Key;
                break; // found the first key >= zoom
            }

            // Handle out-of-range zooms (clamp)
            if (lowerKey == null)
            {
                var first = DataDict.First();
                CopyDelayToState(first.Key, first.Value);
                return;
            }
            if (upperKey == null)
            {
                var last = DataDict.Last();
                CopyDelayToState(last.Key, last.Value);
                return;
            }

            // Interpolate between lowerKey and upperKey
            var d0 = DataDict[lowerKey.Value];
            var d1 = DataDict[upperKey.Value];
            double t = (zoom - lowerKey.Value) / (upperKey.Value - lowerKey.Value);

            var delay = LerpDelay(d0, d1, t);
            CopyDelayToState(zoom, delay);
        }

        public void SaveJsonForZoomDelayPair()
        {
            var data = CopyStateToDelay();
            AddData(State.Acq.zoom, data);

            var jsonString = JsonSerializer.Serialize(DataDict, JsonOpts);
            File.WriteAllText(GetJsonFilePath(), jsonString);
        }

        public void LoadJsonForZoomDelayPair()
        {
            var filepath = GetJsonFilePath();
            if (filepath != null && File.Exists(filepath))
            {
                try
                {
                    var json = File.ReadAllText(filepath);
                    DataDict = JsonSerializer.Deserialize<SortedDictionary<double, DelayData>>(json)
                               ?? new SortedDictionary<double, DelayData>();
                }
                catch
                {
                    DataDict = new SortedDictionary<double, DelayData>(); // fallback
                }
            }
        }
    }
}
