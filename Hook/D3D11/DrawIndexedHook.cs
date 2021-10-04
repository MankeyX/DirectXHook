using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.Memory;
using Core.Models;
using Hook.D3D11.Extensions;
using SharpDX.Direct3D11;

namespace Hook.D3D11
{
    public class DrawIndexedHook : IDisposable
    {
        private const int IndexByteWidthDivider = 100;

        private readonly DeviceContext _deviceContext;
        private readonly FunctionHook<DrawIndexed> _drawIndexedHook;
        private readonly DepthStencilState _depthDisabledState;
        private readonly List<ModelInfo> _modelsInScene;
        private readonly Shaders _shaders;
        private readonly ServerInterface _serverInterface;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate void DrawIndexed(IntPtr deviceContextPointer, int indexCount, int startIndex, int baseVertexLocation);

        private int _currentIndex;
        private int _selectedByteWidth;
        private ModelInfo _currentItem;

        public bool OnlyRenderSavedModels { get; private set; }
        public bool IsLoggerEnabled { get; private set; }
        public List<ModelInfo> SavedModels { get; set; }

        public DrawIndexedHook(DeviceContext deviceContext, DepthStencilState depthDisabledState, Shaders shaders, ServerInterface serverInterface)
        {
            _deviceContext = deviceContext;
            _depthDisabledState = depthDisabledState;
            _shaders = shaders;
            _serverInterface = serverInterface;
            _modelsInScene = new List<ModelInfo>();
            SavedModels = new List<ModelInfo>();

            _drawIndexedHook = new FunctionHook<DrawIndexed>(
                VirtualTableAddress.GetVtableAddress(_deviceContext.NativePointer, (int) VirtualTableIndices.D3D11DeviceContext.DrawIndexed),
                new DrawIndexed(OnDrawIndexed),
                this);

            _serverInterface.Message("DrawIndexedHook.ctor");
            _drawIndexedHook.Activate();
        }

        private void OnDrawIndexed(IntPtr deviceContextPointer, int indexCount, int startIndex, int baseVertexLocation)
        {
            if (indexCount < 7)
            {
                _drawIndexedHook.OriginalFunction(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
                return;
            }
            
            _drawIndexedHook.OriginalFunction(deviceContextPointer, indexCount, startIndex, baseVertexLocation);

            using (var defaultDepthState = _deviceContext.OutputMerger.GetDepthStencilState(out var stencilRef))
            {
                _currentItem = GetCurrentItem(indexCount);

                if (IsLoggerEnabled)
                {
                    _deviceContext.OutputMerger.SetDepthStencilState(_depthDisabledState, stencilRef);
                    if (_selectedByteWidth == _deviceContext.GetIndexByteWidth() / IndexByteWidthDivider)
                    {
                        if (!_modelsInScene.Contains(_currentItem))
                            _modelsInScene.Add(_currentItem);

                        if (IsCurrentModel(_currentItem, _currentIndex))
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
                    if (IsSavedModel(_currentItem, out var index))
                    {
                        _deviceContext.OutputMerger.SetDepthStencilState(_depthDisabledState, stencilRef);
                        DrawCustom(indexCount, startIndex, baseVertexLocation, SavedModels[index].Color);
                    }
                }
                
                _deviceContext.OutputMerger.SetDepthStencilState(defaultDepthState, stencilRef);
            }
        }

        private void DrawCustom(int indexCount, int startIndex, int baseVertexLocation, Color color)
        {
            // Disable depth testing so we can see the object through walls, and color it with a shader to make it more visible
            _deviceContext.PixelShader.SetShader(_shaders.Get(color), null, 0);
            _deviceContext.DrawIndexed(indexCount, startIndex, baseVertexLocation);
        }

        private ModelInfo GetCurrentItem(int indexCount)
        {
            return _deviceContext.GetModelInfo(indexCount);
        }

        private bool IsCurrentModel(ModelInfo currentModel, int currentIndex)
        {
            return
                _modelsInScene[currentIndex] != null &&
                _modelsInScene[currentIndex].Equals(currentModel);
        }

        private bool IsSavedModel(ModelInfo model, out int savedItemIndex)
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
            IsLoggerEnabled = !IsLoggerEnabled;

            if (!IsLoggerEnabled)
            {
                _modelsInScene.Clear();
                _serverInterface.Message("DrawIndexedHook.ToggleLogger False");
            }

            return IsLoggerEnabled;
        }

        public void RenderSavedModelsOnly(bool enabled)
        {
            OnlyRenderSavedModels = enabled;
        }

        public ModelInfo GetSelectedModel()
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