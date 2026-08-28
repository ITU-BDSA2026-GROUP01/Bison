using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;


var file = "/home/timhl/Projects/Bison/bison_observe_cli_db.csv";

// Source - https://stackoverflow.com/a/8319246
var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(file);
parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
parser.SetDelimiters(new string[] { "," });

parser.ReadFields();
string author;
string message;
DateTimeOffset time;
while (!parser.EndOfData) 
{
    string[] row = parser.ReadFields();
    
    author = row[0];
    message = row[1];
    time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(row[2]));

    Console.WriteLine(time);
}
