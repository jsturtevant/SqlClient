using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImportsWorld;
using ImportsWorld.wit.imports.wasi.io.v0_2_1;
using ImportsWorld.wit.imports.wasi.sockets.v0_2_1;

namespace Microsoft.Data.SqlClient.Wasi
{
    internal class SslStream : Stream
    {
        private static string authErrorMessage =
            "This operation is only allowed using a successfully authenticated context.";

        private Stream plainStream;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private Stream? cipherStream;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

        public SslStream(Stream plainStream)
        {
            this.plainStream = plainStream;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotImplementedException();
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public new void Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            plainStream.Dispose();
            cipherStream?.Dispose();
        }

        public void AuthenticateAsClient(string host){
            WasiEventLoop.RunAsync(() => AuthenticateAsClientAsync(host));
        }

        public async Task AuthenticateAsClientAsync(string host)
        {
            IStreams.InputStream plainInput;
            IStreams.OutputStream plainOutput;
            if (plainStream is NetworkStream networkStream)
            {
                plainInput = (networkStream as NetworkStream).input;
                plainOutput = networkStream.output;
            }
            else
            {
                // TODO: we'll need to add a `wasi:io/streams#pipe` function and use
                // it to support other types of streams
                throw new NotSupportedException("TODO: non-`NetworkStream` streams not yet supported");
            }

            using var future = ITls.ClientHandshake.Finish(
                new ITls.ClientConnection(plainInput, plainOutput).Connect(host)
            );
            while (true)
            {
                var result = future.Get();
                if (result is not null)
                {
                    var inner = (
                        (Result<Result<(IStreams.InputStream, IStreams.OutputStream), None>, None>)
                            result!
                    ).AsOk;
                    if (inner.IsOk)
                    {
                        var (input, output) = inner.AsOk;
                        cipherStream = new NetworkStream(input, output);
                        break;
                    }
                    else
                    {   
                        throw new Exception($"TLS handshake failed on hostside: {inner.AsErr.ToString()}");
                    }
                }
                else
                {
                    await WasiEventLoop.Register(future.Subscribe(), CancellationToken.None);
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            // ignore
        }

        public override void SetLength(long length)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            if (cipherStream is not null)
            {
                return cipherStream.Read(buffer, offset, length);
            }
            else
            {
                throw new InvalidOperationException(authErrorMessage);
            }
        }

        public override void Write(byte[] buffer, int offset, int length)
        {
            if (cipherStream is not null)
            {
                cipherStream.Write(buffer, offset, length);
            }
            else
            {
                throw new InvalidOperationException(authErrorMessage);
            }
        }

        public override Task<int> ReadAsync(
            byte[] bytes,
            int offset,
            int length,
            CancellationToken cancellationToken
        )
        {
            if (cipherStream is not null)
            {
                return cipherStream.ReadAsync(bytes, offset, length, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException(authErrorMessage);
            }
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        )
        {
            if (cipherStream is not null)
            {
                return cipherStream.ReadAsync(buffer, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException(authErrorMessage);
            }
        }

        public override Task WriteAsync(
            byte[] bytes,
            int offset,
            int length,
            CancellationToken cancellationToken
        )
        {
            if (cipherStream is not null)
            {
                return cipherStream.WriteAsync(bytes, offset, length, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException(authErrorMessage);
            }
        }

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default
        )
        {
            if (cipherStream is not null)
            {
                return cipherStream.WriteAsync(buffer, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException(authErrorMessage);
            }
        }
    }
}