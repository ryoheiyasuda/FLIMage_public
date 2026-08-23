using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using static System.Net.Mime.MediaTypeNames;
using PhysiologyCSharp;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Net.WebRequestMethods;
using SharpAvi;
using SharpAvi.Output;
using SharpAvi.Codecs;
using System.Windows.Media.Media3D;
using MathLibrary;

using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;

namespace FLIMage.FileFormat
{
    public class AVIWriter
    {
        public string filename;
        public bool setup_done = false;

        AviWriter writer;
        IAviVideoStream stream;
        BitsPerPixel bits_per_pixel = BitsPerPixel.Bpp24;

        public AVIWriter(string filename1)
        {
            filename = filename1;
        }

        public void StreamSetup(int width, int height, int framesPerSecond)
        {
            writer = new AviWriter(filename)
            {
                FramesPerSecond = framesPerSecond,
                EmitIndex1 = true
            };

            stream = writer.AddVideoStream();
            stream.Width = width;
            stream.Height = height;
            stream.Codec = CodecIds.Uncompressed;
            stream.BitsPerPixel = bits_per_pixel;

            setup_done = true;
        }

        public void StreamSetup(Bitmap bitmap, int framesPerSecond)
        {
            writer = new AviWriter(filename)
            {
                FramesPerSecond = framesPerSecond,
                EmitIndex1 = true
            };

            stream = writer.AddVideoStream();
            stream.Width = bitmap.Width;
            stream.Height = bitmap.Height;
            stream.Codec = CodecIds.Uncompressed;
            stream.BitsPerPixel = bits_per_pixel;

            setup_done = true;
        }

        public void WriteOneFrame(Bitmap bitmap)
        {
            byte[] frameData;
            frameData = ImageProcessing.BitmapToByteArray(bitmap);
            stream.WriteFrame(true, frameData, 0, frameData.Length);
        }

        public void FinishWriting()
        {
            if (writer != null)
                writer.Close();
        }

        /// <summary>
        /// This is an example of how to use the class. Replace this with actual instance name...
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="frameDataArray"></param>
        public void StreamDataAll(int framesPerSecond, Bitmap[] bitmapArray)
        {
            this.StreamSetup(bitmapArray[0], framesPerSecond);
            for (int i = 0; i < bitmapArray.Length; i++)
            {
                this.WriteOneFrame(bitmapArray[i]);
            }
            this.FinishWriting();
        }
    }

    internal sealed class ZipStoreWriter : IDisposable
    {
        private const uint LocalHeaderSignature = 0x04034b50;
        private const uint CentralHeaderSignature = 0x02014b50;
        private const uint DataDescriptorSignature = 0x08074b50;
        private const uint Zip64EndSignature = 0x06064b50;
        private const uint Zip64LocatorSignature = 0x07064b50;
        private const uint EndSignature = 0x06054b50;
        private const ushort ZipVersion = 45;
        private const ushort Utf8Flag = 0x0800;
        private const ushort DataDescriptorFlag = 0x0008;

        private readonly FileStream stream;
        private readonly List<ZipEntryInfo> entries = new List<ZipEntryInfo>();
        private EntryStream activeEntry;
        private bool disposed;

        private struct ZipEntryInfo
        {
            public string Name;
            public uint Crc32;
            public ulong CompressedSize;
            public ulong UncompressedSize;
            public ulong LocalHeaderOffset;
        }

        public ZipStoreWriter(string path)
        {
            stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        public Stream BeginEntry(string name)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ZipStoreWriter));
            if (activeEntry != null)
                throw new InvalidOperationException("An entry is already open.");

            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            ulong headerOffset = (ulong)stream.Position;
            WriteLocalHeader(nameBytes);

