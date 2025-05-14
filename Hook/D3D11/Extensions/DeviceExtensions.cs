using System.Globalization;
using System.Text;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Hook.D3D11.Extensions
{
    public static class DeviceExtensions
    {
        public static DepthStencilState CreateDepthStencilState(this Device device)
        {
            var description = new DepthStencilStateDescription
            {
                IsStencilEnabled = false,
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Always
            };

            return new DepthStencilState(device, description);
        }

        public static BlendState CreateBlendState(this Device device)
        {
            var description = new BlendStateDescription();

            description.RenderTarget[0].IsBlendEnabled = true;
            description.RenderTarget[0].SourceBlend = BlendOption.One;
            description.RenderTarget[0].DestinationBlend = BlendOption.One;
            description.RenderTarget[0].BlendOperation = BlendOperation.Add;
            description.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            description.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            description.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            description.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            return new BlendState(device, description);
        }

        public static PixelShader GenerateShader(this Device device, float r, float g, float b)
        {
            var shaderText = Encoding.ASCII.GetBytes(
$@"
float4 main() : SV_TARGET0
{{
    return float4(
        {r.ToString(CultureInfo.InvariantCulture)},
        {g.ToString(CultureInfo.InvariantCulture)},
        {b.ToString(CultureInfo.InvariantCulture)},
        0.3f
    );
}}");
            
            using (var pixelShaderBytecode = ShaderBytecode.Compile(shaderText, "main", "ps_4_0"))
            {
                return new PixelShader(device, pixelShaderBytecode);
            }
        }
    }
}
