using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Hook.D3D11.Extensions
{
    public static class DeviceContextExtensions
    {
        public static ModelParameters GetModelParameters(this DeviceContext deviceContext, int indexCount)
        {
            var vertexBuffers = new Buffer[1];
            var strides = new int[1];
            var offsets = new int[1];

            deviceContext.InputAssembler.GetIndexBuffer(out var buffer, out var format, out var offset);
            deviceContext.InputAssembler.GetVertexBuffers(0, 1, vertexBuffers, strides, offsets);
            var resources = deviceContext.PixelShader.GetShaderResources(0, 1);

            if (resources[0] == null)
                return new ModelParameters(
                    indexCount,
                    buffer.Description.SizeInBytes,
                    strides[0],
                    vertexBuffers[0].Description.SizeInBytes,
                    Format.A8P8,
                    Color.White);

            return new ModelParameters(
                indexCount,
                buffer.Description.SizeInBytes,
                strides[0],
                vertexBuffers[0].Description.SizeInBytes,
                resources[0].Description.Format,
                Color.White);
        }

        public static int GetIndexByteWidth(this DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.GetIndexBuffer(out var buffer, out var format, out var offset);
            return buffer.Description.SizeInBytes;
        }
    }
}
