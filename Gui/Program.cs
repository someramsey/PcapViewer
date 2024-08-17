using Gui;


Console.Clear();

int lastBuffSize = 0;
int selected = 0;

while (true) {
    if (lastBuffSize != Console.BufferWidth) {
        Console.Clear();
        Drawer.Draw();

        lastBuffSize = Console.BufferWidth;
    }

    Input();
}

void Input() {
    if (Console.KeyAvailable) {
        var keyInfo = Console.ReadKey();

        switch (keyInfo.Key) {
            case ConsoleKey.UpArrow: {
                if (selected > 0) {
                    selected--;
                }
                
                
                break;
            }

            case ConsoleKey.DownArrow: {
                selected++;
                
                
                break;
            }
        }
    }
}