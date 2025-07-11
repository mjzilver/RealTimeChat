namespace RealTimeServerServer.Websockets.Interfaces;

public interface ICommandProcessor
{
    Task ProcessCommand(string socketId, string command, string data);
}