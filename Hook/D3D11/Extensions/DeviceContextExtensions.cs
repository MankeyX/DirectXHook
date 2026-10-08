using Core.Models;
using Vortice.Direct3D11;

namespace Hook.D3D11.Extensions
{
    public static class DeviceContextExtensions
    {
        public static ModelInfo GetModelInfo(this ID3D11DeviceContext deviceContext, int indexCount)
        {
            var vertexBuffers = new ID3D11Buffer[1];
            var strides = new uint[1];
            var offsets = new uint[1];
            var shaderResources = new ID3D11ShaderResourceView[1];

            deviceContext.IAGetIndexBuffer(out var buffer, out _, out _);
            deviceContext.IAGetVertexBuffers(0, 1, vertexBuffers, strides, offsets);
            deviceContext.PSGetShaderResources(0, 1, shaderResources);
            
            using (vertexBuffers[0])
            using (buffer)
            using (var resources = shaderResources[0])
            {
                ModelInfo modelInfo;
            
                if (resources == null)
                    modelInfo = new ModelInfo(
                        indexCount,
                        (int)buffer.Description.ByteWidth,
                        (int)strides[0],
                        (int)vertexBuffers[0].Description.ByteWidth,
                        0,
                        Color.White);
                else
                    modelInfo = new ModelInfo(
                        indexCount,
                        (int)buffer.Description.ByteWidth,
                        (int)strides[0],
                        (int)vertexBuffers[0].Description.ByteWidth,
                        (int)resources.Description.Format,
                        Color.White);

                return modelInfo;
            }
        }

        public static int GetIndexByteWidth(this ID3D11DeviceContext deviceContext)
        {
            deviceContext.IAGetIndexBuffer(out var buffer, out _, out _);
            
            using (buffer)
                return (int)buffer.Description.ByteWidth;
        }
    }
}
