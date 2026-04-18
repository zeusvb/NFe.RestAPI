using BCrypt.Net;

string senha = "senha123";
string hash = BCrypt.Net.BCrypt.HashPassword(senha);
Console.WriteLine($"Senha: {senha}");
Console.WriteLine($"Hash BCrypt: {hash}");
Console.WriteLine($"\nCópia para teste:");
Console.WriteLine(hash);
