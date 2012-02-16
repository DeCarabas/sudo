namespace sudo
{
    using System;
    using System.IO;
    using Microsoft.Win32.SafeHandles;

    class SimpleFileStream : Stream
    {
        SafeFileHandle fileHandle;

        public SimpleFileStream(SafeFileHandle fileHandle)
        {
            this.fileHandle = fileHandle;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { this.fileHandle.Close(); }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return NativeMethods.ReadFile(this.fileHandle, buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            NativeMethods.WriteFile(this.fileHandle, buffer, offset, count);
        }
    }
}
