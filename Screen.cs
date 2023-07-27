﻿using screenshot_testing.WindowsApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace screenshot_testing
{
    class ScreenCapture : IDisposable
    {
        public const float defaultWindowsDPI = 96f;
        public const int SourceCopyPixelOperation = 0x00CC0020;

        private readonly Rectangle captureRect;
        public Rectangle CaptureRect { get => captureRect; }

        public ScreenCapture(int screen)
        {
            Rectangle screenBounds = Screen.AllScreens[screen].Bounds;

            float scalingFactor = GetScalingFactor(screen);

            int captureWidth = (int)(screenBounds.Width * scalingFactor);
            int captureHeight = (int)(screenBounds.Height * scalingFactor);
            int captureX = (int)(screenBounds.X * scalingFactor);
            int captureY = (int)(screenBounds.Y * scalingFactor);
            captureRect = new Rectangle(captureX, captureY, captureWidth, captureHeight);
        }


        public Bitmap CaptureScreenGraphics()
        {
            Bitmap screenshot = new Bitmap(captureRect.Width, captureRect.Height);

            // Create a graphics object to draw the screenshot
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(captureRect.X, captureRect.Y, 0, 0, captureRect.Size, CopyPixelOperation.SourceCopy);
            }

            return screenshot;
        }

        public Bitmap CaptureScreenApi()
        {
            IntPtr desktopWindow = User32.GetDesktopWindow();
            IntPtr desktopDC = User32.GetDC(desktopWindow);

            IntPtr memoryDC = Gdi32.CreateCompatibleDC(desktopDC);
            IntPtr bitmap = Gdi32.CreateCompatibleBitmap(desktopDC, captureRect.Width, captureRect.Height);

            // Select the bitmap object into the memoryDC to perform the BitBlt.
            IntPtr oldBitmap = Gdi32.SelectObject(memoryDC, bitmap);

            // Use BitBlt to capture the screen content into the bitmap.
            Gdi32.BitBlt(memoryDC, 0, 0, captureRect.Width, captureRect.Height, desktopDC, captureRect.X, captureRect.Y, SourceCopyPixelOperation); // SRCCOPY

            // Restore the old bitmap and release resources.
            Gdi32.SelectObject(memoryDC, oldBitmap);

            Gdi32.DeleteObject(memoryDC);
            User32.ReleaseDC(desktopWindow, desktopDC);

            // Create a Bitmap from the captured screen content and return it.
            Bitmap resultBitmap = Image.FromHbitmap(bitmap);

            Gdi32.DeleteObject(bitmap);

            return resultBitmap;
        }


        private float GetScalingFactor(int screen)
        {
            Point point = new Point(Screen.AllScreens[screen].Bounds.Left + 100,
                Screen.AllScreens[screen].Bounds.Top + 100);

            //IntPtr hWnd = GetDesktopWindow();
            IntPtr hWnd = User32.WindowFromPoint(point);
            IntPtr hDC = User32.GetDC(hWnd);

            int dpiX = User32.GetDpiForWindow(hWnd);
            float scalingFactor = dpiX / defaultWindowsDPI;

            User32.ReleaseDC(hWnd, hDC);

            return scalingFactor;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}