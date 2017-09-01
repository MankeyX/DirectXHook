using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Hook.D3D11.Extensions;
using Hook.Infrastructure;
using SharpDX.Direct3D11;

namespace Hook.D3D11
{
    public class DrawIndexedHook : IDisposable
    {
        private readonly DeviceContext _deviceContext;
        private readonly FunctionHook<DrawIndexed> _drawIndexedHook;
        private readonly DepthStencilState _depthDisabledState;
        private readonly List<ModelParameters> _modelsInScene;
        private readonly Shaders _shaders;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate void DrawIndexed(IntPtr deviceContextPointer, int indexCount, int startIndex, int baseVertexLocation);

        private int _currentIndex;
        private int _selectedByteWidth;

        public bool OnlyRenderSavedModels { get; private set; }
        public bool IsLoggerEnabled { get; private set; }
        public List<ModelParameters> SavedModels { get; set; }

        public DrawIndexedHook(DeviceContext deviceContext, DepthStencilState depthDisabledState, Shaders shaders)
        {
            _deviceContext = deviceContext;
            _depthDisabledState = depthDisabledState;
            _shaders = shaders;
            _modelsInScene = new List<ModelParameters>();
            SavedModels = new List<ModelParameters>();

            _drawIndexedHook = new FunctionHook<DrawIndexed>(
                Memory.GetVtableAddress(_deviceContext.NativePointer, (int)VTableIndices.D3D11DeviceContext.DrawIndexed),
                new DrawIndexed(OnDrawIndexed),
                this);

            _drawIndexedHook.Activate();
        }

        private void OnDrawIndexed(IntPtr deviceContextPointer, int indexCount, int startIndex, int baseVertexLocation)
        {
            if (indexCount < 7)
            {
                _drawIndexedHook.OriginalFunction(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
                return;
            }

            if (!OnlyRenderSavedModels)
                _drawIndexedHook.OriginalFunction(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
            
            var currentItem = GetCurrentItem(indexCount);

            if (IsLoggerEnabled)
            {
                if (_selectedByteWidth == _deviceContext.GetIndexByteWidth() / 500)
                {
                    if (!_modelsInScene.Contains(currentItem))
                        _modelsInScene.Add(currentItem);

                    if (IsCurrentModel(currentItem, _currentIndex))
                    {
                        DrawCustom(indexCount, startIndex, baseVertexLocation, Color.Red);
                        return;
                    }

                    DrawCustom(indexCount, startIndex, baseVertexLocation, Color.White);
                    return;
                }
            }

            lock (SavedModels)
            {
                if (IsSavedModel(currentItem, out var index))
                {
                    DrawCustom(indexCount, startIndex, baseVertexLocation, SavedModels[index].Color);
                }
            }
        }

        private void DrawCustom(int indexCount, int startIndex, int baseVertexLocation, Color color)
        {
            _deviceContext.DrawIndexed(indexCount, startIndex, baseVertexLocation);

            // Disable depth testing so we can see the object through walls, and color it with a shader to make it more visible
            _deviceContext.PixelShader.SetShader(_shaders.Get(color), null, 0);
            _deviceContext.OutputMerger.SetDepthStencilState(_depthDisabledState);
            _deviceContext.DrawIndexed(indexCount, startIndex, baseVertexLocation);

            // Reset the depth stencil state to the default values
            _deviceContext.OutputMerger.SetDepthStencilState(null);
        }

        private ModelParameters GetCurrentItem(int indexCount)
        {
            return _deviceContext.GetModelParameters(indexCount);
        }

        private bool IsCurrentModel(ModelParameters currentModel, int currentIndex)
        {
            return
                _modelsInScene[currentIndex] != null &&
                _modelsInScene[currentIndex].Equals(currentModel);
        }
        
        private bool IsSavedModel(ModelParameters model, out int savedItemIndex)
        {
            return (savedItemIndex = SavedModels.BinarySearch(model)) >= 0;
        }

        public int ChangeModel(int change)
        {
            // Wrap for easy navigation
            if (_currentIndex + change < 0)
                _currentIndex = _modelsInScene.Count - 1;
            else if (_currentIndex + change >= _modelsInScene.Count)
                _currentIndex = 0;
            else
                _currentIndex += change;

            return _currentIndex;
        }

        public int ChangeSelectedByteWidth(int change)
        {
            _selectedByteWidth += _selectedByteWidth + change < 0
                ? 0
                : change;

            _currentIndex = 0;

            return _selectedByteWidth;
        }

        public bool ToggleLogger()
        {
            _selectedByteWidth = 0;
            _currentIndex = 0;
            return IsLoggerEnabled = !IsLoggerEnabled;
        }

        public void RenderSavedModelsOnly(bool enabled)
        {
            OnlyRenderSavedModels = enabled;
        }

        public ModelParameters GetSelectedModel()
        {
            return _modelsInScene[_currentIndex];
        }

        public void Dispose()
        {
            _drawIndexedHook?.Deactivate();
            _drawIndexedHook?.Dispose();
            _depthDisabledState?.Dispose();

            // Don't dispose _deviceContext, it will crash the application or game
        }
    }
}
