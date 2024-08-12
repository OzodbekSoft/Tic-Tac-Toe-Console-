using Spectre.Console;
using Spectre.Console.Rendering;

public class Game
{
    private string[][] matrix;
    private bool computersTurn;
    private bool isUserX;
    private string winner;
    private int boxWidth = 9;
    private int boxHeight = 5;

    private string userChar;
    private string computerChar;

    public Game()
    {
        matrix = new string[][]
        {
            new string[] { "", "", "" },
            new string[] { "", "", "" },
            new string[] { "", "", "" }
        };
        computersTurn = false;
        isUserX = new Random().Next() % 2 == 0;
        winner = string.Empty;
        userChar = isUserX ? "❌" : "⭕";
        computerChar = isUserX ? "⭕" : "❌";
    }

    public void Start()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        AnsiConsole.MarkupLine("Biron bir katakchani tanlash uchun, o'sha katakchani tepasidagi [green]raqamni[/] kiriting [gray](1-9)[/]\n");
        AnsiConsole.MarkupLine("O'yinni to'xtatish uchun esa [green]`q`[/] harfini kiritish yoki [green]`CTRL+C`[/] klavish kombinatsi bosish kerak\n");
        AnsiConsole.MarkupLine("O'yinni boshlash uchun istalgan klaviatura tugmasini bosing");
        Console.ReadKey();

        string inputBox = "";

        do
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold green]Computer[/] {computerChar} ni o'ynaydi, [bold green]Siz[/] {userChar} ni o'ynaysiz.");
            Console.WriteLine();
            AnsiConsole.WriteLine(computersTurn ? "🤖 Computerni naxbati" : "🫵 ning navbatiz");
            Console.WriteLine();

            ShowGrid();

            if (!computersTurn)
            {
                inputBox = AnsiConsole.Ask<string>($"Qaysi katakchaga {userChar} ni joylamoqchisiz?[gray](1-9)[/]:");
                UpdateGrid(inputBox, userChar);
                computersTurn = true;
            }

            CheckWinner();

            if (computersTurn && winner == string.Empty)
                Task.Run(() => ComputerMoves()).Wait();

            if (IsFinished())
            {
                DisplayResult();
                break;
            }

        } while (inputBox.Trim().ToLower() != "q");
    }

    private void ShowGrid()
    {
        int boxNumber = 1;
        var grid = new Grid();
        grid.AddColumns(3);

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            var rowContent = new IRenderable[3];
            for (int j = 0; j < matrix[i].Length; j++)
            {
                var text = new Text(matrix[i][j], new Style(Color.White)).Centered();

                var panel = new Panel(text)
                {
                    Padding = new Padding(),
                    Width = boxWidth,
                    Height = boxHeight,
                    Header = new PanelHeader($"{boxNumber}", Justify.Center),
                    Border = BoxBorder.Rounded
                };
                rowContent[j] = panel;
                boxNumber++;
            }
            grid.AddRow(rowContent);
        }
        AnsiConsole.Write(grid);
        Console.WriteLine();
    }

    private void UpdateGrid(string position, string content)
    {
        int index = int.Parse(position) - 1;
        int row = index / 3;
        int column = index % 3;

        if (matrix[row][column] == string.Empty)
        {
            matrix[row][column] = content;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Bu katakcha bo'sh emas, boshqasini tanlang![/]");
            position = AnsiConsole.Ask<string>($"Qaysi katakchaga {userChar} ni joylamoqchisiz?[gray](1-9)[/]:");
            UpdateGrid(position, content);
        }
    }

    private void CheckWinner()
    {
        for (int row = 0; row < 3; row++)
        {
            if (matrix[row].All(x => x == "❌"))
            {
                winner = "❌";
                return;
            }
            else if (matrix[row].All(x => x == "⭕"))
            {
                winner = "⭕";
                return;
            }
        }

        for (int column = 0; column < 3; column++)
        {
            var columnValues = matrix.Select(row => row[column]).ToArray();
            if (columnValues.All(x => x == "❌"))
            {
                winner = "❌";
                return;
            }
            else if (columnValues.All(x => x == "⭕"))
            {
                winner = "⭕";
                return;
            }
        }

        var leftDiagonal = matrix.Select((_, i) => matrix[i][i]).ToArray();
        if (leftDiagonal.All(x => x == "❌"))
        {
            winner = "❌";
            return;
        }
        else if (leftDiagonal.All(x => x == "⭕"))
        {
            winner = "⭕";
            return;
        }

        var rightDiagonal = matrix.Select((_, i) => matrix[i][2 - i]).ToArray();
        if (rightDiagonal.All(x => x == "❌"))
        {
            winner = "❌";
            return;
        }
        else if (rightDiagonal.All(x => x == "⭕"))
        {
            winner = "⭕";
            return;
        }
    }

    private bool IsFinished()
    {
        return matrix.All(row => row.All(x => x != string.Empty)) || !string.IsNullOrWhiteSpace(winner);
    }

    private void DisplayResult()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold green]Computer[/] {computerChar} ni o'ynaydi, [bold green]Siz[/] {userChar} ni o'ynaysiz.");
        ShowGrid();

        if (string.IsNullOrEmpty(winner))
        {
            AnsiConsole.MarkupLine("[bold green]Do'stlik g'alaba qozondi![/]");
        }
        else
        {
            if (winner == userChar)
                AnsiConsole.MarkupLine($"[bold green]Siz yutdiz!!![/]");
            else
                AnsiConsole.MarkupLine($"[bold green]Computer yutdi!!![/]");
        }
    }

    private async Task ComputerMoves()
    {
        await Task.Delay(1000);

        var emptyCells = matrix
            .SelectMany((row, rowIndex) => row.Select((cell, columnIndex) => new { rowIndex, columnIndex, cell }))
            .Where(x => x.cell == string.Empty)
            .ToList();

        if (emptyCells.Any())
        {
            var randomCell = emptyCells.OrderBy(_ => Guid.NewGuid()).First();
            matrix[randomCell.rowIndex][randomCell.columnIndex] = computerChar;
        }

        computersTurn = false;
        CheckWinner();
    }
}
