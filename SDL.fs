/// Wrappers and PInvoke of SDL2
module SDL

open System.Runtime.InteropServices

type SDL_WindowFlags =
| SDL_WINDOW_FULLSCREEN = 0x00000001
| SDL_WINDOW_OPENGL = 0x00000002
| SDL_WINDOW_SHOWN = 0x00000004

[<DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)>]
extern int SDL_CreateWindowAndRenderer (int, int, SDL_WindowFlags, int&, int&)