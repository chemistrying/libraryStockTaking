public class Commands
{
    public Commands()
    {

    }

    public void Del(int Args)
    {   
        if (Args == 0)
        {
            // Remove last book
            if (Globals._buffer.Count == 0)
            {
                Console.Beep();
                Console.WriteLine("There is nothing for you to delete.");
            }
            else
            {
                Globals._buffer.RemoveAt(Globals._buffer.Count - 1);
            }
        }
        else if (Args > 0)
        {   
            // Remove last x books
            Globals._buffer.RemoveRange(Math.Min(0, Globals._buffer.Count - 1 - Args), Math.Max(Globals._buffer.Count, Args));
        }
        else
        {
            // Remove a specific position book
            if (Globals._buffer.Count + Args < 0)
            {
                Console.Beep();
                Console.WriteLine("Index out of bound.");
            }
            else
            {
                Globals._buffer.RemoveAt(Globals._buffer.Count + Args);
            }
        }
    }

    public void Save(string Args)
    {
        string fileLocation = Args == "" ? Globals._currentFileLocation : Args.Substring(0, Args.IndexOf(' ') == -1 ? Args.Length : Args.IndexOf(' '));
        string furtherArgs = Args.IndexOf(' ') == -1 ? "" : Args.Substring(Args.IndexOf(' ') + 1);
        if (furtherArgs != "-y")
        {
            string Confirm = "";
            do
            {
                Console.Write($"Confirm saving into \"{fileLocation}.txt\" this file? [Y|N] ");
                Confirm = Console.ReadLine();
            } while (Confirm != "Y" && Confirm != "N");
            if (Confirm == "N")
            {
                Console.WriteLine("Save operation cancelled.");
                return;
            }
        }
        
        File.AppendAllText(fileLocation + ".txt", string.Join("\n", Globals._buffer) + "\n");
        Globals._buffer.Clear();
        Console.WriteLine($"Successfully saved to \"{fileLocation}.txt\".");
    }

    public void Help(string Args)
    {
        bool Basic = Args.ToLower() == "basic";
        bool Advanced = Args.ToLower() == "advanced";
        Console.WriteLine(File.ReadAllText("help.txt"));
        if (Basic || !(Basic | Advanced))
        {
            Console.WriteLine(File.ReadAllText("basic.txt"));
        }
        if (Advanced || !(Basic | Advanced))
        {
            Console.WriteLine(File.ReadAllText("advanced.txt"));
        }
    }

    public void Count(string Args)
    {
        string fileLocation = Args == "" ? Globals._currentFileLocation : Args.Substring(0, Args.IndexOf(' ') == -1 ? Args.Length : Args.IndexOf(' '));
        // string furtherArgs = Args.IndexOf(' ') == -1 ? "" : Args.Substring(Args.IndexOf(' ') + 1);
        
        using (StreamReader sr = new StreamReader(fileLocation + ".txt"))
        {
            int cnt = 0;
            while (!sr.EndOfStream)
            {
                sr.ReadLine();
                cnt++;
            }
            Console.WriteLine($"Number of books in \"{fileLocation}.txt\": {cnt}");
        }
    }

    public int Position(string Barcode)
    {
        int l = 0, r = Globals._booklist.Count - 1;
        while (l <= r)
        {
            int m = (l + r) >> 1;
            if (Globals._booklist[m] == Barcode) break;
            else if (String.Compare(Globals._booklist[m], Barcode) < 0) l = m + 1;
            else r = m - 1;
        }
        return l > r ? -1 : (l + r) >> 1;
    }

    public void Check(string Args)
    {
        string fileLocation = Args == "" ? Globals._currentFileLocation : Args.Substring(0, Args.IndexOf(' ') == -1 ? Args.Length : Args.IndexOf(' '));
        try
        {
            long StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            using (StreamReader sr = new StreamReader(fileLocation + ".txt"))
            {
                while (!sr.EndOfStream)
                {
                    string Barcode = sr.ReadLine();
                    int Pos = Position(Barcode);
                    if (Pos == -1)
                    {
                        Console.WriteLine($"WARNING: BOOK CODE {Barcode} NOT FOUND IN THE BOOKLIST.");
                    }
                    else if (Globals._config.ShowAllBooksPosition)
                    {
                        Console.WriteLine($"Found Book {Barcode} in booklist position of {Pos}.");
                    }
                }
            }
            long EndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Console.WriteLine($"Successfully check all the books in \"{fileLocation}.txt\" in {EndTime / 1000.0 - StartTime / 1000.0} second(s).");
        }
        catch
        {
            Console.WriteLine("File location is not valid.");
        }
    }
    
    public void ReloadBooklist(string Args)
    {
        // clear the booklist
        Globals._booklist.Clear();
        string fileLocation = Args;
        try
        {
            using (StreamReader sr = new StreamReader(fileLocation + ".txt"))
            {
                while (!sr.EndOfStream){
                    string Book = sr.ReadLine();
                    Globals._booklist.Add(Book.Substring(0, Book.IndexOf('|')));
                }
            }
            Globals._booklist.Sort();
        }
        catch
        {
            Console.WriteLine("Booklist file location must be given in order to work.");
            return;
        }
    }

    public void ReloadConfig()
    {
        dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText("config.json"));
        Globals._config = json.ToObject(typeof(Config));
    }

    public void Config(string Args)
    {
        int Pos = Args.IndexOf(' ');
        string ConfigName = Pos == -1 ? Args : Args.Substring(0, Pos);
        if (!Globals._config.configs.Contains(ConfigName))
        {
            Console.WriteLine("Configuration Name is wrong. Please check if there are any spelling mistakes.");
        }
        else
        {
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText("config.json"));
            Dictionary<string, dynamic> NewConfig = json.ToObject(typeof(Dictionary<string, dynamic>));
            if (Pos == -1)
            {
                Console.WriteLine($"{ConfigName}: {NewConfig[ConfigName]}");
            }
        }
    }

    public void Quit()
    {
        Globals._running = false;
    }
}