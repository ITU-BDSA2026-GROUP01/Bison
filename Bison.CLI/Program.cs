
using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;


class Program {

    static void Main(string[] args)
    {
        if (args.Length > 0) {
            if (args[0] == "observe") {
                if (args.Length > 1) {
                    observe(args[1]); 
                } else {
                    Console.WriteLine("No message is provided");
                }
                
            } else if (args[0] == "read") {
                read();
            }
        } else {
            read();
        }
    }

    static void read() {
    var file = "/home/timhl/Projects/Bison/bison_observe_cli_db.csv";

    // Source - https://stackoverflow.com/a/8319246
    var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(file);
    parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
    parser.SetDelimiters(new string[] { "," });

    parser.ReadFields();
    string author;
    string message;
    DateTimeOffset time;

    while (!parser.EndOfData) {
        string[] row = parser.ReadFields();
        
        author = row[0];
        message = row[1];
        time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(row[2]));
        DateTimeOffset localTime = time.ToLocalTime();

        var SB = new StringBuilder();

        SB.AppendFormat("{0} @ ", author);
        SB.Append(localTime.ToString());
        SB.AppendFormat(": {0}", message);

        Console.WriteLine(SB.ToString());
        }
    }

    static void observe(string message) {
    
        long time = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();
        string author = Environment.UserName;
        
        StringBuilder SB = new StringBuilder();
        string csvappend = $"{author},\"{message}\",{time}";
        //String.Format("{0},{1},{2}", author, message, time);

        var file = "/home/timhl/Projects/Bison/bison_observe_cli_db.csv";
        var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(file);

        ArrayList csvLines = new ArrayList();
        while (!parser.EndOfData) {
            csvLines.Add(parser.ReadLine());
        }
        csvLines.Add(csvappend);

        using (StreamWriter outputFile = new StreamWriter(Path.Combine(file)))
        {
            foreach (string line in csvLines)
                outputFile.WriteLine(line);
        }

        
    }
}
