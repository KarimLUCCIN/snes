using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.DXGI;
using System.Windows.Forms;
using SlimDX;

namespace SnesEmulator.Renderer
{
    public class RenderEngine : IDisposable
    {
        private Form hwndRenderingWindow;

        public RenderEngine()
        {

        }

        /// <summary>
        /// Crée le Device DirectX 10 et initialise les resources
        /// </summary>
        public void Initialize()
        {
            Dispose(true);

            hwndRenderingWindow = new Form();
            hwndRenderingWindow.Width = hwndRenderingWindow.Height = 100;

            var description = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = hwndRenderingWindow.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };
        }

        #region IDisposable Members

        ~RenderEngine()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clean();

                GC.SuppressFinalize(this);
            }
        }

        private void Clean()
        {
            if (hwndRenderingWindow != null)
            {
                hwndRenderingWindow.Dispose();
                hwndRenderingWindow = null;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
