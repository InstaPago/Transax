namespace InstaTransfer.ITLogic
{
    public interface ICommand
    {
        void Execute();
        object Parameter { get; set; }
    }
}