            activeEntry = new EntryStream(this, name, headerOffset);
            return activeEntry;
        }

        public void WriteTextEntry(string name, string content)
        {
            using (var entry = BeginEntry(name))
            {
                var bytes = Encoding.UTF8.GetBytes(content ?? "");
                entry.Write(bytes, 0, bytes.Length);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            if (activeEntry != null)
                activeEntry.Dispose();

            WriteCentralDirectory();
            stream.Dispose();
        }

        private void WriteLocalHeader(byte[] nameBytes)
        {
            ushort flags = (ushort)(Utf8Flag | DataDescriptorFlag);
            WriteUInt32(LocalHeaderSignature);
            WriteUInt16(ZipVersion);
            WriteUInt16(flags);
            WriteUInt16(0); // method = store
            WriteUInt16(0); // mod time
            WriteUInt16(0); // mod date
            WriteUInt32(0); // crc
            WriteUInt32(0xFFFFFFFF); // comp size
            WriteUInt32(0xFFFFFFFF); // uncomp size
            WriteUInt16((ushort)nameBytes.Length);
            WriteUInt16(20); // extra length (Zip64)
            stream.Write(nameBytes, 0, nameBytes.Length);

            // Zip64 extra: header ID + size + uncomp + comp
            WriteUInt16(0x0001);
            WriteUInt16(16);
            WriteUInt64(0);
            WriteUInt64(0);
        }

        private void FinishEntry(EntryStream entry)
        {
            var info = new ZipEntryInfo
            {
                Name = entry.Name,
                Crc32 = entry.Crc32,
                CompressedSize = entry.BytesWritten,
                UncompressedSize = entry.BytesWritten,
                LocalHeaderOffset = entry.LocalHeaderOffset
            };
            entries.Add(info);

            WriteUInt32(DataDescriptorSignature);
            WriteUInt32(info.Crc32);
            WriteUInt64(info.CompressedSize);
            WriteUInt64(info.UncompressedSize);

            activeEntry = null;
        }

        private void WriteCentralDirectory()
        {
            ulong centralStart = (ulong)stream.Position;
            foreach (var entry in entries)
            {
                byte[] nameBytes = Encoding.UTF8.GetBytes(entry.Name);
                ushort flags = (ushort)(Utf8Flag | DataDescriptorFlag);
                WriteUInt32(CentralHeaderSignature);
                WriteUInt16(ZipVersion); // version made by
                WriteUInt16(ZipVersion); // version needed
                WriteUInt16(flags);
                WriteUInt16(0); // method = store
                WriteUInt16(0); // mod time
                WriteUInt16(0); // mod date
                WriteUInt32(entry.Crc32);
                WriteUInt32(0xFFFFFFFF);
                WriteUInt32(0xFFFFFFFF);
                WriteUInt16((ushort)nameBytes.Length);
                WriteUInt16(28); // extra length (Zip64 sizes+offset)
                WriteUInt16(0); // comment length
                WriteUInt16(0); // disk number
                WriteUInt16(0); // internal attrs
                WriteUInt32(0); // external attrs
                WriteUInt32(0xFFFFFFFF);
                stream.Write(nameBytes, 0, nameBytes.Length);

                WriteUInt16(0x0001);
                WriteUInt16(24);
                WriteUInt64(entry.UncompressedSize);
                WriteUInt64(entry.CompressedSize);
                WriteUInt64(entry.LocalHeaderOffset);
            }

            ulong centralSize = (ulong)stream.Position - centralStart;
            WriteZip64End(centralStart, centralSize, (ulong)entries.Count);
        }

        private void WriteZip64End(ulong centralStart, ulong centralSize, ulong entryCount)
        {
            ulong zip64EndOffset = (ulong)stream.Position;
            WriteUInt32(Zip64EndSignature);
            WriteUInt64(44); // size of record
            WriteUInt16(ZipVersion);
            WriteUInt16(ZipVersion);
            WriteUInt32(0); // disk
            WriteUInt32(0); // disk with central dir
            WriteUInt64(entryCount);
            WriteUInt64(entryCount);
            WriteUInt64(centralSize);
            WriteUInt64(centralStart);

            WriteUInt32(Zip64LocatorSignature);
            WriteUInt32(0); // disk with zip64 end
            WriteUInt64(zip64EndOffset);
            WriteUInt32(1); // total disks

            WriteUInt32(EndSignature);
            WriteUInt16(0); // disk
            WriteUInt16(0); // disk with central dir
            WriteUInt16(0xFFFF);
            WriteUInt16(0xFFFF);
            WriteUInt32(0xFFFFFFFF);
            WriteUInt32(0xFFFFFFFF);
            WriteUInt16(0); // comment length
        }

        private void WriteUInt16(ushort value) => stream.Write(BitConverter.GetBytes(value), 0, 2);
        private void WriteUInt32(uint value) => stream.Write(BitConverter.GetBytes(value), 0, 4);
        private void WriteUInt64(ulong value) => stream.Write(BitConverter.GetBytes(value), 0, 8);

        private sealed class EntryStream : Stream
        {
            private readonly ZipStoreWriter owner;
            private readonly CRC32 crc32 = new CRC32();
            private bool disposed;

            public string Name { get; }
            public ulong LocalHeaderOffset { get; }
            public uint Crc32 => crc32.Current;
            public ulong BytesWritten { get; private set; }

            public EntryStream(ZipStoreWriter owner, string name, ulong offset)
            {
                this.owner = owner;
                Name = name;
                LocalHeaderOffset = offset;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(EntryStream));
                owner.stream.Write(buffer, offset, count);
                crc32.Update(buffer, offset, count);
                BytesWritten += (ulong)count;
            }

            protected override void Dispose(bool disposing)
            {
                if (!disposed && disposing)
                {
                    disposed = true;
                    owner.FinishEntry(this);
                }
                base.Dispose(disposing);
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => (long)BytesWritten;
            public override long Position { get => (long)BytesWritten; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        private sealed class CRC32
        {
            private static readonly uint[] Table = BuildTable();
            private uint current = 0xFFFFFFFF;
            public uint Current => current ^ 0xFFFFFFFF;

            public void Update(byte[] buffer, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    current = (current >> 8) ^ Table[(current ^ buffer[offset + i]) & 0xFF];
                }
            }

            private static uint[] BuildTable()
            {
                var table = new uint[256];
                for (uint i = 0; i < table.Length; i++)
                {
                    uint c = i;
                    for (int k = 0; k < 8; k++)
                    {
                        c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                    }
                    table[i] = c;
                }
                return table;
            }
        }
    }

    public class PhotonFileHandle
    {
        private const string PhotonSafeModeEnvVar = "FLIMAGE_PHOTON_SAFE_MODE";
        private static bool? photonSafeMode;

        public string filename;
        public string filename2;
        public string extension;
        public string filename_no_ext;
        public string dirname;
        public string slice_name_for_DLL;
        public string slice_name_without_dir;
        public List<string> extracted_file_name = new List<string>();
        public ScanParameters State;
        public string header_all = "";
        public DateTime binary_trigger_time;

        private CompressionLevel compressionLevel = CompressionLevel.Fastest;
        private List<ZipArchiveEntry> file_list = new List<ZipArchiveEntry>();
        private object syncZip = new object();

        private bool tar_mode = false;
        private bool zip_stream_write = false;
        private ZipStoreWriter zip_store_writer = null;
        private Stream zip_bin_stream = null;
        private const int CopyBufferSize = 1024 * 1024;
        private const long MaxZipUpdateEntryBytes = int.MaxValue - 1;
        private const int StableFileCheckDelayMs = 200;
        private const int StableFileMaxAttempts = 10;
        private StreamingZipIndex streamingZipIndex = null;
        private bool streamingZipIndexFailed = false;
        private readonly object streamingZipIndexLock = new object();
        private bool? hasCentralDirectory = null;
        private readonly object centralDirectoryLock = new object();

        private static bool IsPhotonSafeModeEnabled()
        {
            if (photonSafeMode.HasValue)
                return photonSafeMode.Value;

            var value = Environment.GetEnvironmentVariable(PhotonSafeModeEnvVar);
            if (string.IsNullOrWhiteSpace(value))
            {
                photonSafeMode = false;
                return photonSafeMode.Value;
            }

            value = value.Trim();
            photonSafeMode = value == "1"
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
            return photonSafeMode.Value;
        }

        public static bool GetPhotonSafeModeEnabled()
        {
            return IsPhotonSafeModeEnabled();
        }

        public static void SetPhotonSafeMode(bool enabled)
        {
            photonSafeMode = enabled;
            Environment.SetEnvironmentVariable(PhotonSafeModeEnvVar, enabled ? "1" : "0");
        }

        public PhotonFileHandle(string photon_file_name)
        {
            filename = photon_file_name;
            extension = Path.GetExtension(filename);
            filename_no_ext = Path.GetFileNameWithoutExtension(filename);
            dirname = Path.GetDirectoryName(filename);
            State = new ScanParameters();
        }

        public bool IsZip(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] signature = new byte[4];
                fs.Read(signature, 0, 4);
                return signature[0] == 0x50 && signature[1] == 0x4B; // "PK"
            }
        }

        private bool HasZipCentralDirectory()
        {
            if (hasCentralDirectory.HasValue)
                return hasCentralDirectory.Value;

            lock (centralDirectoryLock)
            {
                if (hasCentralDirectory.HasValue)
                    return hasCentralDirectory.Value;

                hasCentralDirectory = TryFindZipCentralDirectory(filename);
                return hasCentralDirectory.Value;
            }
        }

        private void InvalidateCentralDirectory()
        {
            lock (centralDirectoryLock)
            {
                hasCentralDirectory = false;
            }
        }

        private static bool TryFindZipCentralDirectory(string path)
        {
            const int MaxScan = 66000; // EOCD must be within the last 64k + header.
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long size = fs.Length;
                if (size < 4)
                    return false;

                int scan = (int)Math.Min(size, MaxScan);
                fs.Seek(-scan, SeekOrigin.End);
                byte[] buffer = new byte[scan];
                int read = fs.Read(buffer, 0, scan);
                for (int i = read - 4; i >= 0; i--)
                {
                    if (buffer[i] == 0x50 && buffer[i + 1] == 0x4B && buffer[i + 2] == 0x05 && buffer[i + 3] == 0x06)
                    {
                        long eocdOffset = size - read + i;
                        return ValidateCentralDirectory(fs, eocdOffset, size);
                    }
                }
            }

            return false;
        }

        private static bool ValidateCentralDirectory(FileStream fs, long eocdOffset, long fileLength)
        {
            if (eocdOffset < 0 || eocdOffset + 22 > fileLength)
                return false;

            byte[] eocd = new byte[22];
            fs.Seek(eocdOffset, SeekOrigin.Begin);
            int read = fs.Read(eocd, 0, eocd.Length);
            if (read < eocd.Length)
                return false;

            ushort commentLength = BitConverter.ToUInt16(eocd, 20);
            if (eocdOffset + 22 + commentLength > fileLength)
                return false;

            uint centralSize = BitConverter.ToUInt32(eocd, 12);
            uint centralOffset = BitConverter.ToUInt32(eocd, 16);
            if (centralSize == 0xFFFFFFFF || centralOffset == 0xFFFFFFFF)
                return false;

            return centralOffset + centralSize <= fileLength;
        }

        private bool EnsureStreamingZipIndex()
        {
            if (streamingZipIndex != null)
                return true;
            if (streamingZipIndexFailed)
                return false;

            lock (streamingZipIndexLock)
            {
                if (streamingZipIndex != null)
                    return true;
                if (streamingZipIndexFailed)
                    return false;

                streamingZipIndex = StreamingZipIndex.TryBuild(filename);
                if (streamingZipIndex == null)
                {
                    streamingZipIndexFailed = true;
                    return false;
                }

                Debug.WriteLine("Debug: Using streaming ZIP reader (missing central directory).");
                if (streamingZipIndex.HasTruncatedEntry)
                    Debug.WriteLine("Debug: Streaming ZIP appears truncated; last entry may be incomplete.");
                return true;
            }
        }

        private bool TryGetStreamingEntry(string entryName, out StreamingZipIndex.EntryInfo entry)
        {
            entry = default(StreamingZipIndex.EntryInfo);
            return EnsureStreamingZipIndex() && streamingZipIndex.TryGetEntry(entryName, out entry);
        }

        private bool TryGetStreamingEntryWithFallback(string entryName, out StreamingZipIndex.EntryInfo entry)
        {
            if (TryGetStreamingEntry(entryName, out entry))
                return true;

            string resolvedName = FindArchiveEntryName(EnumerateStreamingEntries().Select(e => e.Name), entryName);
            return !string.IsNullOrEmpty(resolvedName) && TryGetStreamingEntry(resolvedName, out entry);
        }

        private static ZipArchiveEntry GetZipEntryWithFallback(ZipArchive archive, string entryName)
        {
            if (archive == null)
                return null;

            var entry = archive.GetEntry(entryName);
            if (entry != null)
                return entry;

            string resolvedName = FindArchiveEntryName(archive.Entries.Select(e => e.FullName), entryName);
            return string.IsNullOrEmpty(resolvedName) ? null : archive.GetEntry(resolvedName);
        }

        private static IArchiveEntry GetArchiveEntryWithFallback(IEnumerable<IArchiveEntry> entries, string entryName)
        {
            if (entries == null)
                return null;

            var fileEntries = entries.Where(e => e != null && !e.IsDirectory).ToList();
            var entry = fileEntries.FirstOrDefault(e => EntryNameEquals(e.Key, entryName));
            if (entry != null)
                return entry;

            string resolvedName = FindArchiveEntryName(fileEntries.Select(e => e.Key), entryName);
            return string.IsNullOrEmpty(resolvedName)
                ? null
                : fileEntries.FirstOrDefault(e => EntryNameEquals(e.Key, resolvedName));
        }

        private static string FindArchiveEntryName(IEnumerable<string> entryNames, string requestedName)
        {
            if (entryNames == null || string.IsNullOrWhiteSpace(requestedName))
                return null;

            var names = entryNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (names.Count == 0)
                return null;

            string normalized = NormalizeEntryName(requestedName);
            var exact = names.FirstOrDefault(n => EntryNameEquals(n, normalized));
            if (exact != null)
                return exact;

            string directory = GetEntryDirectory(normalized);
            string fileName = GetEntryFileName(normalized);
            string extension = Path.GetExtension(fileName);
            string stem = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrWhiteSpace(stem) || string.IsNullOrWhiteSpace(extension))
                return null;

            // Older photon-save paths could pass a device-suffixed name into code that
            // appended the device suffix again, producing entries like *_0_0_0.bin.
            string legacyName = directory + stem + "_0" + extension;
            var legacy = names.FirstOrDefault(n => EntryNameEquals(n, legacyName));
            if (legacy != null)
                return legacy;

            string legacyPrefix = directory + stem + "_";
            return names
                .Where(n => HasEntryExtension(n, extension)
                    && NormalizeEntryName(n).StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => NormalizeEntryName(n).Length)
                .ThenBy(n => NormalizeEntryName(n), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        private static bool EntryNameEquals(string left, string right)
        {
            return string.Equals(NormalizeEntryName(left), NormalizeEntryName(right), StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasEntryExtension(string entryName, string extension)
        {
            return string.Equals(Path.GetExtension(GetEntryFileName(entryName)), extension, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeEntryName(string entryName)
        {
            return string.IsNullOrEmpty(entryName)
                ? entryName
                : entryName.Replace('\\', '/').TrimStart('/');
        }

        private static string GetEntryDirectory(string entryName)
        {
            string normalized = NormalizeEntryName(entryName);
            int slash = normalized?.LastIndexOf('/') ?? -1;
            return slash < 0 ? string.Empty : normalized.Substring(0, slash + 1);
        }

        private static string GetEntryFileName(string entryName)
        {
            string normalized = NormalizeEntryName(entryName);
            int slash = normalized?.LastIndexOf('/') ?? -1;
            return slash < 0 ? normalized : normalized.Substring(slash + 1);
        }

        private IEnumerable<StreamingZipIndex.EntryInfo> EnumerateStreamingEntries()
        {
            if (!EnsureStreamingZipIndex())
                return Enumerable.Empty<StreamingZipIndex.EntryInfo>();
            return streamingZipIndex.Entries;
        }

        public PhotonFileHandle(ScanParameters state1)
        {
            CreateNew(state1);
        }

        public PhotonFileHandle(string photon_file_name, ScanParameters state1)
        {
            State = state1;
            filename = photon_file_name;
            extension = Path.GetExtension(filename);
            filename_no_ext = Path.GetFileNameWithoutExtension(filename);
            dirname = Path.GetDirectoryName(filename);

            if (extension == State.Files.extension_photon)
            {
                if (!IsPhotonSafeModeEnabled())
                {
                    CreateArchiveFromPhotonBinaries(filename);
                }
            }
        }

        public void CreateNew(ScanParameters state1)
        {
            State = state1;
            bool safeMode = IsPhotonSafeModeEnabled();
            filename = State.Files.GetPhotonFilePath(!safeMode);
            extension = Path.GetExtension(filename);
            filename_no_ext = Path.GetFileNameWithoutExtension(filename);
            dirname = Path.GetDirectoryName(filename);

            FileIO fileIO = new FileIO(State);
            string headerContent = fileIO.AllSetupFile();

            if (extension == State.Files.extension_photon)
            {
                try
                {
                    if (!string.IsNullOrEmpty(dirname))
                        Directory.CreateDirectory(dirname);
                    System.IO.File.WriteAllText(filename, headerContent ?? string.Empty, Encoding.UTF8);
                }
                catch (Exception)
                {
                }
            }
            else if (extension == State.Files.extension_photon_archive)
            {
                var header_file_name = filename_no_ext + State.Files.extension_photon; //Path.Combine(dirname, filename_no_ext) + State.Files.extension_photon;

                if (tar_mode)
                {
                    using (var fs = new FileStream(filename, FileMode.Create))
                    using (var tarWriter = WriterFactory.OpenWriter(fs, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
                    {
                        // Create a temporary stream to hold text
                        using (var tempStream = new MemoryStream())
                        using (var writerStream = new StreamWriter(tempStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
                        {
                            writerStream.Write(headerContent);
                            writerStream.Flush();       // Ensure all text is written to the stream
                            tempStream.Position = 0;    // Rewind the stream to the beginning

                            tarWriter.Write(header_file_name, tempStream, null);
                        }
                    }
#if DEBUG
                    using (var archive = TarArchive.OpenArchive(filename))
                    {
                        var file_list = archive.Entries.Where(e => !e.IsDirectory).ToList();
                        if (file_list.Count > 0)
                            Debug.WriteLine("File archived: " + file_list[0].ToString());
                        else
                            Debug.WriteLine("Failed to create " + header_file_name);
                    }
#endif
                }
                else
                {
                    zip_stream_write = true;
                    zip_store_writer = new ZipStoreWriter(filename);
                    zip_store_writer.WriteTextEntry(header_file_name, headerContent);
                }
            }
        }

        public bool IsStreamingZipWrite => zip_stream_write && zip_store_writer != null;

        public Stream BeginPhotonBinaryStreamWrite(string entryName)
        {
            if (!IsStreamingZipWrite)
                return null;

            lock (syncZip)
            {
                EndPhotonBinaryStreamWrite();
                zip_bin_stream = zip_store_writer.BeginEntry(entryName);
                return zip_bin_stream;
            }
        }

        public void EndPhotonBinaryStreamWrite()
        {
            lock (syncZip)
            {
                if (zip_bin_stream != null)
                {
                    zip_bin_stream.Dispose();
                    zip_bin_stream = null;
                }
            }
        }

        public void AddPhotonTimeEntry(string entryName, DateTime acquiredTime)
        {
            if (!IsStreamingZipWrite)
                return;

            lock (syncZip)
            {
                zip_store_writer.WriteTextEntry(entryName, "Acquired_Time = " + State.Acq.timeToString(acquiredTime));
            }
        }

        public bool GetAcquisitionTime(string entry_filename, out DateTime dt)
        {
            dt = DateTime.Now;
            bool header_exist = false;

            if (!string.IsNullOrEmpty(dirname) && !string.IsNullOrEmpty(entry_filename))
            {
                var loosePath = Path.Combine(dirname, entry_filename);
                if (System.IO.File.Exists(loosePath) && TryParseAcquiredTimeFromFile(loosePath, out dt))
                    return true;
            }

            if (!IsZip(filename))
            {
                using (var archive = TarArchive.OpenArchive(filename))
                {
                    var entry = archive.Entries
                        .FirstOrDefault(e => !e.IsDirectory && e.Key.Equals(entry_filename, StringComparison.OrdinalIgnoreCase));

                    if (entry != null)
                    {
                        using (var sr = new StreamReader(entry.OpenEntryStream()))
                            header_exist = TryParseAcquiredTimeFromReader(sr, out dt);
                    }
                }

            }
            else
            {
                bool useStreaming = !HasZipCentralDirectory();
                if (!useStreaming)
                {
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(filename))
                        {
                            var entry = GetZipEntryWithFallback(archive, entry_filename); //prefix + ".txt");
                            if (entry != null)
                            {
                                using (StreamReader sr = new StreamReader(entry.Open()))
                                    header_exist = TryParseAcquiredTimeFromReader(sr, out dt);
                            }
                        }
                    }
                    catch (InvalidDataException)
                    {
                        InvalidateCentralDirectory();
                        useStreaming = true;
                    }
                }

                if (useStreaming && TryGetStreamingEntryWithFallback(entry_filename, out var streamEntry))
                {
                    using (StreamReader sr = new StreamReader(streamingZipIndex.OpenEntryStream(streamEntry)))
                        header_exist = TryParseAcquiredTimeFromReader(sr, out dt);
                }
            }

            if (!header_exist)
            {
                if (!string.IsNullOrWhiteSpace(header_all) && TryParseAcquiredTimeFromText(header_all, out dt))
                    return true;

                if (!string.IsNullOrEmpty(dirname))
                {
                    string headerName = filename_no_ext + State.Files.extension_photon;
                    string headerPath = Path.Combine(dirname, headerName);
                    if (System.IO.File.Exists(headerPath) && TryParseAcquiredTimeFromFile(headerPath, out dt))
                        return true;
                }

                if (TryParseSliceIndex(entry_filename, out int slice))
                {
                    dt = State.Acq.estimatedAcquiredTime(0, slice);
                    Debug.WriteLine($"Debug: Using estimated Acquired_Time for slice {slice}.");
                    return true;
                }

                Debug.WriteLine("Debug: Acquired_Time missing; using DateTime.Now.");
                return true;
            }

            return header_exist;
        }

        private bool TryParseAcquiredTimeFromFile(string path, out DateTime dt)
        {
            dt = DateTime.Now;
            try
            {
                using (var sr = new StreamReader(path))
                {
                    return TryParseAcquiredTimeFromReader(sr, out dt);
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseAcquiredTimeFromText(string text, out DateTime dt)
        {
            dt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (TryParseAcquiredTimeFromLine(line, out dt))
                        return true;
                }
            }

            return false;
        }

        private bool TryParseAcquiredTimeFromReader(TextReader reader, out DateTime dt)
        {
            dt = DateTime.Now;
            if (reader == null)
                return false;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (TryParseAcquiredTimeFromLine(line, out dt))
                    return true;
            }

            return false;
        }

        private bool TryParseAcquiredTimeFromLine(string line, out DateTime dt)
        {
            dt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(line) || line.IndexOf("Acquired_Time", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            var parts = line.Split('=');
            if (parts.Length < 2)
                return false;

            var value = parts[1].Trim();
            if (DateTime.TryParseExact(value, State.Acq.datetime_formatter, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out dt))
                return true;

            return DateTime.TryParse(value, out dt);
        }

        private static bool TryParseSliceIndex(string entryName, out int slice)
        {
            slice = 0;
            if (string.IsNullOrWhiteSpace(entryName))
                return false;

            string baseName = Path.GetFileNameWithoutExtension(entryName);
            int underscore = baseName.LastIndexOf('_');
            if (underscore < 0 || underscore == baseName.Length - 1)
                return false;

            return int.TryParse(baseName.Substring(underscore + 1), out slice);
        }


        public void CreateArchiveFromPhotonBinaries(string header_file)
        {
            filename = changeExtension(header_file, State.Files.extension_photon_archive);
            extension = Path.GetExtension(filename);
            filename_no_ext = Path.GetFileNameWithoutExtension(filename);
            dirname = Path.GetDirectoryName(filename);

            string prefix = filename_no_ext;
            var searchPattern = prefix + "*.bin";
            var filepaths = Directory.GetFiles(dirname, searchPattern);

            if (!System.IO.File.Exists(filename))
            {
                if (tar_mode)
                {
                    using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                    using (var writer = WriterFactory.OpenWriter(fs, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
                    {
                        // Add the header file and delete the source
                        using (var stream = System.IO.File.OpenRead(header_file))
                        {
                            writer.Write(Path.GetFileName(header_file), stream, null);
                        }
                        System.IO.File.Delete(header_file);

                        foreach (var file in filepaths)
                        {
                            var time_file_name = Path.GetFileNameWithoutExtension(file);
                            time_file_name = time_file_name.Substring(0, time_file_name.Length - 2);
                            var sliceN = Convert.ToInt32(time_file_name.Split('_').Last());
                            var time_file_full_name = Path.Combine(dirname, time_file_name + ".txt");

                            // Add .txt time file
                            if (System.IO.File.Exists(time_file_full_name))
                            {
                                using (var stream = System.IO.File.OpenRead(time_file_full_name))
                                {
                                    writer.Write(time_file_name + ".txt", stream, null);
                                }
                            }
                            else
                            {
                                // Create and write the acquired time file dynamically
                                string timeContent = "Acquired_Time = " + State.Acq.timeToString(State.Acq.estimatedAcquiredTime(0, sliceN));
                                using (var memStream = new MemoryStream())
                                using (var sw = new StreamWriter(memStream, Encoding.UTF8, 1024, leaveOpen: true))
                                {
                                    sw.Write(timeContent);
                                    sw.Flush();
                                    memStream.Position = 0;
                                    writer.Write(time_file_name + ".txt", memStream, null);
                                }
                            }

                            // Add .bin file if not already present and then delete it
                            using (var binStream = System.IO.File.OpenRead(file))
                            {
                                writer.Write(Path.GetFileName(file), binStream, null);
                            }
                            System.IO.File.Delete(file);
                        }
                    }

                }
                else
                {
                    using (var zip = ZipFile.Open(filename, ZipArchiveMode.Update))
                    {
                        zip.CreateEntryFromFile(header_file, Path.GetFileName(header_file), compressionLevel);
                        System.IO.File.Delete(header_file);

                        foreach (var file in filepaths)
                        {
                            var time_file_name = Path.GetFileNameWithoutExtension(file);
                            time_file_name = time_file_name.Substring(0, time_file_name.Length - 2);
                            var sliceN = Convert.ToInt32(time_file_name.Split('_').Last());
                            var time_file_full_name = Path.Combine(dirname, time_file_name + ".txt");

                            if (System.IO.File.Exists(time_file_full_name))
                            {
                                zip.CreateEntryFromFile(time_file_full_name, time_file_name + ".txt", compressionLevel);
                            }
                            else
                                CreateAcquiredTimeFile(time_file_name + ".txt", zip, State.Acq.estimatedAcquiredTime(0, sliceN));

                            var entry1 = zip.GetEntry(Path.GetFileName(file));
                            if (entry1 == null)
                            {
                                zip.CreateEntryFromFile(file, Path.GetFileName(file), compressionLevel);
                                System.IO.File.Delete(file);
                            }
                        }
                    }
                }
            }
        }

        public void CreateAcquiredTimeFile(string acq_header_name, ZipArchive zip, DateTime acquiredTime)
        {
            CreateAcquiredTimeEntry(zip, acq_header_name, acquiredTime, compressionLevel);
        }

        private void CreateAcquiredTimeEntry(ZipArchive zip, string acq_header_name, DateTime acquiredTime, CompressionLevel level)
        {
            var acq_header = zip.CreateEntry(acq_header_name, level);
            using (var entryStream = acq_header.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.Write("Acquired_Time = " + State.Acq.timeToString(acquiredTime));
            }
        }

        public void AddPhotonFileEntry(string filename_bin, DateTime acquiredTime)
        {
            var filename_bin0 = Path.GetFileNameWithoutExtension(filename_bin);
            var slice_base_name = filename_bin0;
            if (!string.IsNullOrEmpty(filename_bin)
                && filename_bin.EndsWith("_0.bin", StringComparison.OrdinalIgnoreCase)
                && filename_bin0.EndsWith("_0", StringComparison.OrdinalIgnoreCase))
            {
                slice_base_name = filename_bin0.Substring(0, filename_bin0.Length - 2);
            }

            var acq_header_name = slice_base_name + ".txt";
            if (extension == State.Files.extension_photon)
            {
                var acqPath = string.IsNullOrEmpty(dirname)
                    ? acq_header_name
                    : Path.Combine(dirname, acq_header_name);
                var content = "Acquired_Time = " + State.Acq.timeToString(acquiredTime);

                try
                {
                    System.IO.File.WriteAllText(acqPath, content, Encoding.UTF8);
                }
                catch (Exception)
                {
                }
                return;
            }

            var filepaths = Directory.GetFiles(dirname, slice_base_name + "*.bin");
            if (tar_mode)
            {
#if DEBUG
                using (var archive = TarArchive.OpenArchive(filename))
                {
                    var file_list = archive.Entries.Where(e => !e.IsDirectory).ToList();
                    if (file_list.Count > 0)
                        Debug.WriteLine("File archived: " + file_list[0].ToString());
                }
#endif

                string acqTimeText = "Acquired_Time = " + State.Acq.timeToString(acquiredTime);

                using (var fs = new FileStream(filename, FileMode.Open))
                using (var writer = WriterFactory.OpenWriter(fs, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
                {
                    // Write acquired time file
                    using (var tempStream = new MemoryStream())
                    using (var streamWriter = new StreamWriter(tempStream, Encoding.UTF8, 1024, leaveOpen: true))
                    {
                        streamWriter.Write(acqTimeText);
                        streamWriter.Flush();
                        tempStream.Position = 0;
                        writer.Write(acq_header_name, tempStream, null);
                    }

                    // Add binary files
                    foreach (var file in filepaths)
                    {
                        using (var binStream = System.IO.File.OpenRead(file))
                        {
                            writer.Write(Path.GetFileName(file), binStream, null);
                        }

                        // Delete after adding
                        System.IO.File.Delete(file);
                    }
                }
            }
            else
            {
                lock (syncZip)
                {
                    if (zip_stream_write && zip_store_writer != null)
                    {
                        zip_store_writer.WriteTextEntry(acq_header_name, "Acquired_Time = " + State.Acq.timeToString(acquiredTime));

                        foreach (var file in filepaths)
                        {
                            if (!WaitForStableFile(file))
                                continue;
                            var entryName = Path.GetFileName(file);
                            using (var entry = zip_store_writer.BeginEntry(entryName))
                            using (var binStream = System.IO.File.OpenRead(file))
                            {
                                binStream.CopyTo(entry);
                            }
                            System.IO.File.Delete(file);
                        }
                        return;
                    }

                    if (RequiresZipRebuild(filepaths))
                    {
                        RebuildPhotonArchive(acq_header_name, filepaths, acquiredTime);
                    }
                    else
                    {
                        var mode = System.IO.File.Exists(filename) ? ZipArchiveMode.Update : ZipArchiveMode.Create;
                        using (var zip = ZipFile.Open(filename, mode))
                        {
                            var existingHeader = zip.GetEntry(acq_header_name);
                            existingHeader?.Delete();
                            CreateAcquiredTimeFile(acq_header_name, zip, acquiredTime);

                            foreach (var file in filepaths)
                            {
                                if (!WaitForStableFile(file))
                                    continue;
                                var entryName = Path.GetFileName(file);
                                var existingEntry = zip.GetEntry(entryName);
                                existingEntry?.Delete();
                                zip.CreateEntryFromFile(file, entryName, CompressionLevel.Fastest);
                                System.IO.File.Delete(file);
                            }
                        }
                    }
                }
            }
        }

        public void FinishWriting()
        {
            lock (syncZip)
            {
                EndPhotonBinaryStreamWrite();

                if (zip_store_writer != null)
                {
                    zip_store_writer.Dispose();
                    zip_store_writer = null;
                }
            }
        }

        public string changeExtension(string filename_old, string ext_new)
        {
            var filename_ne1 = Path.GetFileNameWithoutExtension(filename_old);
            var dirname1 = Path.GetDirectoryName(filename_old);
            var filename_new = Path.Combine(dirname1, filename_ne1);
            return filename_new + ext_new;
        }


        public void FinishReading()
        {
            if (extension == State.Files.extension_photon_archive)
            {
                foreach (var file in extracted_file_name)
                {
                    if (System.IO.File.Exists(file))
                        System.IO.File.Delete(file);
                }
            }
        }

        public string MakePhotonBinaryNameForDLL(int slice)
        {
            var slice_name = String.Format("{0}_{1}", filename_no_ext, slice);
            slice_name_for_DLL = Path.Combine(dirname, slice_name);
            slice_name_without_dir = slice_name;
            return slice_name_for_DLL;
        }

        public uint[] ReadUInt32ArrayFromArchive(string entryName)
        {
            byte[] rawBytes = null;

            if (IsZip(filename))
            {
                bool useStreaming = !HasZipCentralDirectory();
                if (!useStreaming)
                {
                    try
                    {
                        using (var archive = ZipFile.OpenRead(filename))
                        {
                            var entry = GetZipEntryWithFallback(archive, entryName);
                            if (entry != null)
                            {
                                using (var stream = entry.Open())
                                {
                                    rawBytes = ReadAllBytes(stream, entry.Length);
                                }
                            }
                        }
                    }
                    catch (InvalidDataException)
                    {
                        InvalidateCentralDirectory();
                        useStreaming = true;
                    }
                }

                if (useStreaming && TryGetStreamingEntryWithFallback(entryName, out var streamEntry))
                {
                    using (var stream = streamingZipIndex.OpenEntryStream(streamEntry))
                    {
                        rawBytes = ReadAllBytes(stream, streamEntry.Length);
                    }
                }
            }
            else
            {
                using (var archive = TarArchive.OpenArchive(filename))
                {
                    var entry = GetArchiveEntryWithFallback(archive.Entries, entryName);
                    if (entry != null)
                    {
                        using (var stream = entry.OpenEntryStream())
                        {
                            rawBytes = ReadAllBytes(stream, entry.Size);
                        }
                    }
                }
            }

            if (rawBytes == null || rawBytes.Length % 4 != 0)
                throw new InvalidDataException("Invalid binary data length.");

            return ConvertRawBytesToUInt32(rawBytes);
        }

        public uint[] ReadUInt32ArrayFromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, FileOptions.SequentialScan))
            {
                var rawBytes = ReadAllBytes(stream, stream.Length);
                if (rawBytes == null || rawBytes.Length % 4 != 0)
                    throw new InvalidDataException("Invalid binary data length.");

                return ConvertRawBytesToUInt32(rawBytes);
            }
        }

        public long GetPhotonBinaryLength(string entryName)
        {
            if (IsZip(filename))
            {
                if (!HasZipCentralDirectory() && TryGetStreamingEntryWithFallback(entryName, out var streamEntryImmediate))
                    return streamEntryImmediate.Length;

                try
                {
                    using (var archive = ZipFile.OpenRead(filename))
                    {
                        var zipEntry = GetZipEntryWithFallback(archive, entryName);
                        return zipEntry?.Length ?? -1;
                    }
                }
                catch (InvalidDataException)
                {
                    InvalidateCentralDirectory();
                    if (TryGetStreamingEntryWithFallback(entryName, out var streamEntryFallback))
                        return streamEntryFallback.Length;
                }
                return -1;
            }

            using (var archive = TarArchive.OpenArchive(filename))
            {
                var entry = GetArchiveEntryWithFallback(archive.Entries, entryName);
                return entry?.Size ?? -1;
            }
        }

        public void StreamUInt32ChunksFromArchive(string entryName, int bytesPerChunk, Action<uint[], bool> sendChunk)
        {
            if (bytesPerChunk <= 0)
                throw new ArgumentOutOfRangeException(nameof(bytesPerChunk));

            if (IsZip(filename))
            {
                if (!HasZipCentralDirectory())
                {
                    if (!TryGetStreamingEntryWithFallback(entryName, out var streamEntryImmediate))
                        throw new FileNotFoundException("Photon binary entry not found.", entryName);

                    using (var stream = streamingZipIndex.OpenEntryStream(streamEntryImmediate))
                    {
                        StreamUInt32ChunksFromStream(stream, bytesPerChunk, sendChunk);
                    }
                }
                else
                {
                    try
                    {
                        using (var archive = ZipFile.OpenRead(filename))
                        {
                            var entry = GetZipEntryWithFallback(archive, entryName);
                            if (entry == null)
                                throw new FileNotFoundException("Photon binary entry not found.", entryName);

                            using (var stream = entry.Open())
                            {
                                StreamUInt32ChunksFromStream(stream, bytesPerChunk, sendChunk);
                            }
                        }
                    }
                    catch (InvalidDataException)
                    {
                        InvalidateCentralDirectory();
                        if (!TryGetStreamingEntryWithFallback(entryName, out var streamEntryFallback))
                            throw new FileNotFoundException("Photon binary entry not found.", entryName);

                        using (var stream = streamingZipIndex.OpenEntryStream(streamEntryFallback))
                        {
                            StreamUInt32ChunksFromStream(stream, bytesPerChunk, sendChunk);
                        }
                    }
                }
            }
            else
            {
                using (var archive = TarArchive.OpenArchive(filename))
                {
                    var entry = GetArchiveEntryWithFallback(archive.Entries, entryName);
                    if (entry == null)
                        throw new FileNotFoundException("Photon binary entry not found.", entryName);

                    using (var stream = entry.OpenEntryStream())
                    {
                        StreamUInt32ChunksFromStream(stream, bytesPerChunk, sendChunk);
                    }
                }
            }
        }




        /// <summary>
        /// Return if success
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public bool ExtractFile(int slice, out string ext)
        {
            var success = false;
            ext = ".bin";

            var slice_name = String.Format("{0}_{1}", filename_no_ext, slice);

            MakePhotonBinaryNameForDLL(slice);

            extracted_file_name.Clear();

            if (extension == State.Files.extension_photon)
            {
                var searchPattern = slice_name + "*.bin";
                var filepaths = Directory.GetFiles(dirname, searchPattern);
                extracted_file_name = filepaths.ToList();
            }
            else if (extension == State.Files.extension_photon_archive)
            {
                if (IsZip(filename))
                {
                    var acquired_time_file = slice_name + ".txt";
                    bool success_time = GetAcquisitionTime(acquired_time_file, out binary_trigger_time);
                    success = ExtractPhotonBinariesFromZip(slice_name);

                    if (!success && ArchiveLoosePhotonBinaries(slice_name, acquired_time_file, success_time, binary_trigger_time))
                    {
                        success_time = GetAcquisitionTime(acquired_time_file, out binary_trigger_time);
                        success = ExtractPhotonBinariesFromZip(slice_name);
                    }

                    return success && success_time;
                }
                else
                {
                    var acquired_time_file = slice_name + ".txt";
                    bool success_time = GetAcquisitionTime(acquired_time_file, out binary_trigger_time);
                    success = ExtractPhotonBinariesFromTar(slice_name);

                    if (!success && ArchiveLoosePhotonBinaries(slice_name, acquired_time_file, success_time, binary_trigger_time))
                    {
                        success_time = GetAcquisitionTime(acquired_time_file, out binary_trigger_time);
                        success = ExtractPhotonBinariesFromTar(slice_name);
                    }

                    return success && success_time;

                }
            }

            return true;
        }

        private sealed class StreamingZipIndex
        {
            internal readonly struct EntryInfo
            {
                public EntryInfo(string name, long dataOffset, long length)
                {
                    Name = name;
                    DataOffset = dataOffset;
                    Length = length;
                }

                public string Name { get; }
                public long DataOffset { get; }
                public long Length { get; }
            }

            private const uint LocalHeaderSignature = 0x04034b50;
            private const ushort DataDescriptorFlag = 0x0008;
            private const uint Zip64SizeMarker = 0xFFFFFFFF;

            private readonly string path;
            private readonly List<EntryInfo> entries;
            private readonly Dictionary<string, EntryInfo> entryMap;
            private readonly bool hasTruncatedEntry;

            private StreamingZipIndex(string path, List<EntryInfo> entries, bool hasTruncatedEntry)
            {
                this.path = path;
                this.entries = entries;
                this.hasTruncatedEntry = hasTruncatedEntry;
                entryMap = new Dictionary<string, EntryInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in entries)
                {
                    if (!entryMap.ContainsKey(entry.Name))
                        entryMap.Add(entry.Name, entry);
                }
            }

            public static StreamingZipIndex TryBuild(string path)
            {
                try
                {
                    var entries = ReadEntries(path, out bool hasTruncatedEntry);
                    if (entries == null || entries.Count == 0)
                        return null;
                    return new StreamingZipIndex(path, entries, hasTruncatedEntry);
                }
                catch
                {
                    return null;
                }
            }

            public IEnumerable<EntryInfo> Entries => entries;

            public bool TryGetEntry(string name, out EntryInfo entry) => entryMap.TryGetValue(name, out entry);

            public Stream OpenEntryStream(EntryInfo entry) => new SegmentStream(path, entry.DataOffset, entry.Length);

            public bool HasTruncatedEntry => hasTruncatedEntry;

            private static List<EntryInfo> ReadEntries(string path, out bool hasTruncatedEntry)
            {
                hasTruncatedEntry = false;
                var entries = new List<EntryInfo>();
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, CopyBufferSize, FileOptions.SequentialScan))
                using (var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true))
                {
                    while (fs.Position + 4 <= fs.Length)
                    {
                        uint sig = br.ReadUInt32();
                        if (sig != LocalHeaderSignature)
                            break;

                        br.ReadUInt16(); // version needed
                        ushort flags = br.ReadUInt16();
                        br.ReadUInt16(); // method
                        br.ReadUInt16(); // mod time
                        br.ReadUInt16(); // mod date
                        br.ReadUInt32(); // crc
                        uint compSize32 = br.ReadUInt32();
                        br.ReadUInt32(); // uncomp size
                        ushort nameLen = br.ReadUInt16();
                        ushort extraLen = br.ReadUInt16();

                        if (nameLen == 0 || fs.Position + nameLen + extraLen > fs.Length)
                            break;

                        byte[] nameBytes = br.ReadBytes(nameLen);
                        byte[] extra = br.ReadBytes(extraLen);
                        string name = Encoding.UTF8.GetString(nameBytes);

                        long dataStart = fs.Position;
                        if (!TryGetEntryDataRange(fs, flags, compSize32, extra, dataStart, out long dataLength, out long nextOffset, out bool truncated))
                            return null;

                        entries.Add(new EntryInfo(name, dataStart, dataLength));
                        if (truncated)
                            hasTruncatedEntry = true;
                        fs.Position = nextOffset;
                    }
                }

                return entries;
            }

            private static bool TryGetEntryDataRange(FileStream fs, ushort flags, uint compSize32, byte[] extra, long dataStart, out long dataLength, out long nextOffset, out bool truncated)
            {
                dataLength = -1;
                nextOffset = -1;
                truncated = false;

                bool hasDescriptor = (flags & DataDescriptorFlag) != 0;
                if (!hasDescriptor)
                {
                    if (compSize32 != Zip64SizeMarker)
                    {
                        dataLength = compSize32;
                        nextOffset = dataStart + dataLength;
                        return nextOffset <= fs.Length;
                    }

                    if (TryReadZip64Size(extra, out long compSize64))
                    {
                        dataLength = compSize64;
                        nextOffset = dataStart + dataLength;
                        return nextOffset <= fs.Length;
                    }

                    return false;
                }

                if (TryFindDataDescriptor(fs, dataStart, out dataLength, out nextOffset))
                    return true;

                if (TryFindNextLocalHeader(fs, dataStart, out dataLength, out nextOffset))
                    return true;

                if (dataStart >= fs.Length)
                    return false;

                dataLength = fs.Length - dataStart;
                nextOffset = fs.Length;
                truncated = true;
                return dataLength > 0;
            }

            private static bool TryReadZip64Size(byte[] extra, out long compSize)
            {
                compSize = -1;
                if (extra == null || extra.Length < 4)
                    return false;

                int offset = 0;
                while (offset + 4 <= extra.Length)
                {
                    ushort headerId = BitConverter.ToUInt16(extra, offset);
                    ushort dataSize = BitConverter.ToUInt16(extra, offset + 2);
                    offset += 4;
                    if (offset + dataSize > extra.Length)
                        break;

                    if (headerId == 0x0001 && dataSize >= 8)
                    {
                        ulong size = BitConverter.ToUInt64(extra, offset);
                        if (size <= long.MaxValue)
                        {
                            compSize = (long)size;
                            return true;
                        }
                        return false;
                    }
                    offset += dataSize;
                }

                return false;
            }

            private static bool TryFindDataDescriptor(FileStream fs, long dataStart, out long dataLength, out long nextOffset)
            {
                dataLength = -1;
                nextOffset = -1;
                const int BufferSize = 1024 * 1024;
                byte[] buffer = new byte[BufferSize];

                long scanPos = dataStart;
                int overlap = 3;

                while (scanPos + 16 < fs.Length)
                {
                    fs.Position = scanPos;
                    int read = fs.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;

                    for (int i = 0; i <= read - 4; i++)
                    {
                        if (buffer[i] == 0x50 && buffer[i + 1] == 0x4B && buffer[i + 2] == 0x07 && buffer[i + 3] == 0x08)
                        {
                            long descOffset = scanPos + i;
                            if (TryParseDataDescriptor(fs, descOffset, dataStart, out dataLength, out nextOffset)
                                && IsValidNextOffset(fs, nextOffset))
                            {
                                return true;
                            }
                        }
                    }

                    scanPos += read - overlap;
                }

                return false;
            }

            private static bool TryParseDataDescriptor(FileStream fs, long descOffset, long dataStart, out long dataLength, out long nextOffset)
            {
                dataLength = -1;
                nextOffset = -1;

                fs.Position = descOffset + 4;
                byte[] buffer = new byte[20];
                int read = fs.Read(buffer, 0, buffer.Length);
                if (read < 12)
                    return false;

                uint compSize32 = BitConverter.ToUInt32(buffer, 4);
                if (dataStart + compSize32 == descOffset)
                {
                    dataLength = compSize32;
                    nextOffset = descOffset + 16;
                    return nextOffset <= fs.Length;
                }

                if (read >= 20)
                {
                    ulong compSize64 = BitConverter.ToUInt64(buffer, 4);
                    if (compSize64 <= long.MaxValue && dataStart + (long)compSize64 == descOffset)
                    {
                        dataLength = (long)compSize64;
                        nextOffset = descOffset + 24;
                        return nextOffset <= fs.Length;
                    }
                }

                return false;
            }

            private static bool IsValidNextOffset(FileStream fs, long nextOffset)
            {
                if (nextOffset < 0 || nextOffset > fs.Length)
                    return false;
                if (nextOffset == fs.Length)
                    return true;
                if (nextOffset + 4 > fs.Length)
                    return false;

                byte[] sigBytes = new byte[4];
                fs.Position = nextOffset;
                int read = fs.Read(sigBytes, 0, sigBytes.Length);
                if (read < sigBytes.Length)
                    return false;

                uint sig = BitConverter.ToUInt32(sigBytes, 0);
                return sig == 0x04034b50 || sig == 0x02014b50 || sig == 0x06054b50;
            }

            private static bool TryFindNextLocalHeader(FileStream fs, long dataStart, out long dataLength, out long nextOffset)
            {
                dataLength = -1;
                nextOffset = -1;
                const int BufferSize = 1024 * 1024;
                byte[] buffer = new byte[BufferSize];
                long fileLength = fs.Length;

                long scanPos = Math.Min(fileLength, dataStart + 1);
                int overlap = 3;

                while (scanPos + 4 <= fileLength)
                {
                    fs.Position = scanPos;
                    int read = fs.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;

                    for (int i = 0; i <= read - 4; i++)
                    {
                        if (buffer[i] == 0x50 && buffer[i + 1] == 0x4B && buffer[i + 2] == 0x03 && buffer[i + 3] == 0x04)
                        {
                            long headerOffset = scanPos + i;
                            if (headerOffset <= dataStart)
                                continue;

                            if (IsPlausibleLocalHeader(fs, headerOffset, fileLength))
                            {
                                dataLength = headerOffset - dataStart;
                                nextOffset = headerOffset;
                                return dataLength > 0;
                            }
                        }
                    }

                    scanPos += read - overlap;
                }

                return false;
            }

            private static bool IsPlausibleLocalHeader(FileStream fs, long headerOffset, long fileLength)
            {
                if (headerOffset < 0 || headerOffset + 30 > fileLength)
                    return false;

                byte[] header = new byte[30];
                fs.Position = headerOffset;
                int read = fs.Read(header, 0, header.Length);
                if (read < header.Length)
                    return false;

                ushort nameLen = BitConverter.ToUInt16(header, 26);
                ushort extraLen = BitConverter.ToUInt16(header, 28);
                if (nameLen == 0)
                    return false;

                long nameStart = headerOffset + 30;
                long nameEnd = nameStart + nameLen + extraLen;
                if (nameEnd > fileLength)
                    return false;

                byte[] nameBytes = new byte[nameLen];
                fs.Position = nameStart;
                read = fs.Read(nameBytes, 0, nameBytes.Length);
                if (read < nameBytes.Length)
                    return false;

                for (int i = 0; i < nameBytes.Length; i++)
                {
                    if (nameBytes[i] == 0x00)
                        return false;
                }

                return true;
            }
        }

        private sealed class SegmentStream : Stream
        {
            private readonly FileStream stream;
            private readonly long start;
            private readonly long length;
            private long position;

            public SegmentStream(string path, long start, long length)
            {
                this.start = start;
                this.length = length;
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, CopyBufferSize, FileOptions.SequentialScan);
                stream.Seek(start, SeekOrigin.Begin);
                position = 0;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => length;
            public override long Position
            {
                get => position;
                set => Seek(value, SeekOrigin.Begin);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (position >= length)
                    return 0;

                long remaining = length - position;
                if (remaining < count)
                    count = (int)remaining;

                int read = stream.Read(buffer, offset, count);
                position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long newPos;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        newPos = offset;
                        break;
                    case SeekOrigin.Current:
                        newPos = position + offset;
                        break;
                    case SeekOrigin.End:
                        newPos = length + offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin));
                }

                if (newPos < 0 || newPos > length)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                stream.Seek(start + newPos, SeekOrigin.Begin);
                position = newPos;
                return position;
            }

            public override void Flush() { }

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    stream.Dispose();
                base.Dispose(disposing);
            }
        }

        private static byte[] ReadAllBytes(Stream stream, long length)
        {
            if (length < 0 || length > int.MaxValue)
                throw new InvalidDataException("Invalid binary data length.");

            int totalLength = (int)length;
            byte[] buffer = new byte[totalLength];
            int offset = 0;
            while (offset < totalLength)
            {
                int read = stream.Read(buffer, offset, totalLength - offset);
                if (read == 0)
                    break;
                offset += read;
            }

            if (offset != totalLength)
                throw new EndOfStreamException("Unexpected end of stream.");

            return buffer;
        }

        private static uint[] ConvertRawBytesToUInt32(byte[] rawBytes)
        {
            uint[] data = new uint[rawBytes.Length / 4];
            Buffer.BlockCopy(rawBytes, 0, data, 0, rawBytes.Length);
            return data;
        }

        private static void StreamUInt32ChunksFromStream(Stream stream, int bytesPerChunk, Action<uint[], bool> sendChunk)
        {
            byte[] buffer = new byte[bytesPerChunk];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bytesRead % sizeof(uint) != 0)
                    throw new InvalidDataException("Invalid binary data length.");

                int recordCount = bytesRead / sizeof(uint);
                uint[] chunk = new uint[recordCount];
                Buffer.BlockCopy(buffer, 0, chunk, 0, bytesRead);
                sendChunk(chunk, false);
            }

            sendChunk(Array.Empty<uint>(), true);
        }

        private static void ExtractEntryToFile(Stream entryStream, string targetPath)
        {
            using (entryStream)
            using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, CopyBufferSize))
            {
                entryStream.CopyTo(fileStream, CopyBufferSize);
            }
        }

        private bool ExtractPhotonBinariesFromZip(string sliceName)
        {
            bool useStreaming = !HasZipCentralDirectory();
            if (!useStreaming)
            {
                try
                {
                    using (ZipArchive archive = ZipFile.Open(filename, ZipArchiveMode.Read))
                    {
                        var file_list = archive.Entries.Where(e => e.FullName.Contains(".bin") && e.FullName.Contains(sliceName));

                        foreach (var entry in file_list)
                        {
                            var extracted_file_name1 = Path.Combine(dirname, entry.FullName);
                            if (!System.IO.File.Exists(extracted_file_name1))
                                ExtractEntryToFile(entry.Open(), extracted_file_name1);

                            extracted_file_name.Add(extracted_file_name1);
                        }
                    }
                }
                catch (InvalidDataException)
                {
                    InvalidateCentralDirectory();
                    useStreaming = true;
                }
            }

            if (useStreaming)
            {
                foreach (var entry in EnumerateStreamingEntries()
                    .Where(e => e.Name.IndexOf(".bin", StringComparison.OrdinalIgnoreCase) >= 0
                        && e.Name.IndexOf(sliceName, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var extracted_file_name1 = Path.Combine(dirname, entry.Name);
                    if (!System.IO.File.Exists(extracted_file_name1))
                    {
                        using (var stream = streamingZipIndex.OpenEntryStream(entry))
                        {
                            ExtractEntryToFile(stream, extracted_file_name1);
                        }
                    }

                    extracted_file_name.Add(extracted_file_name1);
                }
            }

            return extracted_file_name != null && extracted_file_name.Count > 0;
        }

        private bool ExtractPhotonBinariesFromTar(string sliceName)
        {
            using (var archive = TarArchive.OpenArchive(filename))
            {
                var file_list_tar = archive.Entries
                    .Where(e => !e.IsDirectory && e.Key.Contains(".bin") && e.Key.Contains(sliceName));

                foreach (var entry in file_list_tar)
                {
                    var extracted_file_name1 = Path.Combine(dirname, entry.Key);

                    if (!System.IO.File.Exists(extracted_file_name1))
                    {
                        ExtractEntryToFile(entry.OpenEntryStream(), extracted_file_name1);
                    }

                    extracted_file_name.Add(extracted_file_name1);
                }
            }

            return extracted_file_name != null && extracted_file_name.Count > 0;
        }

        private bool ArchiveLoosePhotonBinaries(string sliceName, string acquiredTimeFile, bool hasArchiveTime, DateTime archiveTime)
        {
            var searchPattern = sliceName + "*.bin";
            var filepaths = Directory.GetFiles(dirname, searchPattern);
            if (filepaths.Length == 0)
                return false;

            var stableFiles = filepaths.Where(WaitForStableFile).ToList();
            if (stableFiles.Count == 0)
                return false;

            DateTime acquiredTime = archiveTime;
            if (!hasArchiveTime)
            {
                var localTimePath = Path.Combine(dirname, acquiredTimeFile);
                if (TryReadAcquiredTimeFromFile(localTimePath, out DateTime localTime))
                {
                    acquiredTime = localTime;
                }
                else
                {
                    acquiredTime = DateTime.Now;
                }
            }

            if (IsZip(filename))
            {
                lock (syncZip)
                {
                    if (RequiresZipRebuild(stableFiles))
                    {
                        RebuildPhotonArchive(acquiredTimeFile, stableFiles, acquiredTime);
                        return true;
                    }

                    using (var zip = ZipFile.Open(filename, ZipArchiveMode.Update))
                    {
                        var existingHeader = zip.GetEntry(acquiredTimeFile);
                        existingHeader?.Delete();
                        CreateAcquiredTimeFile(acquiredTimeFile, zip, acquiredTime);

                        foreach (var file in stableFiles)
                        {
                            var entryName = Path.GetFileName(file);
                            var existingEntry = zip.GetEntry(entryName);
                            existingEntry?.Delete();
                            AddFileToZip(zip, file, GetCompressionLevelForFile(file));
                            System.IO.File.Delete(file);
                        }
                    }
                }

                return true;
            }

            string acqTimeText = "Acquired_Time = " + State.Acq.timeToString(acquiredTime);

            using (var fs = new FileStream(filename, FileMode.Open))
            using (var writer = WriterFactory.OpenWriter(fs, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
            {
                using (var tempStream = new MemoryStream())
                using (var streamWriter = new StreamWriter(tempStream, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    streamWriter.Write(acqTimeText);
                    streamWriter.Flush();
                    tempStream.Position = 0;
                    writer.Write(acquiredTimeFile, tempStream, null);
                }

                foreach (var file in stableFiles)
                {
                    using (var binStream = System.IO.File.OpenRead(file))
                    {
                        writer.Write(Path.GetFileName(file), binStream, null);
                    }

                    System.IO.File.Delete(file);
                }
            }

            return true;
        }

        private static void AddFileToZip(ZipArchive zip, string filePath, CompressionLevel level)
        {
            var entryName = Path.GetFileName(filePath);
            var entry = zip.CreateEntry(entryName, level);
            using (var entryStream = entry.Open())
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, CopyBufferSize, FileOptions.SequentialScan))
            {
                fileStream.CopyTo(entryStream, CopyBufferSize);
            }
        }

        private static CompressionLevel GetCompressionLevelForFile(string filePath)
        {
            try
            {
                var info = new FileInfo(filePath);
                if (info.Exists && info.Length > MaxZipUpdateEntryBytes)
                    return CompressionLevel.NoCompression;
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return CompressionLevel.Fastest;
        }

        private bool TryReadAcquiredTimeFromFile(string filePath, out DateTime dt)
        {
            dt = default;
            if (!System.IO.File.Exists(filePath))
                return false;

            foreach (var line in System.IO.File.ReadLines(filePath))
            {
                if (!line.Contains("Acquired_Time"))
                    continue;

                var str1 = line.Split('=')[1].Replace(" ", "");
                dt = DateTime.ParseExact(str1, State.Acq.datetime_formatter, null);
                return true;
            }

            return false;
        }

        private static bool WaitForStableFile(string path)
        {
            if (!System.IO.File.Exists(path))
                return false;

            long lastLength = -1;
            for (int attempt = 0; attempt < StableFileMaxAttempts; attempt++)
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, FileOptions.SequentialScan))
                    {
                        var length = stream.Length;
                        if (length > 0 && length == lastLength)
                            return true;
                        lastLength = length;
                    }
                }
                catch (IOException)
                {
                    // File may still be in use; retry.
                }
                catch (UnauthorizedAccessException)
                {
                    // File may still be locked; retry.
                }

                System.Threading.Thread.Sleep(StableFileCheckDelayMs);
            }

            return false;
        }

        private bool RequiresZipRebuild(IEnumerable<string> filepaths)
        {
            foreach (var file in filepaths)
            {
                var info = new FileInfo(file);
                if (info.Exists && info.Length > MaxZipUpdateEntryBytes)
                    return true;
            }

            return false;
        }

        private void RebuildPhotonArchive(string acqHeaderName, IEnumerable<string> filepaths, DateTime acquiredTime)
        {
            var tempZipPath = filename + ".tmp";

            if (System.IO.File.Exists(tempZipPath))
                System.IO.File.Delete(tempZipPath);

            var newEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                acqHeaderName
            };

            foreach (var file in filepaths)
            {
                if (!WaitForStableFile(file))
                    continue;
                newEntryNames.Add(Path.GetFileName(file));
            }

            using (var zip = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
            {
                if (System.IO.File.Exists(filename))
                {
                    using (var existingZip = ZipFile.OpenRead(filename))
                    {
                        foreach (var entry in existingZip.Entries)
                        {
                            if (newEntryNames.Contains(entry.FullName))
                                continue;

                            var newEntry = zip.CreateEntry(entry.FullName, compressionLevel);
                            newEntry.LastWriteTime = entry.LastWriteTime;
                            using (var sourceStream = entry.Open())
                            using (var destStream = newEntry.Open())
                            {
                                sourceStream.CopyTo(destStream);
                            }
                        }
                    }
                }

                CreateAcquiredTimeFile(acqHeaderName, zip, acquiredTime);

                foreach (var file in filepaths)
                {
                    if (!WaitForStableFile(file))
                        continue;
                    AddFileToZip(zip, file, GetCompressionLevelForFile(file));
                    System.IO.File.Delete(file);
                }
            }

            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Replace(tempZipPath, filename, null);
            }
            else
            {
                System.IO.File.Move(tempZipPath, filename);
            }
        }

        public bool PhotonFile_OpenFileHeader(out ScanParameters state1)
        {
            var fileIO = new FileIO(new ScanParameters());
            bool no_header = false;

            if (extension == State.Files.extension_photon)
            {
                fileIO.LoadSetupFile(filename);
            }
            else if (extension == State.Files.extension_photon_archive)
            {
                
                var photon_file = Path.GetFileNameWithoutExtension(filename) + State.Files.extension_photon;
                if (IsZip(filename))
                {
                    bool useStreaming = !HasZipCentralDirectory();
                    if (!useStreaming)
                    {
                        try
                        {
                            using (ZipArchive zip = ZipFile.Open(filename, ZipArchiveMode.Read))
                            {
                                file_list = zip.Entries.ToList();

                                var header_list = file_list.Where(e => e.FullName.Contains(State.Files.extension_photon)).ToList();
                                if (header_list.Count == 0)
                                {
                                    no_header = true;
                                }
                                else
                                {
                                    photon_file = header_list[0].FullName;
                                    filename_no_ext = Path.GetFileNameWithoutExtension(photon_file);

                                    var entry = zip.GetEntry(photon_file);

                                    StringBuilder SB = new StringBuilder();
                                    using (StreamReader sr = new StreamReader(entry.Open()))
                                    {
                                        string s = "";
                                        while ((s = sr.ReadLine()) != null)
                                        {
                                            try
                                            {
                                                fileIO.ExecuteLine(s);
                                                SB.AppendLine(s);
                                            }
                                            catch
                                            {
                                                Debug.WriteLine("Problem in " + s);
                                            }
                                        }
                                        sr.Close();
                                    }
                                    header_all = SB.ToString();
                                }
                            }
                        }
                        catch (InvalidDataException)
                        {
                            InvalidateCentralDirectory();
                            useStreaming = true;
                        }
                    }

                    if (useStreaming)
                    {
                        var header_entry = EnumerateStreamingEntries()
                            .FirstOrDefault(e => e.Name.EndsWith(State.Files.extension_photon, StringComparison.OrdinalIgnoreCase));

                        if (string.IsNullOrEmpty(header_entry.Name))
                        {
                            no_header = true;
                        }
                        else
                        {
                            photon_file = header_entry.Name;
                            filename_no_ext = Path.GetFileNameWithoutExtension(photon_file);

                            StringBuilder SB = new StringBuilder();
                            using (StreamReader sr = new StreamReader(streamingZipIndex.OpenEntryStream(header_entry)))
                            {
                                string s = "";
                                while ((s = sr.ReadLine()) != null)
                                {
                                    try
                                    {
                                        fileIO.ExecuteLine(s);
                                        SB.AppendLine(s);
                                    }
                                    catch
                                    {
                                        Debug.WriteLine("Problem in " + s);
                                    }
                                }
                                sr.Close();
                            }
                            header_all = SB.ToString();
                        }
                    }
                }
                else
                {
                    using (var archive = TarArchive.OpenArchive(filename))
                    {
                        var file_list = archive.Entries.Where(e => !e.IsDirectory).ToList();

                        var header_list = file_list
                            .Where(e => e.Key.EndsWith(State.Files.extension_photon, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (header_list.Count == 0)
                        {
                            no_header = true;
                        }
                        else
                        {
                            photon_file = header_list[0].Key;
                            filename_no_ext = Path.GetFileNameWithoutExtension(photon_file);

                            var entry = archive.Entries.First(e => e.Key == photon_file);

                            StringBuilder SB = new StringBuilder();
                            using (var sr = new StreamReader(entry.OpenEntryStream()))
                            {
                                string s = "";
                                while ((s = sr.ReadLine()) != null)
                                {
                                    try
                                    {
                                        fileIO.ExecuteLine(s);
                                        SB.AppendLine(s);
                                    }
                                    catch
                                    {
                                        Debug.WriteLine("Problem in " + s);
                                    }
                                }
                            }

                            header_all = SB.ToString();
                        }
                    }
                }
            }


            State = fileIO.State;
            state1 = State;
            return !no_header; // fileIO.State;
        }


    }

    public enum ImageFormat
    {
        TIFF = 1,
        AVI = 2,
    }
}
