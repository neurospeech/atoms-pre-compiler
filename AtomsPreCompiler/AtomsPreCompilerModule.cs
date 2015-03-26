using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NeuroSpeech.AtomsPreCompiler
{
    public class AtomsPreCompilerModule : IHttpModule
    {

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {

            context.BeginRequest += (s, e) =>
            {

                context.Response.Filter = new AtomPreCompilerStream(context.Response.Filter, context.Context);

            };
        }

    }

    public class AtomPreCompilerStream : AtomPreCompilerFilterStream
    {

        private HttpContext context;

        public AtomPreCompilerStream(Stream s, HttpContext c)
            : base(s)
        {
            this.context = c;
        }

        protected override byte[] ProcessBuffer(byte[] p)
        {
            string html = "";
            System.Text.Encoding encoding = System.Text.Encoding.Default;
            try
            {
                if (context.Response.ContentType.EqualsIgnoreCase("text/html"))
                {
                    NeuroSpeech.AtomsPreCompiler.PageCompiler page = new NeuroSpeech.AtomsPreCompiler.PageCompiler();
                    page.Debug = true;
                    encoding = context.Response.ContentEncoding ?? System.Text.Encoding.Default;
                    html = encoding.GetString(p);
                    var result = page.Compile(html);
                    html = result.Document;
                    return encoding.GetBytes(html);
                }
            }
            catch (Exception ex)
            {
                html = html + "\r\n<!--" + ex.ToString() + "-->";
                return encoding.GetBytes(html);
            }
            return p;
        }
    }

    public class AtomPreCompilerFilterStream : Stream
    {
        /// <summary>
        /// The original stream
        /// </summary>
        Stream _stream;


        /// <summary>
        /// Stream that original content is read into
        /// and then passed to TransformStream function
        /// </summary>
        MemoryStream _cacheStream = new MemoryStream(5000);

        public byte[] Buffer
        {
            get
            {
                return _cacheStream.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseStream"></param>
        public AtomPreCompilerFilterStream(Stream responseStream)
        {
            _stream = responseStream;
        }


        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Length
        {
            get { return 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            //return _stream.Seek(offset, direction);
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public override void SetLength(long length)
        {
            //_stream.SetLength(length);
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Close()
        {
            byte[] data = ProcessBuffer(_cacheStream.ToArray());
            _stream.Write(data, 0, data.Length);
            _stream.Close();
        }

        protected virtual byte[] ProcessBuffer(byte[] p)
        {
            return p;
        }

        /// <summary>
        /// Override flush by writing out the cached stream data
        /// </summary>
        public override void Flush()
        {

            // default flush behavior
            //_stream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }


        /// <summary>
        /// Overriden to capture output written by ASP.NET and captured
        /// into a cached stream that is written out later when Flush()
        /// is called.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _cacheStream.Write(buffer, offset, count);
            //_stream.Write(buffer, offset, count);

        }

    }

}
