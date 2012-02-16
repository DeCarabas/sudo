namespace sudo
{
    using System;
    using System.IO;

    public class FlushingWriter : TextWriter
    {
        public FlushingWriter() { }
        public FlushingWriter(TextWriter baseWriter)
        {
            this.BaseWriter = baseWriter;
        }

        public TextWriter BaseWriter { get; set; }

        public override System.Text.Encoding Encoding
        {
            get { return BaseWriter.Encoding; }
        }

        public override IFormatProvider FormatProvider
        {
            get
            {
                return BaseWriter.FormatProvider;
            }
        }

        public override string NewLine
        {
            get
            {
                return BaseWriter.NewLine;
            }
            set
            {
                BaseWriter.NewLine = value;
            }
        }

        public override void Close()
        {
            BaseWriter.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { BaseWriter.Dispose(); }
        }

        public override void Flush()
        {
            BaseWriter.Flush();
        }

        public override void Write(bool value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(char value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(char[] buffer)
        {
            BaseWriter.Write(buffer);
            BaseWriter.Flush();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            BaseWriter.Write(buffer, index, count);
            BaseWriter.Flush();
        }

        public override void Write(decimal value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(double value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(float value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(int value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(long value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(object value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(string format, object arg0)
        {
            BaseWriter.Write(format, arg0);
            BaseWriter.Flush();
        }

        public override void Write(string format, object arg0, object arg1)
        {
            BaseWriter.Write(format, arg0, arg1);
            BaseWriter.Flush();            
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            BaseWriter.Write(format, arg0, arg1, arg2);
            BaseWriter.Flush();            
        }

        public override void Write(string format, params object[] arg)
        {
            BaseWriter.Write(format, arg);
            BaseWriter.Flush();
        }

        public override void Write(string value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(uint value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void Write(ulong value)
        {
            BaseWriter.Write(value);
            BaseWriter.Flush();
        }

        public override void WriteLine()
        {
            BaseWriter.WriteLine();
            BaseWriter.Flush();
        }

        public override void WriteLine(bool value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(char value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(char[] buffer)
        {
            BaseWriter.WriteLine(buffer);
            BaseWriter.Flush();
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            BaseWriter.WriteLine(buffer, index, count);
            BaseWriter.Flush();
        }

        public override void WriteLine(decimal value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(double value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(float value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(int value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(long value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(object value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(string format, object arg0)
        {
            BaseWriter.WriteLine(format, arg0);
            BaseWriter.Flush();
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            BaseWriter.WriteLine(format, arg0, arg1);
            BaseWriter.Flush();            
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            BaseWriter.WriteLine(format, arg0, arg1, arg2);
            BaseWriter.Flush();            
        }

        public override void WriteLine(string format, params object[] arg)
        {
            BaseWriter.WriteLine(format, arg);
            BaseWriter.Flush();
        }

        public override void WriteLine(string value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(uint value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }

        public override void WriteLine(ulong value)
        {
            BaseWriter.WriteLine(value);
            BaseWriter.Flush();
        }
    }
}
