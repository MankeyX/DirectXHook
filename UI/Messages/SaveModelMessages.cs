using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Models;

namespace UI.Messages
{
    /// <summary>
    /// Sent when the user confirms saving a model in the save model window.
    /// </summary>
    public class SaveModelMessage : ValueChangedMessage<ModelInfo>
    {
        public SaveModelMessage(ModelInfo model) : base(model)
        {
        }
    }

    /// <summary>
    /// Sent to close the save model window.
    /// </summary>
    public class CloseSaveModelWindowMessage
    {
    }
}
