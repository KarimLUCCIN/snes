﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using SharpDX.Direct3D9;
using System.Windows;

namespace SnesEmulator.Renderer
{
    class DX10ImageSource : D3DImage, IDisposable
    {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();
        private static int ActiveClients;
        private static Direct3DEx D3DContext;
        private static DeviceEx D3DDevice;
        private Texture RenderTarget;

        public DX10ImageSource()
        {
            this.StartD3D();
            DX10ImageSource.ActiveClients++;
        }

        public void Dispose()
        {
            this.SetRenderTargetDX10(null);
            Disposer.SafeDispose(ref this.RenderTarget);

            DX10ImageSource.ActiveClients--;
            this.EndD3D();
        }

        public void InvalidateD3DImage()
        {
            if (this.RenderTarget != null)
            {
                base.Lock();
                base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                base.Unlock();
            }
        }

        public void SetRenderTargetDX10(SharpDX.Direct3D10.Texture2D renderTarget)
        {
            if (this.RenderTarget != null)
            {
                this.RenderTarget = null;

                base.Lock();
                base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                base.Unlock();
            }

            if (renderTarget == null)
                return;

            if (!IsShareable(renderTarget))
                throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");

            Format format = DX10ImageSource.TranslateFormat(renderTarget);
            if (format == Format.Unknown)
                throw new ArgumentException("Texture format is not compatible with OpenSharedResource");

            IntPtr handle = GetSharedHandle(renderTarget);
            if (handle == IntPtr.Zero)
                throw new ArgumentNullException("Handle");

            this.RenderTarget = new Texture(DX10ImageSource.D3DDevice, renderTarget.Description.Width, renderTarget.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle);
            using (Surface surface = this.RenderTarget.GetSurfaceLevel(0))
            {
                base.Lock();
                base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                base.Unlock();
            }
        }

        private void StartD3D()
        {
            if (DX10ImageSource.ActiveClients != 0)
                return;

            D3DContext = new Direct3DEx();

            PresentParameters presentparams = new PresentParameters();
            presentparams.Windowed = true;
            presentparams.SwapEffect = SwapEffect.Discard;
            presentparams.DeviceWindowHandle = GetDesktopWindow();
            presentparams.PresentationInterval = PresentInterval.Default;

            DX10ImageSource.D3DDevice = new DeviceEx(D3DContext, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
        }

        private void EndD3D()
        {
            if (DX10ImageSource.ActiveClients != 0)
                return;

            Disposer.SafeDispose(ref this.RenderTarget);
            Disposer.SafeDispose(ref DX10ImageSource.D3DDevice);
            Disposer.SafeDispose(ref DX10ImageSource.D3DContext);
        }

        private IntPtr GetSharedHandle(SharpDX.Direct3D10.Texture2D Texture)
        {
            SharpDX.DXGI.Resource resource = Texture.QueryInterface<SharpDX.DXGI.Resource>();
            IntPtr result = resource.SharedHandle;
            resource.Dispose();
            return result;
        }

        private static Format TranslateFormat(SharpDX.Direct3D10.Texture2D Texture)
        {
            switch (Texture.Description.Format)
            {
                case SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                    return SharpDX.Direct3D9.Format.A2B10G10R10;

                case SharpDX.DXGI.Format.R16G16B16A16_Float:
                    return SharpDX.Direct3D9.Format.A16B16G16R16F;

                case SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    return SharpDX.Direct3D9.Format.A8R8G8B8;

                default:
                    return SharpDX.Direct3D9.Format.Unknown;
            }
        }

        private static bool IsShareable(SharpDX.Direct3D10.Texture2D Texture)
        {
            return (Texture.Description.OptionFlags & SharpDX.Direct3D10.ResourceOptionFlags.Shared) != 0;
        }
    }
}
