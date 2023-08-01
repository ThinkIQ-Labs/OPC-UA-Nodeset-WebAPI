// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;

Console.WriteLine("Hello, World!");





string Undefined_var = "";
bool boola = String.IsNullOrWhiteSpace(Undefined_var);
bool asdf = string.IsNullOrEmpty(Undefined_var);



stuff s1;

if (true)
{
    s1 = new stuff("asdf");
} else
{
    s1 = new stuff { name = "dfkjghor" };
}


Console.WriteLine("all done.");
Console.ReadLine();


public class stuff
{
    public string name;
    public stuff(string s)
    {
        name = s;
    }

    public stuff()
    {

    }
}
