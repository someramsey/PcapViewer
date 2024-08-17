namespace Gui;

public static class Drawer {
    public static void Draw() {
        DrawTop();
        DrawContent();
        DrawBottom();
    }

    private static void DrawContent() {
               
    }
    
    private static void DrawTop() {
        Console.BackgroundColor = ConsoleColor.Gray;
        FillBackground();

        const string msg = "Something";

        Console.SetCursorPosition(Console.BufferWidth / 2 - msg.Length / 2, Console.CursorTop);
        Console.Write(msg);
        Console.SetCursorPosition(0, Console.CursorTop + 1);

        Console.ResetColor();
    }

    private static void DrawBottom() {
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.Write(new string(' ', Console.BufferWidth));
        Console.ResetColor();
    }
    
    private static void FillBackground() {
        Console.Write(new string(' ', Console.BufferWidth));
    }
}