using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Workout.Messages
{
    public class ToggleNavMenuVisibilityMessage : ValueChangedMessage<bool>
    {
        public ToggleNavMenuVisibilityMessage(bool isVisible) : base(isVisible)
        {
        }
    }
}
