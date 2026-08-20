using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FLIMage
{
    /// <summary>
    /// Native reader for PicoQuant unified TTTR (PTU) T3 image files.
    /// </summary>
    internal static class PtuFileReader
    {
        private const uint PicoHarpT3 = 0x00010303;
        private const uint HydraHarpT3 = 0x00010304;
        private const uint HydraHarp2T3 = 0x01010304;
        private const uint TimeHarp260NT3 = 0x00010305;
        private const uint TimeHarp260PT3 = 0x00010306;
        private const uint GenericT3 = 0x00010307;

        private const uint TyEmpty8 = 0xFFFF0008;
        private const uint TyBool8 = 0x00000008;
        private const uint TyInt8 = 0x10000008;
        private const uint TyBitSet64 = 0x11000008;
        private const uint TyColor8 = 0x12000008;
        private const uint TyFloat8 = 0x20000008;
        private const uint TyTDateTime = 0x21000008;
        private const uint TyFloat8Array = 0x2001FFFF;
        private const uint TyAnsiString = 0x4001FFFF;
        private const uint TyWideString = 0x4002FFFF;
        private const uint TyBinaryBlob = 0xFFFFFFFF;

        private static readonly object CacheLock = new object();
        private static string cachedPath;
        private static long cachedLength;
        private static DateTime cachedWriteTimeUtc;
        private static Metadata cachedMetadata;

        public static int SetupOpening(string fileName, out string header)
        {
            Metadata metadata = GetMetadata(fileName);
            header = BuildFlimHeader(metadata, fileName);
            return metadata.FrameCount;
        }

        public static FileIO.FileError OpenPage(
            string fileName,
            long readPage,
            int intoPage,
            FLIMData flim,
            bool newFile,
            bool savePagesInMemory)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return FileIO.FileError.NotFound;

            try
            {
                Metadata metadata = GetMetadata(fileName);
                if (readPage < 0 || readPage >= metadata.FrameCount)
                    return FileIO.FileError.FormatError;

                double[] savedOffset = flim.offset == null ? null : (double[])flim.offset.Clone();
                if (newFile && readPage == 0)
                {
                    flim.State = new ScanParameters();
                    flim.decodeHeader(BuildFlimHeader(metadata, fileName), fileName);
                    flim.InitializeData(flim.State, true);
                }

                ushort[][,,] page = DecodePage(fileName, metadata, (int)readPage);
                DateTime acquiredTime = metadata.AcquiredTime.AddSeconds(readPage * metadata.FrameTimeSeconds);

                if (newFile)
                {
                    flim.clearMemory();
                    flim.n_pages = metadata.FrameCount;
                }

                flim.KeepPagesInMemory = savePagesInMemory;
                flim.imagesPerFile = 1;

                if (savePagesInMemory)
                {
                    flim.PutToPage(page, acquiredTime, intoPage);
                }
                else
                {
                    flim.LoadFLIMRawFromData4D(page, acquiredTime, false);
                    flim.currentPage = (int)readPage;
                    flim.expandPage(intoPage + 1);
                    if (flim.acquiredTime_Pages != null && intoPage < flim.acquiredTime_Pages.Length)
                        flim.acquiredTime_Pages[intoPage] = acquiredTime;
                }

                string filePath;
                string fileBaseName;
                int fileNumber;
                FileIO.FileParts(fileName, out filePath, out fileBaseName, out fileNumber);
                string shortName = Path.GetFileName(fileName);

                flim.State.Files.pathName = filePath;
                flim.pathName = filePath;
                flim.baseName = fileNumber >= 0 ? fileBaseName : Path.GetFileNameWithoutExtension(fileName);
                flim.fileCounter = Math.Max(0, fileNumber);
                flim.numberedFile = fileNumber >= 0;
                flim.fileName = shortName;
                flim.fileExtension = ".ptu";
                flim.State.Files.extension = ".ptu";
                flim.fullFileName = fileName;

                if (savedOffset != null && flim.offset != null)
                {
                    for (int i = 0; i < flim.offset.Length && i < savedOffset.Length; i++)
                        flim.offset[i] = savedOffset[i];
                    flim.State.Spc.analysis.offset = (double[])flim.offset.Clone();
                }

                return FileIO.FileError.Success;
            }
            catch (FileNotFoundException)
            {
                return FileIO.FileError.NotFound;
            }
            catch (InvalidDataException)
            {
                return FileIO.FileError.FormatError;
            }
            catch (NotSupportedException)
            {
                return FileIO.FileError.FormatError;
            }
            catch
            {
                return FileIO.FileError.UnKnown;
            }
        }

        private static Metadata GetMetadata(string fileName)
        {
            string fullPath = Path.GetFullPath(fileName);
            var info = new FileInfo(fullPath);
            if (!info.Exists)
                throw new FileNotFoundException("PTU file was not found.", fullPath);

            lock (CacheLock)
            {
                if (cachedMetadata != null
                    && string.Equals(cachedPath, fullPath, StringComparison.OrdinalIgnoreCase)
                    && cachedLength == info.Length
                    && cachedWriteTimeUtc == info.LastWriteTimeUtc)
                {
                    return cachedMetadata;
                }
            }

            Metadata metadata = ReadMetadata(fullPath);
            lock (CacheLock)
            {
                cachedPath = fullPath;
                cachedLength = info.Length;
                cachedWriteTimeUtc = info.LastWriteTimeUtc;
                cachedMetadata = metadata;
            }
            return metadata;
        }

        private static Metadata ReadMetadata(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                Dictionary<string, object> tags = ReadHeader(reader);
                var metadata = new Metadata
                {
                    RecordOffset = stream.Position,
                    RecordCount = GetInt64(tags, "TTResult_NumberOfRecords", -1),
                    RecordType = (uint)GetInt64(tags, "TTResultFormat_TTTRRecType", 0),
                    Width = GetInt32(tags, "ImgHdr_PixX", 0),
                    Height = GetInt32(tags, "ImgHdr_PixY", 0),
                    LineStartMask = MarkerMask(tags, "ImgHdr_LineStart"),
                    LineStopMask = MarkerMask(tags, "ImgHdr_LineStop"),
                    FrameMask = MarkerMask(tags, "ImgHdr_Frame"),
                    Bidirectional = GetInt32(tags, "ImgHdr_BiDirect", 0) != 0,
                    Sinusoidal = GetInt32(tags, "ImgHdr_SinCorrection", 0) != 0,
                    TcspcResolutionSeconds = GetDouble(tags, "MeasDesc_Resolution", 0),
                    GlobalResolutionSeconds = GetDouble(tags, "MeasDesc_GlobalResolution", 0),
                    SyncRate = GetInt32(tags, "TTResult_SyncRate", 0),
                    AcquiredTime = File.GetCreationTime(fileName)
                };

                ValidateMetadata(metadata, tags, stream.Length);
                ScanRecords(reader, metadata);

                if (metadata.FrameCount < 1 || metadata.ChannelCount < 1 || metadata.BinCount < 1)
                    throw new InvalidDataException("PTU file does not contain a complete T3 image.");
                if (metadata.ChannelCount > 2)
                    throw new NotSupportedException("FLIMage PTU import currently supports up to two active channels.");

                metadata.LineTime = metadata.ValidLineCount > 0
                    ? (long)Math.Round((double)metadata.TotalLineTime / metadata.ValidLineCount)
                    : 0;
                metadata.PixelTime = metadata.Width > 0
                    ? Math.Max(1, (long)Math.Round((double)metadata.LineTime / metadata.Width))
                    : 0;
                metadata.FrameTimeSeconds = metadata.FrameCount > 0
                    ? metadata.TotalAcquisitionTime * metadata.GlobalResolutionSeconds / metadata.FrameCount
                    : 0;
                return metadata;
            }
        }

        private static Dictionary<string, object> ReadHeader(BinaryReader reader)
        {
            string magic = Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0');
            reader.ReadBytes(8); // version
            if (!string.Equals(magic, "PQTTTR", StringComparison.Ordinal))
                throw new InvalidDataException("File is not a PicoQuant PTU file.");

            var tags = new Dictionary<string, object>(StringComparer.Ordinal);
            while (true)
            {
                byte[] identBytes = reader.ReadBytes(32);
                if (identBytes.Length != 32)
                    throw new EndOfStreamException("Incomplete PTU header.");

                string ident = Encoding.ASCII.GetString(identBytes).TrimEnd('\0');
                int index = reader.ReadInt32();
                uint type = reader.ReadUInt32();
                long value = reader.ReadInt64();
                object decoded;

                switch (type)
                {
                    case TyEmpty8:
                        decoded = null;
                        break;
                    case TyBool8:
                        decoded = value != 0;
                        break;
                    case TyInt8:
                    case TyBitSet64:
                    case TyColor8:
                        decoded = value;
                        break;
                    case TyFloat8:
                    case TyTDateTime:
                        decoded = BitConverter.Int64BitsToDouble(value);
                        break;
                    case TyAnsiString:
                        decoded = Encoding.UTF8.GetString(ReadPayload(reader, value)).TrimEnd('\0');
                        break;
                    case TyWideString:
                        decoded = Encoding.Unicode.GetString(ReadPayload(reader, value)).TrimEnd('\0');
                        break;
                    case TyFloat8Array:
                    case TyBinaryBlob:
                        ReadPayload(reader, value);
                        decoded = null;
                        break;
                    default:
                        throw new InvalidDataException("Unsupported PTU tag type: 0x" + type.ToString("X8"));
                }

                if (!tags.ContainsKey(ident) || index <= 0)
                    tags[ident] = decoded;
                if (string.Equals(ident, "Header_End", StringComparison.Ordinal))
                    break;
            }
            return tags;
        }

        private static byte[] ReadPayload(BinaryReader reader, long length)
        {
            if (length < 0 || length > int.MaxValue || reader.BaseStream.Position + length > reader.BaseStream.Length)
                throw new InvalidDataException("Invalid PTU tag payload length.");
            byte[] result = reader.ReadBytes((int)length);
            if (result.Length != (int)length)
                throw new EndOfStreamException("Incomplete PTU tag payload.");
            return result;
        }

        private static void ValidateMetadata(Metadata metadata, Dictionary<string, object> tags, long fileLength)
        {
            if (GetInt32(tags, "Measurement_Mode", 0) != 3
                || GetInt32(tags, "Measurement_SubMode", 0) != 3)
                throw new NotSupportedException("Only PTU T3 image measurements are supported.");
            if (!IsSupportedT3(metadata.RecordType))
                throw new NotSupportedException("Unsupported PTU T3 record type: 0x" + metadata.RecordType.ToString("X8"));
            if (metadata.Width < 1 || metadata.Height < 1)
                throw new InvalidDataException("PTU image dimensions are missing.");
            if (metadata.LineStartMask == 0 || metadata.LineStopMask == 0
                || metadata.LineStartMask == metadata.LineStopMask)
                throw new InvalidDataException("PTU line marker definitions are invalid.");
            if (metadata.Sinusoidal)
                throw new NotSupportedException("Sinusoidal PTU image scans are not supported.");
            if (metadata.TcspcResolutionSeconds <= 0 || metadata.GlobalResolutionSeconds <= 0)
                throw new InvalidDataException("PTU timing resolution is missing.");

            long availableRecords = (fileLength - metadata.RecordOffset) / 4;
            if (metadata.RecordCount <= 0)
                metadata.RecordCount = availableRecords;
            if (metadata.RecordCount > availableRecords)
                throw new InvalidDataException("PTU record count exceeds the file size.");
        }

        private static void ScanRecords(BinaryReader reader, Metadata metadata)
        {
            reader.BaseStream.Position = metadata.RecordOffset;
            ulong overflow = 0;
            ulong lineStart = ulong.MaxValue;
            int y = -1;
            int frame = -1;
            int minChannel = int.MaxValue;
            int maxChannel = -1;
            int maxBin = -1;
            long photons = 0;
            ulong firstTime = ulong.MaxValue;
            ulong lastTime = 0;

            for (long i = 0; i < metadata.RecordCount; i++)
            {
                Record record = DecodeRecord(reader.ReadUInt32(), metadata.RecordType, ref overflow);
                ulong globalTime = overflow + record.Time;
                if (globalTime < firstTime)
                    firstTime = globalTime;
                if (globalTime > lastTime)
                    lastTime = globalTime;

                if (record.Kind == RecordKind.Photon)
                {
                    minChannel = Math.Min(minChannel, record.Channel);
                    maxChannel = Math.Max(maxChannel, record.Channel);
                    maxBin = Math.Max(maxBin, record.DTime);
                    photons++;
                    continue;
                }
                if (record.Kind != RecordKind.Marker)
                    continue;

                if ((record.Marker & metadata.FrameMask) != 0)
                {
                    lineStart = ulong.MaxValue;
                    y = -1;
                }
                if ((record.Marker & metadata.LineStopMask) != 0)
                {
                    if (lineStart != ulong.MaxValue && y >= 0 && y < metadata.Height && globalTime >= lineStart)
                    {
                        metadata.TotalLineTime += globalTime - lineStart;
                        metadata.ValidLineCount++;
                    }
                    lineStart = ulong.MaxValue;
                }
                if ((record.Marker & metadata.LineStartMask) != 0)
                {
                    y++;
                    if (y == 0)
                        frame++;
                    if (y < metadata.Height)
                        lineStart = globalTime;
                }
            }

            metadata.FrameCount = frame + 1;
            metadata.FirstChannel = minChannel == int.MaxValue ? 0 : minChannel;
            metadata.ChannelCount = maxChannel < metadata.FirstChannel ? 0 : maxChannel - metadata.FirstChannel + 1;
            metadata.BinCount = maxBin + 1;
            metadata.PhotonCount = photons;
            metadata.TotalAcquisitionTime = firstTime == ulong.MaxValue || lastTime < firstTime ? 0 : lastTime - firstTime;
        }

        private static ushort[][,,] DecodePage(string fileName, Metadata metadata, int requestedFrame)
        {
            var page = new ushort[metadata.ChannelCount][,,];
            for (int channel = 0; channel < page.Length; channel++)
                page[channel] = new ushort[metadata.Height, metadata.Width, metadata.BinCount];

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                stream.Position = metadata.RecordOffset;
                ulong overflow = 0;
                ulong lineStart = ulong.MaxValue;
                int y = -1;
                int frame = -1;

                for (long i = 0; i < metadata.RecordCount; i++)
                {
                    Record record = DecodeRecord(reader.ReadUInt32(), metadata.RecordType, ref overflow);
                    ulong globalTime = overflow + record.Time;

                    if (record.Kind == RecordKind.Photon)
                    {
                        if (frame != requestedFrame || lineStart == ulong.MaxValue || y < 0 || y >= metadata.Height)
                            continue;
                        int channel = record.Channel - metadata.FirstChannel;
                        if (channel < 0 || channel >= metadata.ChannelCount || record.DTime >= metadata.BinCount)
                            continue;

                        long elapsed = (long)(globalTime - lineStart);
                        if (elapsed < 0 || elapsed >= metadata.LineTime)
                            continue;
                        if (metadata.Bidirectional && (y & 1) != 0)
                            elapsed = metadata.LineTime - 1 - elapsed;
                        int x = (int)(elapsed / metadata.PixelTime);
                        if (x < 0 || x >= metadata.Width)
                            continue;

                        ushort count = page[channel][y, x, record.DTime];
                        if (count < ushort.MaxValue)
                            page[channel][y, x, record.DTime] = (ushort)(count + 1);
                        continue;
                    }
                    if (record.Kind != RecordKind.Marker)
                        continue;

                    if ((record.Marker & metadata.FrameMask) != 0)
                    {
                        lineStart = ulong.MaxValue;
                        y = -1;
                    }
                    if ((record.Marker & metadata.LineStopMask) != 0)
                        lineStart = ulong.MaxValue;
                    if ((record.Marker & metadata.LineStartMask) != 0)
                    {
                        y++;
                        if (y == 0)
                        {
                            frame++;
                            if (frame > requestedFrame)
                                break;
                        }
                        lineStart = frame == requestedFrame && y < metadata.Height ? globalTime : ulong.MaxValue;
                    }
                }
            }
            return page;
        }

        private static Record DecodeRecord(uint value, uint recordType, ref ulong overflow)
        {
            if (recordType == PicoHarpT3)
            {
                uint time = value & 0xFFFF;
                int dtime = (int)((value >> 16) & 0xFFF);
                int channel = (int)((value >> 28) & 0xF);
                if (channel != 0xF)
                    return new Record(RecordKind.Photon, time, dtime, channel >= 1 && channel <= 4 ? channel - 1 : 4, 0);
                if (dtime == 0)
                {
                    overflow += 65536;
                    return new Record(RecordKind.Overflow, time, 0, 0, 0);
                }
                return new Record(RecordKind.Marker, time, 0, 0, dtime);
            }

            uint nsync = value & 0x3FF;
            int fineTime = (int)((value >> 10) & 0x7FFF);
            bool special = ((value >> 31) & 1) != 0;
            int code = (int)((value >> 25) & 0x3F);
            if (!special)
                return new Record(RecordKind.Photon, nsync, fineTime, code, 0);
            if (code == 0x3F)
            {
                if (recordType == HydraHarpT3)
                    overflow += 1024;
                else
                    overflow += nsync <= 1 ? 1024 : nsync * 1024UL;
                return new Record(RecordKind.Overflow, nsync, 0, 0, 0);
            }
            if (code > 0 && code < 16)
                return new Record(RecordKind.Marker, nsync, 0, 0, code);
            return new Record(RecordKind.Overflow, nsync, 0, 0, 0);
        }

        private static bool IsSupportedT3(uint recordType)
        {
            return recordType == PicoHarpT3
                || recordType == HydraHarpT3
                || recordType == HydraHarp2T3
                || recordType == TimeHarp260NT3
                || recordType == TimeHarp260PT3
                || recordType == GenericT3;
        }

        private static string BuildFlimHeader(Metadata metadata, string fileName)
        {
            string bools = string.Join(", ", Enumerable.Repeat("True", metadata.ChannelCount));
            string falses = string.Join(", ", Enumerable.Repeat("False", metadata.ChannelCount));
            string resolutions = string.Join(", ", Enumerable.Repeat(
                (metadata.TcspcResolutionSeconds * 1e12).ToString("R", CultureInfo.InvariantCulture),
                metadata.ChannelCount));
            string syncRates = string.Join(", ", Enumerable.Repeat(metadata.SyncRate.ToString(CultureInfo.InvariantCulture), metadata.ChannelCount));
            double acquisitionSeconds = metadata.TotalAcquisitionTime * metadata.GlobalResolutionSeconds;
            int countRate = acquisitionSeconds > 0
                ? (int)Math.Min(int.MaxValue, Math.Round(metadata.PhotonCount / acquisitionSeconds / metadata.ChannelCount))
                : 0;
            string countRates = string.Join(", ", Enumerable.Repeat(countRate.ToString(CultureInfo.InvariantCulture), metadata.ChannelCount));

            var header = new StringBuilder();
            header.Append("FLIMimage parameters\r");
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.pixelsPerLine = {0};\r", metadata.Width);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.linesPerFrame = {0};\r", metadata.Height);
            header.Append("State.Acq.ZStack = False;\r");
            header.Append("State.Acq.fastZScan = False;\r");
            header.Append("State.Acq.FastZ_nSlices = 1;\r");
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.nChannels = {0};\r", metadata.ChannelCount);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.nFrames = {0};\r", metadata.FrameCount);
            header.Append("State.Acq.nSlices = 1;\r");
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.msPerLine = {0:R};\r", metadata.LineTime * metadata.GlobalResolutionSeconds * 1000.0);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.acqFLIMA = [{0}];\r", bools);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.acquisition = [{0}];\r", bools);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Acq.aveFrameA = [{0}];\r", falses);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Spc.spcData.n_dataPoint = {0};\r", metadata.BinCount);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Spc.spcData.resolution = [{0}];\r", resolutions);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Spc.datainfo.syncRate = [{0}];\r", syncRates);
            header.AppendFormat(CultureInfo.InvariantCulture, "State.Spc.datainfo.countRate = [{0}];\r", countRates);
            header.AppendFormat(CultureInfo.InvariantCulture, "SaveChannels = [{0}];\r", bools);
            header.AppendFormat(CultureInfo.InvariantCulture, "Acquired_Time = {0:yyyy-MM-ddTHH:mm:ss.fff};\r", metadata.AcquiredTime);
            header.Append("Format = Linear;\r");
            return header.ToString();
        }

        private static uint MarkerMask(Dictionary<string, object> tags, string key)
        {
            int marker = GetInt32(tags, key, 0);
            return marker >= 1 && marker <= 32 ? 1U << (marker - 1) : 0;
        }

        private static int GetInt32(Dictionary<string, object> tags, string key, int fallback)
        {
            long value = GetInt64(tags, key, fallback);
            return value < int.MinValue || value > int.MaxValue ? fallback : (int)value;
        }

        private static long GetInt64(Dictionary<string, object> tags, string key, long fallback)
        {
            object value;
            if (!tags.TryGetValue(key, out value) || value == null)
                return fallback;
            if (value is long)
                return (long)value;
            if (value is bool)
                return (bool)value ? 1 : 0;
            return fallback;
        }

        private static double GetDouble(Dictionary<string, object> tags, string key, double fallback)
        {
            object value;
            if (!tags.TryGetValue(key, out value) || value == null)
                return fallback;
            if (value is double)
                return (double)value;
            if (value is long)
                return (long)value;
            return fallback;
        }

        private enum RecordKind
        {
            Photon,
            Overflow,
            Marker
        }

        private struct Record
        {
            public readonly RecordKind Kind;
            public readonly uint Time;
            public readonly int DTime;
            public readonly int Channel;
            public readonly int Marker;

            public Record(RecordKind kind, uint time, int dtime, int channel, int marker)
            {
                Kind = kind;
                Time = time;
                DTime = dtime;
                Channel = channel;
                Marker = marker;
            }
        }

        private sealed class Metadata
        {
            public long RecordOffset;
            public long RecordCount;
            public uint RecordType;
            public int Width;
            public int Height;
            public uint LineStartMask;
            public uint LineStopMask;
            public uint FrameMask;
            public bool Bidirectional;
            public bool Sinusoidal;
            public double TcspcResolutionSeconds;
            public double GlobalResolutionSeconds;
            public int SyncRate;
            public DateTime AcquiredTime;
            public int FrameCount;
            public int FirstChannel;
            public int ChannelCount;
            public int BinCount;
            public long PhotonCount;
            public ulong TotalLineTime;
            public long ValidLineCount;
            public ulong TotalAcquisitionTime;
            public long LineTime;
            public long PixelTime;
            public double FrameTimeSeconds;
        }
    }
}
