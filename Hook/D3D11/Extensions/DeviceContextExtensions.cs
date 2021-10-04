using Core.Models;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Hook.D3D11.Extensions
{
    public static class DeviceContextExtensions
    {
        public static ModelInfo GetModelInfo(this DeviceContext deviceContext, int indexCount)
        {
            var vertexBuffers = new Buffer[1];
            var strides = new int[1];
            var offsets = new int[1];

            deviceContext.InputAssembler.GetIndexBuffer(out var buffer, out _, out _);
            deviceContext.InputAssembler.GetVertexBuffers(0, 1, vertexBuffers, strides, offsets);
            
            using (vertexBuffers[0])
            using (buffer)
            using (var resources = deviceContext.PixelShader.GetShaderResources(0, 1)?[0])
            {
                ModelInfo modelInfo;
            
                if (resources == null)
                    modelInfo = new ModelInfo(
                        indexCount,
                        buffer.Description.SizeInBytes,
                        strides[0],
                        vertexBuffers[0].Description.SizeInBytes,
                        0,
                        Color.White);
                else
                    modelInfo = new ModelInfo(
                        indexCount,
                        buffer.Description.SizeInBytes,
                        strides[0],
                        vertexBuffers[0].Description.SizeInBytes,
                        (int)resources.Description.Format,
                        Color.White);

                return modelInfo;
            }
        }

        public static int GetIndexByteWidth(this DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.GetIndexBuffer(out var buffer, out _, out _);
            
            using (buffer)
                return buffer.Description.SizeInBytes;
        }
    }
}
