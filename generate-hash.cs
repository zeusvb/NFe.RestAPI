using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string senha = "senha123";
        string hash = BCrypt.Net.BCrypt.HashPassword(senha);
        Console.WriteLine($"Senha: {senha}");
        Console.WriteLine($"Hash BCrypt: {hash}");
    }
}
