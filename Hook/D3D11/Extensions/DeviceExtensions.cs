using System.Globalization;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;

namespace Hook.D3D11.Extensions
{
    public static class DeviceExtensions
    {
        public static ID3D11DepthStencilState CreateDepthStencilState(this ID3D11Device device)
        {
            var description = new DepthStencilDescription
            {
                StencilEnable = false,
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunction.Always
            };

            return device.CreateDepthStencilState(description);
        }

        public static ID3D11BlendState CreateBlendState(this ID3D11Device device)
        {
            // Blending disabled, writing all color channels
            return device.CreateBlendState(BlendDescription.Opaque);
        }

        public static ID3D11PixelShader GenerateShader(this ID3D11Device device, float r, float g, float b)
        {
            var shaderText =
$@"
float4 main() : SV_TARGET0
{{
    return float4(
        {r.ToString(CultureInfo.InvariantCulture)},
        {g.ToString(CultureInfo.InvariantCulture)},
        {b.ToString(CultureInfo.InvariantCulture)},
        1
    );
}}";
            
            var pixelShaderBytecode = Compiler.Compile(shaderText, "main", "PixelShader", "ps_4_0");
            return device.CreatePixelShader(pixelShaderBytecode.Span);
        }
    }
}
