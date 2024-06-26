using System.Reflection;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Serilog;

public class ChatReader
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    // Retrieve client and CommandService instance via ctor
    public ChatReader(DiscordSocketClient client, CommandService commands)
    {
        _commands = commands;
        _client = client;
    }
    
    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        _client.MessageReceived += HandleCommandAsync;

        // Here we discover all of the command modules in the entry 
        // assembly and load them. Starting from Discord.NET 2.0, a
        // service provider is required to be passed into the
        // module registration method to inject the 
        // required dependencies.
        //
        // If you do not use Dependency Injection, pass null.
        // See Dependency Injection guide for more information.
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), 
                                        services: null);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        ulong ChannelId = messageParam.Channel.Id;

        Serilog.Log.Debug("Received!");
        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (message.Author.IsBot)
        {
            return;
        }
        else if (Globals._shelfName.Keys.Contains(ChannelId))
        {
            Serilog.Log.Debug(messageParam.Content);
            // await context.Channel.SendMessageAsync($"Received message \"{messageParam.Content}\".");
            ulong Source = context.Channel.Id;
            string[] Inputs = messageParam.Content.Split('\n');
            if (Inputs.Count() == 1) 
            {
                await context.Channel.SendMessageAsync(Globals._inputHandler.HandleInput(messageParam.Content, Source, _client, messageParam.Author));
            }
            else
            {
                Serilog.Log.Debug($"{Inputs.Count()}");
                foreach (string Input in Inputs)
                {
                    Globals._inputBuffer[Source].Enqueue(Input);
                }
                await context.Channel.SendMessageAsync(Globals._commands.Next(Source));
            }
        }
    }
}