﻿using System.IO;

namespace SharpBSABA2.BA2Util
{
    public class BA2FileEntry : ArchiveEntry
    {
        public uint flags { get; set; }
        public uint align { get; set; }

        public override bool Compressed
        {
            get { return this.Size != 0; }
        }
        public override uint DisplaySize
        {
            get
            {
                return this.RealSize;
            }
        }

        public BA2FileEntry(Archive ba2) : base(ba2)
        {
            nameHash = ba2.BinaryReader.ReadUInt32();
            Extension = new string(ba2.BinaryReader.ReadChars(4));
            dirHash = ba2.BinaryReader.ReadUInt32();

            FullPath = dirHash > 0 ? $"{dirHash:X}_" : string.Empty;
            FullPath += $"{nameHash:X}.{Extension.TrimEnd('\0')}";
            FullPathOriginal = FullPath;

            flags = ba2.BinaryReader.ReadUInt32();
            Offset = ba2.BinaryReader.ReadUInt64();
            Size = ba2.BinaryReader.ReadUInt32();
            RealSize = ba2.BinaryReader.ReadUInt32();
            align = ba2.BinaryReader.ReadUInt32();
        }

        public override MemoryStream GetRawDataStream()
        {
            var ms = new MemoryStream();

            this.WriteDataToStream(ms, false);

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public override string GetToolTipText()
        {
            return $"{nameof(nameHash)}: {nameHash}\n" +
                $"{nameof(FullPath)}: {FullPath}\n" +
                $"{nameof(Extension)}: {Extension.TrimEnd('\0')}\n" +
                $"{nameof(dirHash)}: {dirHash}\n" +
                $"{nameof(flags)}: {flags}\n" +
                $"{nameof(Offset)}: {Offset}\n" +
                $"{nameof(Size)}: {Size}\n" +
                $"{nameof(RealSize)}: {RealSize}\n" +
                $"{nameof(align)}: {align}";
        }

        protected override void WriteDataToStream(Stream stream)
        {
            this.WriteDataToStream(stream, true);
        }

        protected void WriteDataToStream(Stream stream, bool decompress)
        {
            uint len = this.Compressed ? this.Size : this.RealSize;
            BinaryReader.BaseStream.Seek((long)this.Offset, SeekOrigin.Begin);

            if (!decompress || !this.Compressed)
            {
                Archive.WriteSectionToStream(BinaryReader.BaseStream,
                                             len,
                                             stream,
                                             bytesWritten => this.BytesWritten = bytesWritten);
            }
            else
            {
                Archive.Decompress(BinaryReader.BaseStream,
                                   len,
                                   stream,
                                   bytesWritten => this.BytesWritten = bytesWritten);
            }
        }
    }
}
