﻿
namespace EasyGames.Services;

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string FromName { get; set; } = "EasyGames";
    public string FromEmail { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
